using System;
using System.Collections.Generic;
using InfrastructureTycoon.Content;

namespace InfrastructureTycoon.Simulation
{
    public sealed partial class CompanySession
    {
        public bool TryPurchaseUpgrade(string upgradeId, string hardwareId, out string reason)
        {
            UpgradeData upgrade = null;
            foreach (var definition in Catalog.Upgrades)
                if (definition.Id == upgradeId) { upgrade = definition; break; }
            if (upgrade == null)
            {
                reason = "Unknown upgrade.";
                return false;
            }
            HardwareInstance hardware = null;
            foreach (var instance in Hardware)
                if (instance.Id == hardwareId) { hardware = instance; break; }
            if (hardware == null)
            {
                reason = "Unknown hardware instance.";
                return false;
            }
            if (upgrade.TargetHardwareId != hardware.Definition.Id)
            {
                reason = "This upgrade is not compatible with that hardware.";
                return false;
            }
            if (hardware.AppliedUpgradeIds.Contains(upgrade.Id))
            {
                reason = "This upgrade has already been purchased.";
                return false;
            }
            if (Cash < upgrade.Price)
            {
                reason = "Not enough cash for this upgrade.";
                return false;
            }

            // Prepare the complete resource state before committing either capacity or money.
            HardwareInstance prepared;
            try
            {
                var records = new List<UpgradePurchase>(hardware.PurchaseRecords)
                {
                    new UpgradePurchase(hardware.Id, upgrade.Id, upgrade.Price)
                };
                prepared = new HardwareInstance(hardware.Definition, hardware.Id, records, Catalog.Upgrades);
                prepared.SetUsedResources(hardware.Resources.Used);
            }
            catch (ArgumentOutOfRangeException)
            {
                reason = "The resulting capacity cannot be represented.";
                return false;
            }
            decimal remainingCash = Cash - upgrade.Price;
            hardware.CommitProgression(prepared);
            Cash = remainingCash;
            reason = string.Empty;
            return true;
        }
    }
}
