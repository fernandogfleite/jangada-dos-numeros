using UnityEngine;

namespace JangadaDosNumeros.Runtime
{
    public sealed class SlotView : MonoBehaviour
    {
        [SerializeField] private int index;
        [SerializeField] private Transform cargoAnchor;

        public int Index
        {
            get => index;
            set => index = value;
        }

        public Transform CargoAnchor
        {
            get => cargoAnchor != null ? cargoAnchor : transform;
            set => cargoAnchor = value;
        }
    }
}
