using System;
using System.Collections.ObjectModel;
using InfrastructureTycoon.Content;

namespace InfrastructureTycoon.Simulation
{
    public sealed class CompanySession
    {
        public HostingCatalog Catalog { get; }
        public SimulationClock Clock { get; } = new SimulationClock();
        public decimal Cash { get; private set; }
        public decimal OperatingProfit { get; private set; }
        public ReadOnlyCollection<HardwareInstance> Hardware { get; }
        public ReadOnlyCollection<ContractInstance> AcceptedContracts { get; }

        public CompanySession(HostingCatalog catalog)
        {
            Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            Cash = 150m;
            Hardware = Array.AsReadOnly(new[] { new HardwareInstance(catalog.StarterPc) });
            AcceptedContracts = Array.AsReadOnly(Array.Empty<ContractInstance>());
        }
    }

    public sealed class HardwareInstance
    {
        public string Id { get; } = Guid.NewGuid().ToString("N");
        public HardwareData Definition { get; }
        public ResourceUsage Resources { get; }

        public HardwareInstance(HardwareData definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Resources = new ResourceUsage(definition.Capacity, default);
        }
    }

    public sealed class ContractInstance
    {
        public string Id { get; } = Guid.NewGuid().ToString("N");
        public OfferData Definition { get; }

        internal ContractInstance(OfferData definition) { Definition = definition; }
    }

    public readonly struct ResourceUsage
    {
        public ResourceAmounts Capacity { get; }
        public ResourceAmounts Used { get; }
        // Available headroom never goes negative; Used still retains actual demand.
        public ResourceAmounts Available => new ResourceAmounts(
            Math.Max(0, Capacity.CpuCores - Used.CpuCores),
            Math.Max(0, Capacity.RamGb - Used.RamGb),
            Math.Max(0, Capacity.StorageGb - Used.StorageGb),
            Math.Max(0, Capacity.BandwidthMbps - Used.BandwidthMbps));

        public ResourceUsage(ResourceAmounts capacity, ResourceAmounts used)
        {
            Capacity = capacity;
            Used = used;
        }
    }
}
