using UnityEngine;

namespace InfrastructureTycoon.World
{
    [RequireComponent(typeof(Camera))]
    public sealed class GarageCamera : MonoBehaviour
    {
        [SerializeField] private RectTransform topBar;
        [SerializeField] private RectTransform statusBar;
        [SerializeField] private Vector2 worldSize = new Vector2(12, 7);
        [SerializeField] private float padding = 0.4f;
        private Camera worldCamera;
        private readonly Vector3[] corners = new Vector3[4];

        private void Awake() => worldCamera = GetComponent<Camera>();
        private void OnEnable() => Canvas.willRenderCanvases += UpdateViewport;
        private void OnDisable() => Canvas.willRenderCanvases -= UpdateViewport;

        // Canvas layout and scaling must finish before allocating the world viewport.
        private void UpdateViewport()
        {
            topBar.GetWorldCorners(corners);
            float top = corners[0].y / Screen.height;
            statusBar.GetWorldCorners(corners);
            float bottom = corners[1].y / Screen.height;
            worldCamera.rect = new Rect(0, bottom, 1, Mathf.Max(0.01f, top - bottom));
            worldCamera.orthographicSize = Mathf.Max(worldSize.y / 2, worldSize.x / (2 * worldCamera.aspect)) + padding;
        }
    }
}
