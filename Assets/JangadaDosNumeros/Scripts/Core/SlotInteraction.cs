using System;

namespace JangadaDosNumeros.Core
{
    public enum SlotInteractionAction
    {
        PlacedSelected,
        TookExisting,
        RejectedOccupied,
        NoSelection
    }

    public static class SlotInteraction
    {
        public static SlotInteractionAction Apply(RaftLoadout loadout, int slotIndex, ref CargoType? selectedCargo)
        {
            if (loadout == null)
            {
                throw new ArgumentNullException(nameof(loadout));
            }

            var existingCargo = loadout.GetCargo(slotIndex);
            if (existingCargo.HasValue)
            {
                if (selectedCargo.HasValue)
                {
                    return SlotInteractionAction.RejectedOccupied;
                }

                CargoType removedCargo;
                loadout.TryRemove(slotIndex, out removedCargo);
                selectedCargo = removedCargo;
                return SlotInteractionAction.TookExisting;
            }

            if (!selectedCargo.HasValue)
            {
                return SlotInteractionAction.NoSelection;
            }

            loadout.TryPlace(slotIndex, selectedCargo.Value);
            selectedCargo = null;
            return SlotInteractionAction.PlacedSelected;
        }
    }
}
