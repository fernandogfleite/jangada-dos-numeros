using UnityEngine;

namespace JangadaDosNumeros.Runtime
{
    [DisallowMultipleComponent]
    public sealed class PlayerCharacterController : MonoBehaviour
    {
        private static readonly Rect[] WalkableRegions =
        {
            new Rect(-7.35f, 0.65f, 3.35f, 3.15f),
            new Rect(-5.05f, -2.25f, 2.55f, 4.15f),
            new Rect(-2.85f, -0.82f, 2.25f, 1.64f)
        };

        [SerializeField] private Transform carryAnchor;
        [SerializeField] private float moveSpeed = 2.8f;

        private Vector3 spawnPosition;
        private Quaternion spawnRotation;

        public Transform CarryAnchor
        {
            get => carryAnchor;
            set => carryAnchor = value;
        }

        public void CaptureSpawnPose()
        {
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        public void Move(Vector2 input, float deltaTime)
        {
            var direction = new Vector3(input.x, 0f, input.y);
            if (direction.sqrMagnitude > 1f)
            {
                direction.Normalize();
            }

            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            transform.position = ClampToWalkableArea(transform.position + direction * moveSpeed * deltaTime);
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        public bool IsNear(Vector3 position, float distance)
        {
            var delta = transform.position - position;
            delta.y = 0f;
            return delta.sqrMagnitude <= distance * distance;
        }

        public void Board(Transform driverAnchor)
        {
            transform.SetParent(driverAnchor, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        public void ResetToSpawn(Transform worldParent)
        {
            transform.SetParent(worldParent, true);
            transform.position = spawnPosition;
            transform.rotation = spawnRotation;
        }

        public static Vector3 ClampToWalkableArea(Vector3 position)
        {
            var point = new Vector2(position.x, position.z);
            var best = point;
            var bestDistance = float.PositiveInfinity;
            for (var i = 0; i < WalkableRegions.Length; i++)
            {
                var region = WalkableRegions[i];
                var candidate = new Vector2(
                    Mathf.Clamp(point.x, region.xMin, region.xMax),
                    Mathf.Clamp(point.y, region.yMin, region.yMax));
                var distance = (candidate - point).sqrMagnitude;
                if (distance < bestDistance)
                {
                    best = candidate;
                    bestDistance = distance;
                }
            }

            return new Vector3(best.x, position.y, best.y);
        }
    }
}
