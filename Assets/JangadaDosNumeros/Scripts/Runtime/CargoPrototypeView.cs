using JangadaDosNumeros.Core;
using UnityEngine;

namespace JangadaDosNumeros.Runtime
{
    public sealed class CargoPrototypeView : MonoBehaviour
    {
        [SerializeField] private CargoType cargoType;

        public CargoType CargoType
        {
            get => cargoType;
            set => cargoType = value;
        }
    }
}
