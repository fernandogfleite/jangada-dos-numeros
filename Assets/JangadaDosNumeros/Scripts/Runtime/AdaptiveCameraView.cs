using UnityEngine;

namespace JangadaDosNumeros.Runtime
{
    [RequireComponent(typeof(Camera))]
    public sealed class AdaptiveCameraView : MonoBehaviour
    {
        [SerializeField] private Vector3 focusPoint = new Vector3(0f, 0.7f, 1.1f);
        [SerializeField] private Vector3 viewingDirection = new Vector3(0f, 0.72f, -1f);
        [SerializeField] private float verticalFieldOfView = 42f;
        [SerializeField] private float requiredVerticalHalfExtent = 5.8f;
        [SerializeField] private float requiredHorizontalHalfExtent = 8.8f;
        [SerializeField] private float framingPadding = 1.08f;
        private Camera controlledCamera;

        public float CalculateCameraDistance(float aspect)
        {
            var safeAspect = Mathf.Max(0.1f, aspect);
            var halfVerticalFov = verticalFieldOfView * Mathf.Deg2Rad * 0.5f;
            var halfHorizontalFov = Mathf.Atan(Mathf.Tan(halfVerticalFov) * safeAspect);
            var verticalDistance = requiredVerticalHalfExtent / Mathf.Tan(halfVerticalFov);
            var horizontalDistance = requiredHorizontalHalfExtent / Mathf.Tan(halfHorizontalFov);
            return Mathf.Max(verticalDistance, horizontalDistance) * framingPadding;
        }

        public void ApplyFramingForAspect(float aspect)
        {
            if (controlledCamera == null)
            {
                controlledCamera = GetComponent<Camera>();
            }

            if (controlledCamera == null)
            {
                return;
            }

            controlledCamera.orthographic = false;
            controlledCamera.fieldOfView = verticalFieldOfView;
            transform.position = focusPoint + viewingDirection.normalized * CalculateCameraDistance(aspect);
            transform.LookAt(focusPoint);
        }

        private void Awake()
        {
            controlledCamera = GetComponent<Camera>();
            ApplyFramingForAspect(controlledCamera.aspect);
        }

        private void OnValidate()
        {
            controlledCamera = GetComponent<Camera>();
            ApplyFramingForAspect(controlledCamera != null ? controlledCamera.aspect : 16f / 9f);
        }

        private void OnPreCull()
        {
            ApplyFramingForAspect(controlledCamera != null ? controlledCamera.aspect : 16f / 9f);
        }
    }
}
