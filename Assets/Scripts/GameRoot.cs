using InfrastructureTycoon.Simulation;
using InfrastructureTycoon.Content;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace InfrastructureTycoon
{
    public sealed class GameRoot : MonoBehaviour
    {
        [SerializeField] private StarterCatalog starterCatalog;
        [SerializeField] private Text dateLabel;
        [SerializeField] private Text speedLabel;
        // Inspector order: pause, 1x, 2x, 4x, 8x.
        [SerializeField] private Button[] speedButtons;

        private static readonly SimulationSpeed[] Speeds =
        {
            SimulationSpeed.Paused, SimulationSpeed.Normal, SimulationSpeed.Double,
            SimulationSpeed.Quadruple, SimulationSpeed.Octuple
        };

        public CompanySession Session { get; private set; }
        public SimulationClock Clock => Session.Clock;

        private void Awake()
        {
            Session = new CompanySession(starterCatalog.ToRuntime());
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

        private void SelectSpeed(SimulationSpeed speed)
        {
            Clock.Speed = speed;
            RefreshHud();
        }

        private void RefreshHud()
        {
            dateLabel.text = "DATE  " + Clock.Date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture).ToUpperInvariant();
            speedLabel.text = Clock.Speed == SimulationSpeed.Paused ? "SPEED  PAUSED" : "SPEED  " + (int)Clock.Speed + "x";
            for (int i = 0; i < Speeds.Length; i++)
                speedButtons[i].interactable = Clock.Speed != Speeds[i];
        }
    }
}
