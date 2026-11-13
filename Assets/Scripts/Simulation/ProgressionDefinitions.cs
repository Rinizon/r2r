using System;

namespace InfrastructureTycoon.Content
{
    public sealed class UpgradeData
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string TargetHardwareId { get; }
        public decimal Price { get; }
        public ResourceAmounts CapacityDelta { get; }

        public UpgradeData(string id, string displayName, string targetHardwareId, decimal price, ResourceAmounts delta)
        {
            Id = ContentValidation.Text(id, nameof(id));
            DisplayName = ContentValidation.Text(displayName, nameof(displayName));
            TargetHardwareId = ContentValidation.Text(targetHardwareId, nameof(targetHardwareId));
            if (price <= 0) throw new ArgumentOutOfRangeException(nameof(price), "Upgrade price must be positive.");
            if (delta.CpuCores == 0 && delta.RamGb == 0 && delta.StorageGb == 0 && delta.BandwidthMbps == 0)
                throw new ArgumentException("Upgrade must increase at least one resource.", nameof(delta));
            Price = price;
            CapacityDelta = delta;
        }
    }

    public sealed class GrowthMilestoneData
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string TargetOfferId { get; }
        public int RequiredPaidMonths { get; }
        public ResourceAmounts Requirements { get; }

        public GrowthMilestoneData(string id, string displayName, string targetOfferId, int requiredPaidMonths, ResourceAmounts requirements)
        {
            Id = ContentValidation.Text(id, nameof(id));
            DisplayName = ContentValidation.Text(displayName, nameof(displayName));
            TargetOfferId = ContentValidation.Text(targetOfferId, nameof(targetOfferId));
            if (requiredPaidMonths <= 0) throw new ArgumentOutOfRangeException(nameof(requiredPaidMonths), "Paid-month threshold must be positive.");
            RequiredPaidMonths = requiredPaidMonths;
            Requirements = requirements;
        }
    }
}
