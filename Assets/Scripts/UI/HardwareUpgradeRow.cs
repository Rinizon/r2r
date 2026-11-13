using System;
using System.Globalization;
using System.Text;
using InfrastructureTycoon.Content;
using UnityEngine;
using UnityEngine.UI;

namespace InfrastructureTycoon.UI
{
    public sealed class HardwareUpgradeRow : MonoBehaviour
    {
        [SerializeField] private Text description;
        [SerializeField] private Text preview;
        [SerializeField] private Button buy;
        [SerializeField] private Text buyLabel;
        private GameRoot root;
        private string upgradeId;
        private Action<string> feedback;

        public void Bind(GameRoot gameRoot, string id, Action<string> showFeedback)
        {
            buy.onClick.RemoveListener(Purchase);
            root = gameRoot;
            upgradeId = id;
            feedback = showFeedback;
            buy.onClick.AddListener(Purchase);
            Refresh();
        }

        private UpgradeData Definition()
        {
            foreach (var upgrade in root.Session.Catalog.Upgrades)
                if (upgrade.Id == upgradeId) return upgrade;
            throw new InvalidOperationException("Missing upgrade definition.");
        }

        public void Refresh()
        {
            if (root == null) return;
            var upgrade = Definition();
            var hardware = root.Session.Hardware[0];
            bool owned = hardware.AppliedUpgradeIds.Contains(upgradeId);
            bool affordable = root.Session.Cash >= upgrade.Price;
            description.text = upgrade.DisplayName + "  /  " + HostingPanel.Money(upgrade.Price);
            var current = hardware.EffectiveCapacity;
            var delta = upgrade.CapacityDelta;
            var text = new StringBuilder();
            AddPreview(text, "CPU", "cores", current.CpuCores, delta.CpuCores, owned);
            AddPreview(text, "RAM", "GB", current.RamGb, delta.RamGb, owned);
            AddPreview(text, "Storage", "GB", current.StorageGb, delta.StorageGb, owned);
            AddPreview(text, "Internet", "Mbps", current.BandwidthMbps, delta.BandwidthMbps, owned);
            preview.text = text.ToString();
            buy.interactable = !owned && affordable;
            buyLabel.text = owned ? "OWNED" : affordable ? "BUY" : "NEED CASH";
        }

        private static void AddPreview(StringBuilder text, string resource, string unit, double current, double delta, bool owned)
        {
            if (delta == 0) return;
            if (text.Length > 0) text.Append("   |   ");
            text.AppendFormat(CultureInfo.InvariantCulture, "{0} {1} -> {2} {3}", resource, current, owned ? current : current + delta, unit);
        }

        private void Purchase()
        {
            var upgrade = Definition();
            bool success = root.Session.TryPurchaseUpgrade(upgradeId, root.Session.Hardware[0].Id, out var reason);
            Refresh();
            feedback(success ? upgrade.DisplayName + " purchased for " + HostingPanel.Money(upgrade.Price) + "." : reason);
        }

        private void OnDestroy()
        {
            if (buy != null) buy.onClick.RemoveListener(Purchase);
        }
    }
}
