using System;
using UnityEngine;

namespace InfrastructureTycoon.Content
{
    [CreateAssetMenu(menuName = "Infrastructure Tycoon/Starter Catalog")]
    public sealed class StarterCatalog : ScriptableObject
    {
        [SerializeField] private HardwareDefinition starterPc;
        [SerializeField] private ContractTemplate[] websiteOffers = Array.Empty<ContractTemplate>();
        [SerializeField] private UpgradeDefinition[] upgrades = Array.Empty<UpgradeDefinition>();
        [SerializeField] private GrowthMilestoneDefinition[] milestones = Array.Empty<GrowthMilestoneDefinition>();

        public HostingCatalog ToRuntime()
        {
            if (starterPc == null || websiteOffers == null)
                throw new ArgumentException("Starter catalog requires hardware and an offer list.");
            var offers = new OfferData[websiteOffers.Length];
            for (int i = 0; i < offers.Length; i++)
            {
                if (websiteOffers[i] == null)
                    throw new ArgumentException("Starter catalog contains a missing offer asset.");
                offers[i] = websiteOffers[i].ToRuntime();
            }
            if (upgrades == null || milestones == null) throw new ArgumentException("Catalog requires progression lists.");
            var runtimeUpgrades = new UpgradeData[upgrades.Length];
            for (int i = 0; i < upgrades.Length; i++)
            {
                if (upgrades[i] == null) throw new ArgumentException("Catalog contains a missing upgrade asset.");
                runtimeUpgrades[i] = upgrades[i].ToRuntime();
            }
            var runtimeMilestones = new GrowthMilestoneData[milestones.Length];
            for (int i = 0; i < milestones.Length; i++)
            {
                if (milestones[i] == null) throw new ArgumentException("Catalog contains a missing milestone asset.");
                runtimeMilestones[i] = milestones[i].ToRuntime();
            }
            return new HostingCatalog(starterPc.ToRuntime(), offers, runtimeUpgrades, runtimeMilestones);
        }
    }
}
