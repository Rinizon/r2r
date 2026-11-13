using System;
using UnityEngine;

namespace InfrastructureTycoon.Content
{
    [CreateAssetMenu(menuName = "Infrastructure Tycoon/Upgrade Definition")]
    public sealed class UpgradeDefinition : ScriptableObject
    {
        [SerializeField] private string contentId;
        [SerializeField] private string displayName;
        [SerializeField] private string targetHardwareId;
        [SerializeField, Tooltip("One-time price in whole dollars.")] private int priceDollars;
        [SerializeField] private ResourceDefinition capacityDelta = new ResourceDefinition();

        public UpgradeData ToRuntime()
        {
            if (capacityDelta == null) throw new ArgumentException("Upgrade requires a capacity delta.");
            return new UpgradeData(contentId, displayName, targetHardwareId, priceDollars, capacityDelta.ToRuntime());
        }
    }
}
