using System;
using UnityEngine;

namespace InfrastructureTycoon.Content
{
    [CreateAssetMenu(menuName = "Infrastructure Tycoon/Contract Template")]
    public sealed class ContractTemplate : ScriptableObject
    {
        [SerializeField] private string contentId;
        [SerializeField] private string displayName;
        [SerializeField] private ResourceDefinition requirements = new ResourceDefinition();
        [SerializeField, Tooltip("Whole dollars per month; converted to decimal runtime money.")]
        private int monthlyRevenueDollars;

        public OfferData ToRuntime()
        {
            if (requirements == null)
                throw new ArgumentException("Contract requires resource values.");
            return new OfferData(contentId, displayName, requirements.ToRuntime(), monthlyRevenueDollars);
        }
    }
}
