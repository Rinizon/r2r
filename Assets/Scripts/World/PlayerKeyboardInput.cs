using UnityEngine;

namespace InfrastructureTycoon.World
{
    [RequireComponent(typeof(PlayerMovement))]
    public sealed class PlayerKeyboardInput : MonoBehaviour
    {
        private PlayerMovement movement;
        private void Awake() => movement = GetComponent<PlayerMovement>();

        private void Update()
        {
            float x = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0)
                - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
            float y = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0)
                - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0);
            movement.SetDirection(new Vector2(x, y));
        }

        private void OnDisable()
        {
            if (movement != null) movement.SetDirection(Vector2.zero);
        }
    }
}
