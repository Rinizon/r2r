using UnityEngine;

namespace InfrastructureTycoon.World
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float speed = 3f;
        private Rigidbody2D body;
        private Vector2 direction;

        private void Awake() => body = GetComponent<Rigidbody2D>();

        public void SetDirection(Vector2 input) => direction = Vector2.ClampMagnitude(input, 1);

        private void FixedUpdate()
        {
            // Physics owns movement so world blockers stop the player.
            body.MovePosition(body.position + direction * speed * Time.fixedDeltaTime);
        }

        private void OnDisable() => direction = Vector2.zero;
    }
}
