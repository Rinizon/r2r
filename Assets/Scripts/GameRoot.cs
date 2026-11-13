using InfrastructureTycoon.Simulation;
using InfrastructureTycoon.Content;
using System.Globalization;
using System.IO;
using InfrastructureTycoon.UI;
using InfrastructureTycoon.World;
using UnityEngine;
using UnityEngine.UI;

namespace InfrastructureTycoon
{
    public sealed class GameRoot : MonoBehaviour
    {
        [SerializeField] private StarterCatalog starterCatalog;
        [SerializeField] private Text dateLabel;
        [SerializeField] private Text speedLabel;
        [SerializeField] private Text cashLabel;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Text saveFeedback;
        [SerializeField] private HostingPanel hostingPanel;
        [SerializeField] private HardwarePanel hardwarePanel;
        [SerializeField] private PcStationInteraction station;
        // Inspector order: pause, 1x, 2x, 4x, 8x.
        [SerializeField] private Button[] speedButtons;

        private static readonly SimulationSpeed[] Speeds =
        {
            SimulationSpeed.Paused, SimulationSpeed.Normal, SimulationSpeed.Double,
            SimulationSpeed.Quadruple, SimulationSpeed.Octuple
        };

        public CompanySession Session { get; private set; }
        public SimulationClock Clock => Session.Clock;
        private SessionSaveSlot saveSlot;
        private enum FileAction { None, Save, Load }
        private FileAction pending;

        public void ConfigureSaveSlot(SessionSaveSlot slot) => saveSlot = slot ?? throw new System.ArgumentNullException(nameof(slot));
        public void RequestSave() => pending = FileAction.Save;
        public void RequestLoad() => pending = FileAction.Load;

        private void Awake()
        {
            Session = new CompanySession(starterCatalog.ToRuntime());
            saveSlot = new SessionSaveSlot(Path.Combine(Application.persistentDataPath, "company-v1.json"));
            saveButton.onClick.AddListener(RequestSave);
            loadButton.onClick.AddListener(RequestLoad);
            for (int i = 0; i < Speeds.Length; i++)
            {
                var speed = Speeds[i];
                speedButtons[i].onClick.AddListener(() => SelectSpeed(speed));
            }
            RefreshHud();
        }

        private void Update()
        {
            Clock.Advance(Time.unscaledDeltaTime);
            RefreshHud();
        }

        private void LateUpdate()
        {
            var action = pending;
            pending = FileAction.None;
            // Input can request a save at any time; capture only after Update finishes.
            if (action == FileAction.Save)
                saveFeedback.text = saveSlot.TrySave(Session, Application.version, out var saveReason) ? "Saved." : saveReason;
            else if (action == FileAction.Load)
            {
                if (saveSlot.TryLoad(Session.Catalog, out var restored, out var loadReason))
                {
                    Session = restored;
                    PcStationInteraction.CloseAll();
                    hostingPanel.RebindSession();
                    hardwarePanel.RebindSession();
                    RefreshHud();
                    saveFeedback.text = "Loaded.";
                }
                else saveFeedback.text = loadReason;
            }
        }

        private void OnDestroy()
        {
            if (saveButton != null) saveButton.onClick.RemoveListener(RequestSave);
            if (loadButton != null) loadButton.onClick.RemoveListener(RequestLoad);
        }

        private void SelectSpeed(SimulationSpeed speed)
        {
            Clock.Speed = speed;
            RefreshHud();
        }

        internal void RefreshHud()
        {
            cashLabel.text = "CASH  " + UI.HostingPanel.Money(Session.Cash);
            dateLabel.text = "DATE  " + Clock.Date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture).ToUpperInvariant();
            speedLabel.text = Clock.Speed == SimulationSpeed.Paused ? "SPEED  PAUSED" : "SPEED  " + (int)Clock.Speed + "x";
            for (int i = 0; i < Speeds.Length; i++)
                speedButtons[i].interactable = Clock.Speed != Speeds[i];
        }
    }
}
