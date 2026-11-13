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
            if (IsOpen || !IsWithinReach) return false;
            var selected = EventSystem.current == null ? null : EventSystem.current.currentSelectedGameObject;
            if (selected != null && selected.GetComponentInParent<InputField>() != null) return false;
            panel.SetActive(true);
            RefreshPrompt();
            return true;
        }

        public void Close()
        {
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
            if (closePressed)
            {
                Close();
                return;
            }
            if (interactPressed && !IsOpen) TryOpen();
        }

        private void RefreshPrompt() => prompt.SetActive(IsWithinReach && !IsOpen);

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
