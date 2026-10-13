using UnityEngine;

namespace JangadaDosNumeros.Runtime
{
    [DisallowMultipleComponent]
    public sealed class DeliveryZoneView : MonoBehaviour
    {
        [SerializeField] private float radius = 1.15f;

        public float Radius { get => radius; set => radius = Mathf.Max(0.1f, value); }

        public bool Contains(Vector3 position)
        {
            var delta = position - transform.position;
            delta.y = 0f;
            return delta.sqrMagnitude <= radius * radius;
        }
    }
}
