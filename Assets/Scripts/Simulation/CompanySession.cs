using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using InfrastructureTycoon.Content;

namespace InfrastructureTycoon.Simulation
{
    public sealed partial class CompanySession
    {
        private readonly List<ContractInstance> acceptedContracts = new List<ContractInstance>();
        private readonly List<MonthlyBillingResult> billingHistory = new List<MonthlyBillingResult>();
        private readonly HashSet<DateTime> processedBillingPeriods = new HashSet<DateTime>();
        private readonly DateTime billingStartDate;
        public ReadOnlyCollection<MonthlyBillingResult> BillingHistory { get; }
        public MonthlyBillingResult LastMonthlyResult => billingHistory.Count == 0 ? null : billingHistory[billingHistory.Count - 1];
        public event Action<ContractInstance, GrowthMilestoneData> MilestoneApplied;

        public bool TryBillMonth(DateTime outgoingMonthEnd, out string reason)
        {
            if (processedBillingPeriods.Contains(outgoingMonthEnd))
            {
                reason = "This month has already been billed.";
                return false;
            }
            if (outgoingMonthEnd.TimeOfDay != TimeSpan.Zero ||
                outgoingMonthEnd.Day != DateTime.DaysInMonth(outgoingMonthEnd.Year, outgoingMonthEnd.Month) ||
                outgoingMonthEnd < billingStartDate || outgoingMonthEnd >= Clock.Date)
            {
                reason = "Billing requires a completed calendar month from this session.";
                return false;
            }

            decimal revenue = 0;
            foreach (var contract in acceptedContracts)
                if (contract.IsHosted)
                    revenue += contract.Definition.MonthlyRevenue;
            decimal expenses = 0;
            foreach (var hardware in Hardware)
                expenses += hardware.Definition.MonthlyExpense;
            var result = new MonthlyBillingResult(outgoingMonthEnd, revenue, expenses);
            // Compute finances before committing the billing period or any counters.
            decimal cash = Cash + result.NetProfit;
            decimal profit = OperatingProfit + result.NetProfit;

            Cash = cash;
            OperatingProfit = profit;
            foreach (var contract in acceptedContracts)
                if (contract.IsHosted)
                    contract.PaidMonths++;
            billingHistory.Add(result);
            processedBillingPeriods.Add(outgoingMonthEnd);
            ApplyEligibleMilestones(true);
            reason = string.Empty;
            return true;
        }
        public HostingCatalog Catalog { get; }
        public SimulationClock Clock { get; } = new SimulationClock();
        public decimal Cash { get; private set; }
        public decimal OperatingProfit { get; private set; }
        public ReadOnlyCollection<HardwareInstance> Hardware { get; }
        public ReadOnlyCollection<ContractInstance> AcceptedContracts { get; }
        public ReadOnlyCollection<OfferData> AvailableOffers
        {
            get
            {
                var available = new List<OfferData>();
                foreach (var offer in Catalog.Offers)
                    if (!acceptedContracts.Exists(contract => contract.Definition.Id == offer.Id))
                        available.Add(offer);
                return available.AsReadOnly();
            }
        }

        public bool TryAcceptOffer(string offerId, out ContractInstance contract, out string reason)
        {
            contract = null;
            var offer = FindOffer(offerId);
            if (offer == null)
            {
                reason = "Unknown website offer.";
                return false;
            }
            if (acceptedContracts.Exists(accepted => accepted.Definition.Id == offerId))
            {
                reason = "This offer has already been accepted.";
                return false;
            }

            contract = new ContractInstance(offer);
            acceptedContracts.Add(contract);
            reason = string.Empty;
            return true;
        }

        public bool TryAssignContract(string contractId, string hardwareId, out string reason)
        {
            var contract = acceptedContracts.Find(accepted => accepted.Id == contractId);
            if (contract == null)
            {
                reason = "Unknown accepted contract.";
                return false;
            }
            if (contract.IsHosted)
            {
                reason = "This contract is already assigned to hardware.";
                return false;
            }
            HardwareInstance hardware = null;
            foreach (var candidate in Hardware)
                if (candidate.Id == hardwareId)
                {
                    hardware = candidate;
                    break;
                }
            if (hardware == null)
            {
                reason = "Unknown hardware instance.";
                return false;
            }

            var used = hardware.Resources.Used;
            var capacity = hardware.Resources.Capacity;
            var demand = contract.EffectiveRequirements;
            if (used.CpuCores + demand.CpuCores > capacity.CpuCores)
                reason = "Not enough CPU capacity.";
            else if (used.RamGb + demand.RamGb > capacity.RamGb)
                reason = "Not enough RAM capacity.";
            else if (used.StorageGb + demand.StorageGb > capacity.StorageGb)
                reason = "Not enough storage capacity.";
            else if (used.BandwidthMbps + demand.BandwidthMbps > capacity.BandwidthMbps)
                reason = "Not enough bandwidth capacity.";
            else
                reason = string.Empty;
            if (reason.Length != 0)
                return false;

            // Validate the entire allocation before changing either side of the assignment.
            var totalUsed = used + demand;
            hardware.SetUsedResources(totalUsed);
            contract.AssignedHardwareId = hardware.Id;
            return true;
        }

        public CompanySession(HostingCatalog catalog)
        {
            Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            Cash = 150m;
            Hardware = Array.AsReadOnly(new[] { new HardwareInstance(catalog.StarterPc) });
            AcceptedContracts = acceptedContracts.AsReadOnly();
            BillingHistory = billingHistory.AsReadOnly();
            billingStartDate = Clock.Date;
            Clock.MonthEnded += OnMonthEnded;
        }

        private void OnMonthEnded(DateTime outgoingMonthEnd) => TryBillMonth(outgoingMonthEnd, out _);

        private OfferData FindOffer(string offerId)
        {
            foreach (var offer in Catalog.Offers)
                if (offer.Id == offerId)
                    return offer;
            return null;
        }
    }

    public sealed class HardwareInstance
    {
        public string Id { get; }
        public HardwareData Definition { get; }
        public ResourceAmounts BaseCapacity => Definition.Capacity;
        public ResourceAmounts EffectiveCapacity { get; private set; }
        public ReadOnlyCollection<UpgradePurchase> PurchaseRecords { get; private set; }
        public ReadOnlyCollection<string> AppliedUpgradeIds { get; private set; }
        public ResourceUsage Resources { get; private set; }

        public HardwareInstance(HardwareData definition) : this(definition, Guid.NewGuid().ToString("N")) { }

        internal HardwareInstance(HardwareData definition, string id)
            : this(definition, id, Array.Empty<UpgradePurchase>(), Array.Empty<UpgradeData>()) { }

        public HardwareInstance(HardwareData definition, string id, IEnumerable<UpgradePurchase> purchases, IEnumerable<UpgradeData> upgrades)
        {
            Id = ContentValidation.Text(id, nameof(id));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            if (purchases == null) throw new ArgumentNullException(nameof(purchases));
            var records = new List<UpgradePurchase>(purchases);
            EffectiveCapacity = ProgressionResources.CalculateCapacity(definition, id, records, upgrades);
            PurchaseRecords = records.AsReadOnly();
            var appliedIds = new List<string>();
            foreach (var record in records) appliedIds.Add(record.UpgradeId);
            AppliedUpgradeIds = appliedIds.AsReadOnly();
            Resources = new ResourceUsage(EffectiveCapacity, default);
        }

        internal void SetUsedResources(ResourceAmounts used) => Resources = new ResourceUsage(EffectiveCapacity, used);

        internal void CommitProgression(HardwareInstance prepared)
        {
            if (prepared == null || prepared.Id != Id || !ReferenceEquals(prepared.Definition, Definition))
                throw new ArgumentException("Prepared state must belong to this hardware.", nameof(prepared));
            EffectiveCapacity = prepared.EffectiveCapacity;
            PurchaseRecords = prepared.PurchaseRecords;
            AppliedUpgradeIds = prepared.AppliedUpgradeIds;
            Resources = prepared.Resources;
        }
    }

    public sealed class ContractInstance
    {
        public string Id { get; }
        public OfferData Definition { get; }
        public ResourceAmounts BaseRequirements => Definition.Requirements;
        public ResourceAmounts EffectiveRequirements { get; private set; }
        public ReadOnlyCollection<string> AppliedMilestoneIds { get; private set; }
        public string AssignedHardwareId { get; internal set; }
        public bool IsHosted => AssignedHardwareId != null;
        public int PaidMonths { get; internal set; }

        internal ContractInstance(OfferData definition) : this(definition, Guid.NewGuid().ToString("N")) { }
        internal ContractInstance(OfferData definition, string id)
            : this(definition, id, 0, Array.Empty<string>(), Array.Empty<GrowthMilestoneData>()) { }

        public ContractInstance(OfferData definition, string id, int paidMonths, IEnumerable<string> appliedMilestoneIds,
            IEnumerable<GrowthMilestoneData> milestones, string assignedHardwareId = null)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Id = ContentValidation.Text(id, nameof(id));
            if (paidMonths < 0) throw new ArgumentOutOfRangeException(nameof(paidMonths));
            if (appliedMilestoneIds == null) throw new ArgumentNullException(nameof(appliedMilestoneIds));
            var applied = new List<string>(appliedMilestoneIds);
            EffectiveRequirements = ProgressionResources.CalculateRequirements(definition, paidMonths, applied, milestones);
            AppliedMilestoneIds = applied.AsReadOnly();
            PaidMonths = paidMonths;
            AssignedHardwareId = assignedHardwareId == null ? null : ContentValidation.Text(assignedHardwareId, nameof(assignedHardwareId));
        }

        internal void CommitMilestones(ContractInstance prepared)
        {
            if (prepared == null || prepared.Id != Id || !ReferenceEquals(prepared.Definition, Definition) ||
                prepared.PaidMonths != PaidMonths || prepared.AssignedHardwareId != AssignedHardwareId)
                throw new ArgumentException("Prepared state must belong to this contract.", nameof(prepared));
            EffectiveRequirements = prepared.EffectiveRequirements;
            AppliedMilestoneIds = prepared.AppliedMilestoneIds;
        }
    }

    public readonly struct ResourceUsage
    {
        public ResourceAmounts Capacity { get; }
        public ResourceAmounts Used { get; }
        // Available headroom never goes negative; Used still retains actual demand.
        public ResourceAmounts Available => new ResourceAmounts(
            Math.Max(0, Capacity.CpuCores - Used.CpuCores),
            Math.Max(0, Capacity.RamGb - Used.RamGb),
            Math.Max(0, Capacity.StorageGb - Used.StorageGb),
            Math.Max(0, Capacity.BandwidthMbps - Used.BandwidthMbps));

        public ResourceUsage(ResourceAmounts capacity, ResourceAmounts used)
        {
            Capacity = capacity;
            Used = used;
        }
    }
}
