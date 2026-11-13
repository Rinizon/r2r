using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using InfrastructureTycoon.Content;
using InfrastructureTycoon.Simulation;

namespace InfrastructureTycoon.UI
{
    public static class CapacityFeedback
    {
        public static string ResourceName(ResourceKind resource)
        {
            switch (resource)
            {
                case ResourceKind.Cpu: return "CPU";
                case ResourceKind.Ram: return "RAM";
                case ResourceKind.Storage: return "Storage";
                case ResourceKind.Bandwidth: return "Bandwidth";
                default: throw new System.ArgumentOutOfRangeException(nameof(resource));
            }
        }

        public static string Summary(IReadOnlyList<ResourceBottleneck> reports) => reports.Count == 0
            ? "NO CAPACITY ALERTS"
            : "CAPACITY ALERTS: " + string.Join(", ", reports.Select(x => ResourceName(x.Resource)).Distinct());

        public static string Details(IReadOnlyList<ResourceBottleneck> reports)
        {
            if (reports.Count == 0) return "No capacity alerts.";
            var text = new StringBuilder();
            foreach (var hardware in reports.GroupBy(x => x.HardwareId))
            {
                foreach (var report in hardware)
                {
                    if (text.Length > 0) text.Append('\n');
                    text.AppendFormat(CultureInfo.InvariantCulture, "{0}: {1} {2}/{3} {4} (short {5} {4})",
                        report.HardwareName, ResourceName(report.Resource), report.Demand, report.Capacity, report.Unit, report.Shortfall);
                }
                text.Append("\nAffected: ");
                text.Append(string.Join(", ", hardware.First().AffectedContracts.Select(x => x.DisplayName)));
            }
            return text.ToString();
        }

        public static string Growth(ContractInstance contract, HostingCatalog catalog)
        {
            if (contract == null) return string.Empty;
            var milestone = catalog.Milestones.Where(x => contract.AppliedMilestoneIds.Contains(x.Id))
                .OrderBy(x => x.RequiredPaidMonths).LastOrDefault();
            if (milestone == null) return string.Empty;
            var original = contract.BaseRequirements;
            var current = contract.EffectiveRequirements;
            var changes = new List<string>();
            Change(changes, "CPU", "cores", original.CpuCores, current.CpuCores);
            Change(changes, "RAM", "GB", original.RamGb, current.RamGb);
            Change(changes, "Storage", "GB", original.StorageGb, current.StorageGb);
            Change(changes, "Bandwidth", "Mbps", original.BandwidthMbps, current.BandwidthMbps);
            return "Growth: " + (changes.Count == 0 ? milestone.DisplayName : string.Join(", ", changes)) +
                " after " + milestone.RequiredPaidMonths.ToString(CultureInfo.InvariantCulture) + " paid months";
        }

        private static void Change(List<string> changes, string resource, string unit, double original, double current)
        {
            if (original != current)
                changes.Add(string.Format(CultureInfo.InvariantCulture, "{0} {1} -> {2} {3}", resource, original, current, unit));
        }
    }
}
