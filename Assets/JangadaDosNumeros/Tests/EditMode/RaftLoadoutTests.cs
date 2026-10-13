using JangadaDosNumeros.Core;
using NUnit.Framework;

namespace JangadaDosNumeros.Tests
{
    public sealed class RaftLoadoutTests
    {
        [Test]
        public void Place_BlocksDoubleOccupancy_AndRemoveFreesSlot()
        {
            var loadout = new RaftLoadout();

            Assert.That(loadout.TryPlace(0, CargoType.Peixe), Is.EqualTo(PlacementResult.Success));
            Assert.That(loadout.TryPlace(0, CargoType.Coco), Is.EqualTo(PlacementResult.Occupied));
            Assert.That(loadout.TryRemove(0, out var removed), Is.True);
            Assert.That(removed, Is.EqualTo(CargoType.Peixe));
            Assert.That(loadout.TryPlace(0, CargoType.Coco), Is.EqualTo(PlacementResult.Success));
        }

        [Test]
        public void Count_ReturnsTotalsByCargoType()
        {
            var loadout = new RaftLoadout();
            loadout.TryPlace(0, CargoType.Peixe);
            loadout.TryPlace(1, CargoType.Peixe);
            loadout.TryPlace(2, CargoType.Coco);

            Assert.That(loadout.Count(CargoType.Peixe), Is.EqualTo(2));
            Assert.That(loadout.Count(CargoType.Coco), Is.EqualTo(1));
            Assert.That(loadout.OccupiedCount, Is.EqualTo(3));
        }

        [Test]
        public void Clear_ResetsEverySlot()
        {
            var loadout = new RaftLoadout();
            loadout.TryPlace(0, CargoType.Cesto);
            loadout.TryPlace(7, CargoType.Fruta);

            loadout.Clear();

            Assert.That(loadout.OccupiedCount, Is.Zero);
            Assert.That(loadout.GetCargo(0), Is.Null);
            Assert.That(loadout.GetCargo(7), Is.Null);
        }
    }
}

