using System.Collections.Generic;
using JangadaDosNumeros.Core;
using NUnit.Framework;

namespace JangadaDosNumeros.Tests
{
    public sealed class MissionRulesTests
    {
        private MissionDefinition mission;

        [SetUp]
        public void SetUp()
        {
            mission = new MissionDefinition(new Dictionary<CargoType, int>
            {
                { CargoType.Peixe, 2 },
                { CargoType.Coco, 3 }
            });
        }

        [Test]
        public void Evaluate_ReturnsSuccessForExactAdditionMission()
        {
            var loadout = CreateCorrectLoadout();

            Assert.That(mission.RequiredTotal, Is.EqualTo(5));
            Assert.That(mission.Evaluate(loadout), Is.EqualTo(MissionResult.Success));
        }

        [Test]
        public void Evaluate_ReturnsIncorrectForMissingOrExtraCargo()
        {
            var missing = CreateCorrectLoadout();
            missing.TryRemove(4, out _);
            var extra = CreateCorrectLoadout();
            extra.TryPlace(5, CargoType.Fruta);

            Assert.That(mission.Evaluate(missing), Is.EqualTo(MissionResult.Incorrect));
            Assert.That(mission.Evaluate(extra), Is.EqualTo(MissionResult.Incorrect));
        }

        private static RaftLoadout CreateCorrectLoadout()
        {
            var loadout = new RaftLoadout();
            loadout.TryPlace(0, CargoType.Peixe);
            loadout.TryPlace(1, CargoType.Peixe);
            loadout.TryPlace(2, CargoType.Coco);
            loadout.TryPlace(3, CargoType.Coco);
            loadout.TryPlace(4, CargoType.Coco);
            return loadout;
        }
    }
}

