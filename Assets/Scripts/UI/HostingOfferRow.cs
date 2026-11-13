using System;
using System.Globalization;
using InfrastructureTycoon.Content;
using InfrastructureTycoon.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace InfrastructureTycoon.UI
{
    public sealed class HostingOfferRow : MonoBehaviour
    {
        [SerializeField] private Text offerName;
        [SerializeField] private Text requirements;
        [SerializeField] private Text state;
        [SerializeField] private Button accept;
        [SerializeField] private Button assign;
        private GameRoot root;
        private OfferData offer;
        private Action<string> feedback;

        public void Bind(GameRoot gameRoot, OfferData definition, Action<string> showFeedback)
        {
            root = gameRoot;
            offer = definition;
            feedback = showFeedback;
            accept.onClick.AddListener(Accept);
            assign.onClick.AddListener(Assign);
            offerName.text = offer.DisplayName + "  /  " + HostingPanel.Money(offer.MonthlyRevenue) + "/mo";
            Refresh();
        }

        private ContractInstance Contract()
        {
            foreach (var contract in root.Session.AcceptedContracts)
                if (contract.Definition.Id == offer.Id) return contract;
            return null;
        }

        private void LateUpdate()
        {
            if (root != null) Refresh();
        }

        private void Refresh()
        {
            var contract = Contract();
            var demand = contract == null ? offer.Requirements : contract.EffectiveRequirements;
            requirements.text = string.Format(CultureInfo.InvariantCulture,
                "CPU {0} cores | RAM {1} GB | Storage {2} GB | Bandwidth {3} Mbps",
                demand.CpuCores, demand.RamGb, demand.StorageGb, demand.BandwidthMbps);
            accept.interactable = contract == null;
            assign.interactable = contract != null && !contract.IsHosted;
            state.text = contract == null ? "AVAILABLE" : contract.IsHosted ? "HOSTED ON STARTER PC" : "ACCEPTED / UNASSIGNED";
            var growth = CapacityFeedback.Growth(contract, root.Session.Catalog);
            if (growth.Length > 0) state.text += " | " + growth;
        }

        private void Accept()
        {
            bool success = root.Session.TryAcceptOffer(offer.Id, out _, out var reason);
            Refresh();
            feedback(success ? offer.DisplayName + " accepted. Assign it to begin hosting." : reason);
        }

        private void Assign()
        {
            var contract = Contract();
            if (contract == null)
            {
                feedback("Accept this offer before assigning it.");
                return;
            }
            bool success = root.Session.TryAssignContract(contract.Id, root.Session.Hardware[0].Id, out var reason);
            Refresh();
            feedback(success ? offer.DisplayName + " is now hosted on the starter PC." : reason);
        }

        private void OnDestroy()
        {
            if (accept != null) accept.onClick.RemoveListener(Accept);
            if (assign != null) assign.onClick.RemoveListener(Assign);
        }
    }
}
