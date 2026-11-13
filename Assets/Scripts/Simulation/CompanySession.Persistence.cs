using System;
using System.Collections.Generic;
using System.Globalization;
using InfrastructureTycoon.Content;

namespace InfrastructureTycoon.Simulation
{
    public sealed partial class CompanySession
    {
        internal static CompanySession Restore(HostingCatalog catalog, SessionSaveData data)
        {
            if (data == null) throw new ArgumentException("Save data is missing.");
            if (data.SaveVersion != 1 && data.SaveVersion != SessionSaveJson.CurrentVersion) throw new ArgumentException("Unsupported save version.");
            if (string.IsNullOrWhiteSpace(data.GameVersion) ||
                !DateTimeOffset.TryParseExact(data.Timestamp, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var timestamp) ||
                timestamp.Offset != TimeSpan.Zero)
                throw new ArgumentException("Save metadata is invalid.");
            return new CompanySession(catalog, data);
        }

        private CompanySession(HostingCatalog catalog, SessionSaveData data) : this(catalog)
        {
            var date = SessionSaveJson.ParseDate(data.Date);
            Clock.RestoreState(date, data.DayProgress, (SimulationSpeed)data.Speed);
            if (data.Hardware == null || data.Hardware.Length != 1 || data.Hardware[0] == null ||
                data.Hardware[0].ContentId != catalog.StarterPc.Id || data.Contracts == null || data.BillingHistory == null)
                throw new ArgumentException("Missing or unknown Phase 2 definitions.");

            var ids = new HashSet<string>(StringComparer.Ordinal);
            AddRuntimeId(ids, data.Hardware[0].Id, data.Hardware[0].ContentId);
            bool legacy = data.SaveVersion == 1;
            var savedHardware = data.Hardware[0];
            if ((!legacy && savedHardware.Purchases == null) || (legacy && savedHardware.Purchases != null && savedHardware.Purchases.Length != 0))
                throw new ArgumentException("Invalid purchase state for the save version.");
            var purchases = new List<UpgradePurchase>();
            decimal spending = 0;
            foreach (var purchase in savedHardware.Purchases ?? Array.Empty<UpgradePurchaseSaveData>())
            {
                if (purchase == null) throw new ArgumentException("Missing purchase record.");
                purchases.Add(new UpgradePurchase(purchase.HardwareId, purchase.UpgradeId, purchase.AmountPaid));
                spending += purchase.AmountPaid;
            }
            Hardware = Array.AsReadOnly(new[] { new HardwareInstance(catalog.StarterPc, savedHardware.Id, purchases, catalog.Upgrades) });
            var offers = new HashSet<string>(StringComparer.Ordinal);
            int completedMonths = (date.Year - 1995) * 12 + date.Month - 1;
            foreach (var saved in data.Contracts)
            {
                if (saved == null) throw new ArgumentException("Missing contract data.");
                var offer = FindOffer(saved.ContentId);
                if (offer == null || !offers.Add(offer.Id)) throw new ArgumentException("Unknown or duplicate offer content ID.");
                AddRuntimeId(ids, saved.Id, offer.Id);
                if (saved.PaidMonths < 0 || saved.PaidMonths > completedMonths ||
                    (saved.AssignedHardwareId == null && saved.PaidMonths != 0))
                    throw new ArgumentException("Invalid paid-month count.");
                if ((!legacy && saved.AppliedMilestoneIds == null) || (legacy && saved.AppliedMilestoneIds != null && saved.AppliedMilestoneIds.Length != 0))
                    throw new ArgumentException("Invalid milestone state for the save version.");
                var milestones = new List<string>();
                if (legacy)
                {
                    // Phase 2 did not store milestones. Apply eligible profiles once during migration,
                    // retaining paid-month counts and without advancing the clock or replaying billing.
                    foreach (var milestone in catalog.Milestones)
                        if (milestone.TargetOfferId == offer.Id && saved.PaidMonths >= milestone.RequiredPaidMonths)
                            milestones.Add(milestone.Id);
                }
                else milestones.AddRange(saved.AppliedMilestoneIds);
                if (saved.AssignedHardwareId != null && saved.AssignedHardwareId != Hardware[0].Id)
                    throw new ArgumentException("Dangling hardware assignment.");
                var contract = new ContractInstance(offer, saved.Id, saved.PaidMonths, milestones, catalog.Milestones, saved.AssignedHardwareId);
                acceptedContracts.Add(contract);
            }

            // Historical assignments may exceed capacity because an applied growth profile increased demand.
            // Their original base allocation must still fit; ordinary new allocations keep their capacity guard.
            var baseDemand = default(ResourceAmounts);
            foreach (var contract in acceptedContracts)
                if (contract.IsHosted) baseDemand += contract.BaseRequirements;
            var capacity = Hardware[0].EffectiveCapacity;
            if (baseDemand.CpuCores > capacity.CpuCores || baseDemand.RamGb > capacity.RamGb ||
                baseDemand.StorageGb > capacity.StorageGb || baseDemand.BandwidthMbps > capacity.BandwidthMbps)
                throw new ArgumentException("Base assignments exceed hardware capacity.");
            Hardware[0].SetUsedResources(ProgressionResources.CalculateUsage(Hardware[0], acceptedContracts).Used);

            // Require the complete chronological ledger; missing periods could otherwise replay payments.
            if (data.BillingHistory.Length != completedMonths) throw new ArgumentException("Incomplete billing history.");
            decimal profit = 0;
            for (int i = 0; i < data.BillingHistory.Length; i++)
            {
                var saved = data.BillingHistory[i];
                if (saved == null || saved.Revenue < 0 || saved.Expenses < 0) throw new ArgumentException("Invalid monthly result.");
                var period = SessionSaveJson.ParseDate(saved.PeriodEnd);
                if (period != new DateTime(1995, 2, 1).AddMonths(i).AddDays(-1))
                    throw new ArgumentException("Duplicate, missing, or unordered billing period.");
                var result = new MonthlyBillingResult(period, saved.Revenue, saved.Expenses);
                profit += result.NetProfit;
                billingHistory.Add(result);
                processedBillingPeriods.Add(period);
            }
            if (data.OperatingProfit != profit || data.Cash != 150m + profit - spending)
                throw new ArgumentException("Finances do not match billing history.");
            Cash = data.Cash;
            OperatingProfit = data.OperatingProfit;
            // Reconcile saves from the earlier state-only v2 build without replaying notifications or billing.
            ApplyEligibleMilestones(false);
        }

        private static void AddRuntimeId(HashSet<string> ids, string id, string contentId)
        {
            if (string.IsNullOrWhiteSpace(id) || id == contentId || !ids.Add(id))
                throw new ArgumentException("Missing, duplicate, or invalid runtime ID.");
        }
    }
}
