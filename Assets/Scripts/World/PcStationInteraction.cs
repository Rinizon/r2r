using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InfrastructureTycoon.World
{
    public sealed class PcStationInteraction : MonoBehaviour
    {
        [SerializeField] private Collider2D reach;
        [SerializeField] private Collider2D playerCollider;
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject prompt;
        [SerializeField] private Button closeButton;
        private static int consumedFrame = -1;

        public static void CloseAll()
        {
            foreach (var station in FindObjectsByType<PcStationInteraction>(FindObjectsSortMode.None))
                station.Close();
        }

        private bool CanSelect()
        {
            foreach (var station in FindObjectsByType<PcStationInteraction>(FindObjectsSortMode.None))
            {
                if (station != this && station.IsOpen) return false;
                if (station == this || !station.IsWithinReach) continue;
                float mine = ((Vector2)reach.bounds.center - (Vector2)playerCollider.bounds.center).sqrMagnitude;
                float other = ((Vector2)station.reach.bounds.center - (Vector2)playerCollider.bounds.center).sqrMagnitude;
                if (other < mine || (other == mine && string.CompareOrdinal(station.name, name) < 0)) return false;
            }
            return true;
        }

        public bool IsOpen => panel.activeSelf;
        public bool IsWithinReach => reach.IsTouching(playerCollider);

        private void Awake()
        {
            panel.SetActive(false);
            prompt.SetActive(false);
            closeButton.onClick.AddListener(Close);
        }

        private void Update()
        {
            if (IsOpen && !IsWithinReach) Close();
            HandleInput(Input.GetKeyDown(KeyCode.E), Input.GetKeyDown(KeyCode.Escape));
            RefreshPrompt();
        }

        public bool TryOpen()
        {
            if (IsOpen || !IsWithinReach || !CanSelect()) return false;
            var selected = EventSystem.current == null ? null : EventSystem.current.currentSelectedGameObject;
            if (selected != null && selected.GetComponentInParent<InputField>() != null) return false;
            panel.SetActive(true);
            RefreshPrompt();
            return true;
        }

        public void Close()
        {
            if (IsOpen) consumedFrame = Time.frameCount;
            var events = EventSystem.current;
            if (events != null && events.currentSelectedGameObject != null &&
                events.currentSelectedGameObject.transform.IsChildOf(panel.transform))
                events.SetSelectedGameObject(null);
            panel.SetActive(false);
            RefreshPrompt();
        }

        public void HandleInput(bool interactPressed, bool closePressed)
        {
            // Consume Escape before considering interaction, including simultaneous input.
            if (closePressed && IsOpen)
            {
                Close();
                return;
            }
            if (closePressed || consumedFrame == Time.frameCount) return;
            if (interactPressed && !IsOpen &&
                (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()) && TryOpen())
                consumedFrame = Time.frameCount;
        }

        private void RefreshPrompt() => prompt.SetActive(IsWithinReach && !IsOpen && CanSelect());

        private void OnDisable()
        {
            if (panel != null) panel.SetActive(false);
            if (prompt != null) prompt.SetActive(false);
        }

        private void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
        }
    }
}
