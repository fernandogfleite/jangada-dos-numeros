using System;

namespace JangadaDosNumeros.Core
{
    public enum PlacementResult
    {
        Success,
        InvalidSlot,
        Occupied
    }

    public sealed class RaftLoadout
    {
        private readonly CargoType?[] slots;

        public RaftLoadout(int slotCount = 8)
        {
            if (slotCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slotCount));
            }

            slots = new CargoType?[slotCount];
        }

        public int SlotCount => slots.Length;

        public int OccupiedCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < slots.Length; i++)
                {
                    if (slots[i].HasValue)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public CargoType? GetCargo(int slotIndex)
        {
            ValidateIndex(slotIndex);
            return slots[slotIndex];
        }

        public PlacementResult TryPlace(int slotIndex, CargoType cargo)
        {
            if (slotIndex < 0 || slotIndex >= slots.Length)
            {
                return PlacementResult.InvalidSlot;
            }

            if (slots[slotIndex].HasValue)
            {
                return PlacementResult.Occupied;
            }

            slots[slotIndex] = cargo;
            return PlacementResult.Success;
        }

        public bool TryRemove(int slotIndex, out CargoType cargo)
        {
            cargo = default(CargoType);
            if (slotIndex < 0 || slotIndex >= slots.Length || !slots[slotIndex].HasValue)
            {
                return false;
            }

            cargo = slots[slotIndex].Value;
            slots[slotIndex] = null;
            return true;
        }

        public int Count(CargoType cargo)
        {
            var count = 0;
            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i] == cargo)
                {
                    count++;
                }
            }

            return count;
        }

        public void Clear()
        {
            Array.Clear(slots, 0, slots.Length);
        }

        private void ValidateIndex(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(slotIndex));
            }
        }
    }
}

