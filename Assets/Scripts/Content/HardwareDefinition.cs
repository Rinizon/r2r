using System;
using UnityEngine;

namespace InfrastructureTycoon.Content
{
    [CreateAssetMenu(menuName = "Infrastructure Tycoon/Hardware Definition")]
    public sealed class HardwareDefinition : ScriptableObject
    {
        [SerializeField] private string contentId;
        [SerializeField] private string displayName;
        [SerializeField] private ResourceDefinition capacity = new ResourceDefinition();
        [SerializeField, Tooltip("Whole dollars per month; converted to decimal runtime money.")]
        private int monthlyExpenseDollars;

        public HardwareData ToRuntime()
        {
            if (capacity == null)
                throw new ArgumentException("Hardware requires capacity values.");
            return new HardwareData(contentId, displayName, capacity.ToRuntime(), monthlyExpenseDollars);
        }
    }
}
