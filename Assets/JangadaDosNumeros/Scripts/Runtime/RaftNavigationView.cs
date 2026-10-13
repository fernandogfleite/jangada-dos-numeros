using UnityEngine;

namespace JangadaDosNumeros.Runtime
{
    [DisallowMultipleComponent]
    public sealed class RaftNavigationView : MonoBehaviour
    {
        [SerializeField] private Transform loadPoint;
        [SerializeField] private Transform boardingPoint;
        [SerializeField] private Transform driverAnchor;
        [SerializeField] private float acceleration = 2.2f;
        [SerializeField] private float maximumSpeed = 2.4f;
        [SerializeField] private float turnSpeed = 70f;

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private float speed;

        public Transform LoadPoint { get => loadPoint; set => loadPoint = value; }
        public Transform BoardingPoint { get => boardingPoint; set => boardingPoint = value; }
        public Transform DriverAnchor { get => driverAnchor; set => driverAnchor = value; }
        public float Speed => speed;

        public void CaptureInitialPose()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        public void Navigate(Vector2 input, float deltaTime, bool forceStop)
        {
            if (forceStop)
            {
                speed = 0f;
                return;
            }

            var targetSpeed = input.y * maximumSpeed;
            speed = Mathf.MoveTowards(speed, targetSpeed, acceleration * deltaTime);
            transform.Rotate(0f, input.x * turnSpeed * deltaTime, 0f, Space.World);
            var next = transform.position + transform.forward * speed * deltaTime;
            transform.position = ClampToNavigableArea(next);
        }

        public void ResetPose()
        {
            speed = 0f;
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }

        public static Vector3 ClampToNavigableArea(Vector3 position)
        {
            return new Vector3(
                Mathf.Clamp(position.x, -0.15f, 4.25f),
                position.y,
                Mathf.Clamp(position.z, -3.6f, 1.05f));
        }
    }
}
