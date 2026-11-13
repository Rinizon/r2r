using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace InfrastructureTycoon.Content
{
    public readonly struct ResourceAmounts
    {
        public double CpuCores { get; }
        public double RamGb { get; }
        public double StorageGb { get; }
        public double BandwidthMbps { get; }

        public static ResourceAmounts operator +(ResourceAmounts left, ResourceAmounts right) =>
            new ResourceAmounts(left.CpuCores + right.CpuCores, left.RamGb + right.RamGb,
                left.StorageGb + right.StorageGb, left.BandwidthMbps + right.BandwidthMbps);

        // Subtracting more than is present is rejected by the nonnegative amount contract.
        public static ResourceAmounts operator -(ResourceAmounts left, ResourceAmounts right) =>
            new ResourceAmounts(left.CpuCores - right.CpuCores, left.RamGb - right.RamGb,
                left.StorageGb - right.StorageGb, left.BandwidthMbps - right.BandwidthMbps);

        public ResourceAmounts(double cpuCores, double ramGb, double storageGb, double bandwidthMbps)
        {
            CpuCores = ContentValidation.Resource(cpuCores, nameof(cpuCores));
            RamGb = ContentValidation.Resource(ramGb, nameof(ramGb));
            StorageGb = ContentValidation.Resource(storageGb, nameof(storageGb));
            BandwidthMbps = ContentValidation.Resource(bandwidthMbps, nameof(bandwidthMbps));
        }
    }

    public sealed class HardwareData
    {
        public string Id { get; }
        public string DisplayName { get; }
        public ResourceAmounts Capacity { get; }
        public decimal MonthlyExpense { get; }

        public HardwareData(string id, string displayName, ResourceAmounts capacity, decimal monthlyExpense)
        {
            Id = ContentValidation.Text(id, nameof(id));
            DisplayName = ContentValidation.Text(displayName, nameof(displayName));
            Capacity = capacity;
            MonthlyExpense = ContentValidation.Money(monthlyExpense, nameof(monthlyExpense));
        }
    }

    public sealed class OfferData
    {
        public string Id { get; }
        public string DisplayName { get; }
        public ResourceAmounts Requirements { get; }
        public decimal MonthlyRevenue { get; }

        public OfferData(string id, string displayName, ResourceAmounts requirements, decimal monthlyRevenue)
        {
            Id = ContentValidation.Text(id, nameof(id));
            DisplayName = ContentValidation.Text(displayName, nameof(displayName));
            Requirements = requirements;
            MonthlyRevenue = ContentValidation.Money(monthlyRevenue, nameof(monthlyRevenue));
        }
    }

    public sealed class HostingCatalog
    {
        public HardwareData StarterPc { get; }
        public ReadOnlyCollection<OfferData> Offers { get; }
        public ReadOnlyCollection<UpgradeData> Upgrades { get; }
        public ReadOnlyCollection<GrowthMilestoneData> Milestones { get; }

        public HostingCatalog(HardwareData starterPc, IEnumerable<OfferData> offers)
            : this(starterPc, offers, Array.Empty<UpgradeData>(), Array.Empty<GrowthMilestoneData>()) { }

        public HostingCatalog(HardwareData starterPc, IEnumerable<OfferData> offers,
            IEnumerable<UpgradeData> upgrades, IEnumerable<GrowthMilestoneData> milestones)
        {
            StarterPc = starterPc ?? throw new ArgumentNullException(nameof(starterPc));
            if (offers == null)
                throw new ArgumentNullException(nameof(offers));
            var copy = new List<OfferData>();
            var ids = new HashSet<string>(StringComparer.Ordinal) { starterPc.Id };
            foreach (var offer in offers)
            {
                if (offer == null)
                    throw new ArgumentException("Catalog contains a missing offer.", nameof(offers));
                if (!ids.Add(offer.Id))
                    throw new ArgumentException("Duplicate content ID: " + offer.Id, nameof(offers));
                copy.Add(offer);
            }
            Offers = copy.AsReadOnly();
            if (upgrades == null) throw new ArgumentNullException(nameof(upgrades));
            if (milestones == null) throw new ArgumentNullException(nameof(milestones));
            var upgradeCopy = new List<UpgradeData>();
            foreach (var upgrade in upgrades)
            {
                if (upgrade == null) throw new ArgumentException("Catalog contains a missing upgrade.", nameof(upgrades));
                if (!ids.Add(upgrade.Id)) throw new ArgumentException("Duplicate content ID: " + upgrade.Id, nameof(upgrades));
                if (upgrade.TargetHardwareId != starterPc.Id) throw new ArgumentException("Unknown upgrade hardware target: " + upgrade.TargetHardwareId, nameof(upgrades));
                upgradeCopy.Add(upgrade);
            }
            var milestoneCopy = new List<GrowthMilestoneData>();
            foreach (var milestone in milestones)
            {
                if (milestone == null) throw new ArgumentException("Catalog contains a missing milestone.", nameof(milestones));
                if (!ids.Add(milestone.Id)) throw new ArgumentException("Duplicate content ID: " + milestone.Id, nameof(milestones));
                if (!copy.Exists(offer => offer.Id == milestone.TargetOfferId))
                    throw new ArgumentException("Unknown milestone offer target: " + milestone.TargetOfferId, nameof(milestones));
                milestoneCopy.Add(milestone);
            }
            Upgrades = upgradeCopy.AsReadOnly();
            Milestones = milestoneCopy.AsReadOnly();
        }
    }

    internal static class ContentValidation
    {
        internal static string Text(string value, string parameter)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Content ID and display name must not be blank.", parameter);
            return value;
        }

        internal static double Resource(double value, string parameter)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
                throw new ArgumentOutOfRangeException(parameter, "Resource values must be finite and nonnegative.");
            return value;
        }

        internal static decimal Money(decimal value, string parameter)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(parameter, "Money values must be nonnegative.");
            return value;
        }
    }
}
