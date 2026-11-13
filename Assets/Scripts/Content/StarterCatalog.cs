using System;
using UnityEngine;

namespace InfrastructureTycoon.Content
{
    [CreateAssetMenu(menuName = "Infrastructure Tycoon/Starter Catalog")]
    public sealed class StarterCatalog : ScriptableObject
    {
        [SerializeField] private HardwareDefinition starterPc;
        [SerializeField] private ContractTemplate[] websiteOffers = Array.Empty<ContractTemplate>();

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
            return new HostingCatalog(starterPc.ToRuntime(), offers);
        }
    }
}
