using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace InfrastructureTycoon.Simulation
{
    public enum ResourceKind { Cpu, Ram, Storage, Bandwidth }

    public sealed class AffectedContract
    {
        public string Id { get; }
        public string ContentId { get; }
        public string DisplayName { get; }
        internal AffectedContract(ContractInstance contract)
        {
            Id = contract.Id;
            ContentId = contract.Definition.Id;
            DisplayName = contract.Definition.DisplayName;
        }
    }

    public sealed class ResourceBottleneck
    {
        public string HardwareId { get; }
        public string HardwareContentId { get; }
        public string HardwareName { get; }
        public ResourceKind Resource { get; }
        public string Unit { get; }
        public double Demand { get; }
        public double Capacity { get; }
        public double Shortfall => Demand - Capacity;
        public ReadOnlyCollection<AffectedContract> AffectedContracts { get; }

        internal ResourceBottleneck(HardwareInstance hardware, ResourceKind resource, string unit,
            double demand, double capacity, ReadOnlyCollection<AffectedContract> affected)
        {
            HardwareId = hardware.Id;
            HardwareContentId = hardware.Definition.Id;
            HardwareName = hardware.Definition.DisplayName;
            Resource = resource;
            Unit = unit;
            Demand = demand;
            Capacity = capacity;
            AffectedContracts = affected;
        }
    }

    public static class ResourceBottlenecks
    {
        public static ReadOnlyCollection<ResourceBottleneck> Diagnose(HardwareInstance hardware, IEnumerable<ContractInstance> contracts)
        {
            if (hardware == null) throw new ArgumentNullException(nameof(hardware));
            if (contracts == null) throw new ArgumentNullException(nameof(contracts));
            var snapshot = new List<ContractInstance>(contracts);
            var usage = ProgressionResources.CalculateUsage(hardware, snapshot);
            var affected = new List<AffectedContract>();
            foreach (var contract in snapshot)
                if (contract.AssignedHardwareId == hardware.Id) affected.Add(new AffectedContract(contract));
            var identities = affected.AsReadOnly();
            var result = new List<ResourceBottleneck>();
            Add(result, hardware, ResourceKind.Cpu, "cores", usage.Used.CpuCores, usage.Capacity.CpuCores, identities);
            Add(result, hardware, ResourceKind.Ram, "GB", usage.Used.RamGb, usage.Capacity.RamGb, identities);
            Add(result, hardware, ResourceKind.Storage, "GB", usage.Used.StorageGb, usage.Capacity.StorageGb, identities);
            Add(result, hardware, ResourceKind.Bandwidth, "Mbps", usage.Used.BandwidthMbps, usage.Capacity.BandwidthMbps, identities);
            return result.AsReadOnly();
        }

        private static void Add(List<ResourceBottleneck> result, HardwareInstance hardware, ResourceKind resource,
            string unit, double demand, double capacity, ReadOnlyCollection<AffectedContract> affected)
        {
            if (demand > capacity) result.Add(new ResourceBottleneck(hardware, resource, unit, demand, capacity, affected));
        }
    }
}
