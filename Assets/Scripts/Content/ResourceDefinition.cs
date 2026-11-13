using System;
using UnityEngine;

namespace InfrastructureTycoon.Content
{
    [Serializable]
    public sealed class ResourceDefinition
    {
        [SerializeField] private double cpuCores;
        [SerializeField] private double ramGb;
        [SerializeField] private double storageGb;
        [SerializeField] private double bandwidthMbps;

        public ResourceAmounts ToRuntime() => new ResourceAmounts(cpuCores, ramGb, storageGb, bandwidthMbps);
    }
}
