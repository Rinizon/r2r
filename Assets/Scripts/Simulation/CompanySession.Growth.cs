using System.Collections.Generic;
using InfrastructureTycoon.Content;

namespace InfrastructureTycoon.Simulation
{
    public sealed partial class CompanySession
    {
        private void ApplyEligibleMilestones(bool notify)
        {
            var prepared = new Dictionary<ContractInstance, ContractInstance>();
            var contracts = new List<ContractInstance>();
            var events = new List<(ContractInstance Contract, GrowthMilestoneData Milestone)>();
            foreach (var contract in acceptedContracts)
            {
                var applied = new List<string>(contract.AppliedMilestoneIds);
                if (contract.IsHosted)
                    foreach (var milestone in Catalog.Milestones)
                        if (milestone.TargetOfferId == contract.Definition.Id &&
                            contract.PaidMonths >= milestone.RequiredPaidMonths && !applied.Contains(milestone.Id))
                        {
                            applied.Add(milestone.Id);
                            events.Add((contract, milestone));
                        }
                if (applied.Count == contract.AppliedMilestoneIds.Count)
                {
                    contracts.Add(contract);
                    continue;
                }
                var candidate = new ContractInstance(contract.Definition, contract.Id, contract.PaidMonths,
                    applied, Catalog.Milestones, contract.AssignedHardwareId);
                prepared.Add(contract, candidate);
                contracts.Add(candidate);
            }
            if (prepared.Count == 0) return;

            // Prepare all demands before committing any profile, allowing growth-induced overloads.
            var usage = new Dictionary<HardwareInstance, ResourceUsage>();
            foreach (var hardware in Hardware)
                usage.Add(hardware, ProgressionResources.CalculateUsage(hardware, contracts));
            foreach (var pair in prepared) pair.Key.CommitMilestones(pair.Value);
            foreach (var pair in usage) pair.Key.SetUsedResources(pair.Value.Used);

            // Observers see completed billing, paid-month counters, profiles, and hardware usage.
            if (notify)
                foreach (var entry in events) MilestoneApplied?.Invoke(entry.Contract, entry.Milestone);
        }
    }
}
