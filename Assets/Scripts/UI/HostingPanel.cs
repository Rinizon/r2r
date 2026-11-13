using System.Globalization;
using InfrastructureTycoon.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace InfrastructureTycoon.UI
{
    public sealed class HostingPanel : MonoBehaviour
    {
        [SerializeField] private GameRoot gameRoot;
        [SerializeField] private HostingOfferRow rowPrefab;
        [SerializeField] private Transform rows;
        [SerializeField] private Text resourceUsage;
        [SerializeField] private Text monthlyResult;
        [SerializeField] private Text feedback;
        private bool initialized;

        private void OnEnable()
        {
            if (!initialized)
            {
                foreach (var offer in gameRoot.Session.Catalog.Offers)
                {
                    var row = Instantiate(rowPrefab, rows);
                    row.name = "Offer-" + offer.Id;
                    row.Bind(gameRoot, offer, ShowFeedback);
                }
                initialized = true;
            }
            Refresh();
        }

        private void LateUpdate() => Refresh();

        private void ShowFeedback(string message)
        {
            feedback.text = message;
            Refresh();
        }

        private void Refresh()
        {
            var session = gameRoot.Session;
            var usage = session.Hardware[0].Resources;
            resourceUsage.text = string.Format(CultureInfo.InvariantCulture,
                "STARTER PC   CPU {0}/{1} cores   |   RAM {2}/{3} GB\nSTORAGE {4}/{5} GB   |   BANDWIDTH {6}/{7} Mbps",
                usage.Used.CpuCores, usage.Capacity.CpuCores, usage.Used.RamGb, usage.Capacity.RamGb,
                usage.Used.StorageGb, usage.Capacity.StorageGb, usage.Used.BandwidthMbps, usage.Capacity.BandwidthMbps);
            var result = session.LastMonthlyResult;
            monthlyResult.text = result == null ? "LAST MONTH\nNo billing completed yet." :
                "LAST MONTH  " + result.PeriodEnd.ToString("MMM yyyy", CultureInfo.InvariantCulture).ToUpperInvariant() +
                "\nRevenue " + Money(result.Revenue) + "   |   Expenses " + Money(result.Expenses) +
                "   |   Net " + (result.NetProfit > 0 ? "+" : "") + Money(result.NetProfit);
        }

        internal static string Money(decimal value) =>
            (value < 0 ? "-$" : "$") + System.Math.Abs(value).ToString("0.##", CultureInfo.InvariantCulture);
    }
}
