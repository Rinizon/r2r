using System;
using UnityEngine;

namespace InfrastructureTycoon.Content
{
    [CreateAssetMenu(menuName = "Infrastructure Tycoon/Growth Milestone Definition")]
    public sealed class GrowthMilestoneDefinition : ScriptableObject
    {
        [SerializeField] private string contentId;
        [SerializeField] private string displayName;
        [SerializeField] private string targetOfferId;
        [SerializeField] private int requiredPaidMonths;
        [SerializeField] private ResourceDefinition requirements = new ResourceDefinition();

        public GrowthMilestoneData ToRuntime()
        {
            if (requirements == null) throw new ArgumentException("Milestone requires workload requirements.");
            return new GrowthMilestoneData(contentId, displayName, targetOfferId, requiredPaidMonths, requirements.ToRuntime());
        }
    }
}
