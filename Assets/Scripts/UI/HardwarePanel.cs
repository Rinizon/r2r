using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace InfrastructureTycoon.UI
{
    public sealed class HardwarePanel : MonoBehaviour
    {
        [SerializeField] private GameRoot gameRoot;
        [SerializeField] private HardwareUpgradeRow rowPrefab;
        [SerializeField] private Transform rows;
        [SerializeField] private Text cash;
        [SerializeField] private Text capacity;
        [SerializeField] private Text feedback;
        private bool initialized;

        private void OnEnable()
        {
            if (!initialized)
            {
                foreach (var upgrade in gameRoot.Session.Catalog.Upgrades)
                {
                    var row = Instantiate(rowPrefab, rows);
                    row.name = "Upgrade-" + upgrade.Id;
                    row.Bind(gameRoot, upgrade.Id, ShowFeedback);
                }
                initialized = true;
            }
            Refresh();
        }

        public void RebindSession()
        {
            feedback.text = "Session loaded.";
            // Rows resolve the current session and content by ID on every refresh/action.
            Refresh();
        }

        private void LateUpdate() => Refresh();

        private void ShowFeedback(string message)
        {
            feedback.text = message;
            Refresh();
            gameRoot.RefreshHud();
        }

        private void Refresh()
        {
            cash.text = "CASH  " + HostingPanel.Money(gameRoot.Session.Cash);
            var current = gameRoot.Session.Hardware[0].EffectiveCapacity;
            capacity.text = string.Format(CultureInfo.InvariantCulture,
                "STARTER PC   CPU {0} cores   |   RAM {1} GB\nSTORAGE {2} GB   |   INTERNET {3} Mbps",
                current.CpuCores, current.RamGb, current.StorageGb, current.BandwidthMbps);
            foreach (var row in rows.GetComponentsInChildren<HardwareUpgradeRow>()) row.Refresh();
        }
    }
}
