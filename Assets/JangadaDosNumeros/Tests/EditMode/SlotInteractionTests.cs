using JangadaDosNumeros.Core;
using NUnit.Framework;

namespace JangadaDosNumeros.Tests
{
    public sealed class SlotInteractionTests
    {
        [Test]
        public void Apply_WithSelectionAndOccupiedSlot_PreservesBothCargos()
        {
            var loadout = new RaftLoadout();
            loadout.TryPlace(0, CargoType.Peixe);
            CargoType? selected = CargoType.Coco;

            var action = SlotInteraction.Apply(loadout, 0, ref selected);

            Assert.That(action, Is.EqualTo(SlotInteractionAction.RejectedOccupied));
            Assert.That(selected, Is.EqualTo(CargoType.Coco));
            Assert.That(loadout.GetCargo(0), Is.EqualTo(CargoType.Peixe));
        }

        [Test]
        public void Apply_WithoutSelectionAndOccupiedSlot_TakesCargoForRepositioning()
        {
            var loadout = new RaftLoadout();
            loadout.TryPlace(0, CargoType.Peixe);
            CargoType? selected = null;

            var action = SlotInteraction.Apply(loadout, 0, ref selected);

            Assert.That(action, Is.EqualTo(SlotInteractionAction.TookExisting));
            Assert.That(selected, Is.EqualTo(CargoType.Peixe));
            Assert.That(loadout.GetCargo(0), Is.Null);
        }

        [Test]
        public void Apply_WithSelectionAndFreeSlot_PlacesAndClearsSelection()
        {
            var loadout = new RaftLoadout();
            CargoType? selected = CargoType.Coco;

            var action = SlotInteraction.Apply(loadout, 3, ref selected);

            Assert.That(action, Is.EqualTo(SlotInteractionAction.PlacedSelected));
            Assert.That(selected, Is.Null);
            Assert.That(loadout.GetCargo(3), Is.EqualTo(CargoType.Coco));
        }
    }
}
