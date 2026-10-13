using System;
using System.Collections.Generic;

namespace JangadaDosNumeros.Core
{
    public enum MissionResult
    {
        Success,
        Incorrect
    }

    public sealed class MissionDefinition
    {
        private readonly int[] requiredCounts;

        public MissionDefinition(IDictionary<CargoType, int> requirements)
        {
            if (requirements == null)
            {
                throw new ArgumentNullException(nameof(requirements));
            }

            requiredCounts = new int[Enum.GetValues(typeof(CargoType)).Length];
            foreach (var requirement in requirements)
            {
                if (requirement.Value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(requirements));
                }

                requiredCounts[(int)requirement.Key] = requirement.Value;
            }
        }

        public int Required(CargoType cargo) => requiredCounts[(int)cargo];

        public int RequiredTotal
        {
            get
            {
                var total = 0;
                for (var i = 0; i < requiredCounts.Length; i++)
                {
                    total += requiredCounts[i];
                }

                return total;
            }
        }

        public MissionResult Evaluate(RaftLoadout loadout)
        {
            if (loadout == null)
            {
                throw new ArgumentNullException(nameof(loadout));
            }

            foreach (CargoType cargo in Enum.GetValues(typeof(CargoType)))
            {
                if (loadout.Count(cargo) != Required(cargo))
                {
                    return MissionResult.Incorrect;
                }
            }

            return MissionResult.Success;
        }
    }
}

