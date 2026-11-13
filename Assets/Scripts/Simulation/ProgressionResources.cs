using System;
using System.Collections.Generic;
using InfrastructureTycoon.Content;

namespace InfrastructureTycoon.Simulation
{
    public sealed class UpgradePurchase
    {
        public string HardwareId { get; }
        public string UpgradeId { get; }
        public decimal AmountPaid { get; }

        public UpgradePurchase(string hardwareId, string upgradeId, decimal amountPaid)
        {
            HardwareId = ContentValidation.Text(hardwareId, nameof(hardwareId));
            UpgradeId = ContentValidation.Text(upgradeId, nameof(upgradeId));
            if (amountPaid <= 0) throw new ArgumentOutOfRangeException(nameof(amountPaid));
            AmountPaid = amountPaid;
        }
    }

    public static class ProgressionResources
    {
        public static ResourceAmounts CalculateCapacity(HardwareData hardware, string runtimeId,
            IEnumerable<UpgradePurchase> purchases, IEnumerable<UpgradeData> definitions)
        {
            if (hardware == null) throw new ArgumentNullException(nameof(hardware));
            ContentValidation.Text(runtimeId, nameof(runtimeId));
            if (purchases == null) throw new ArgumentNullException(nameof(purchases));
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));
            var registry = new Dictionary<string, UpgradeData>(StringComparer.Ordinal);
            foreach (var definition in definitions)
            {
                if (definition == null || registry.ContainsKey(definition.Id))
                    throw new ArgumentException("Missing or duplicate upgrade definition.", nameof(definitions));
                registry.Add(definition.Id, definition);
            }
            var applied = new HashSet<string>(StringComparer.Ordinal);
            var capacity = hardware.Capacity;
            foreach (var purchase in purchases)
            {
                if (purchase == null || purchase.HardwareId != runtimeId || !applied.Add(purchase.UpgradeId))
                    throw new ArgumentException("Missing, duplicated, or incorrectly targeted purchase.", nameof(purchases));
                if (!registry.TryGetValue(purchase.UpgradeId, out var upgrade) || upgrade.TargetHardwareId != hardware.Id)
                    throw new ArgumentException("Unknown or incorrectly targeted upgrade.", nameof(purchases));
                capacity += upgrade.CapacityDelta;
            }
            return capacity;
        }

        public static ResourceAmounts CalculateRequirements(OfferData offer, int paidMonths,
            IEnumerable<string> appliedIds, IEnumerable<GrowthMilestoneData> definitions)
        {
            if (offer == null) throw new ArgumentNullException(nameof(offer));
            if (paidMonths < 0) throw new ArgumentOutOfRangeException(nameof(paidMonths));
            if (appliedIds == null) throw new ArgumentNullException(nameof(appliedIds));
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));
            var registry = new Dictionary<string, GrowthMilestoneData>(StringComparer.Ordinal);
            foreach (var definition in definitions)
            {
                if (definition == null || registry.ContainsKey(definition.Id))
                    throw new ArgumentException("Missing or duplicate milestone definition.", nameof(definitions));
                registry.Add(definition.Id, definition);
            }
            var applied = new HashSet<string>(StringComparer.Ordinal);
            var thresholds = new HashSet<int>();
            GrowthMilestoneData latest = null;
            foreach (string id in appliedIds)
            {
                if (string.IsNullOrWhiteSpace(id) || !applied.Add(id) || !registry.TryGetValue(id, out var milestone))
                    throw new ArgumentException("Missing, unknown, or duplicate applied milestone.", nameof(appliedIds));
                if (milestone.TargetOfferId != offer.Id || paidMonths < milestone.RequiredPaidMonths)
                    throw new ArgumentException("Milestone has the wrong target or is not yet eligible.", nameof(appliedIds));
                if (!thresholds.Add(milestone.RequiredPaidMonths))
                    throw new ArgumentException("Applied workload profiles have ambiguous thresholds.", nameof(appliedIds));
                if (latest == null || milestone.RequiredPaidMonths > latest.RequiredPaidMonths) latest = milestone;
            }
            // Profiles are absolute requirements, not deltas. Eligibility alone never applies one.
            return latest == null ? offer.Requirements : latest.Requirements;
        }

        public static ResourceUsage CalculateUsage(HardwareInstance hardware, IEnumerable<ContractInstance> contracts)
        {
            if (hardware == null) throw new ArgumentNullException(nameof(hardware));
            if (contracts == null) throw new ArgumentNullException(nameof(contracts));
            var used = default(ResourceAmounts);
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var contract in contracts)
            {
                if (contract == null || !ids.Add(contract.Id))
                    throw new ArgumentException("Missing or duplicate contract instance.", nameof(contracts));
                if (contract.AssignedHardwareId == hardware.Id) used += contract.EffectiveRequirements;
            }
            return new ResourceUsage(hardware.EffectiveCapacity, used);
        }
    }
}
