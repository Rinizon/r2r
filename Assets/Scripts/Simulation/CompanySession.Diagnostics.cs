using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace InfrastructureTycoon.Simulation
{
    public sealed partial class CompanySession
    {
        public ReadOnlyCollection<ResourceBottleneck> GetBottlenecks()
        {
            var result = new List<ResourceBottleneck>();
            foreach (var hardware in Hardware) result.AddRange(ResourceBottlenecks.Diagnose(hardware, AcceptedContracts));
            return result.AsReadOnly();
        }
    }
}
