using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWorkforce.World
{
    public class Generatable
    {
        public readonly float MaximumMoisture;
        public readonly float MinimumMoisture;

        public readonly float MaximumElevation;
        public readonly float MinimumElevation;

        public readonly ushort ItemId;

        public Generatable(float maximumMoisture, float minimumMoisture, float maximumElevation, float minimumElevation, ushort itemId)
        {
            MaximumMoisture = maximumMoisture;
            MinimumMoisture = minimumMoisture;
            MaximumElevation = maximumElevation;
            MinimumElevation = minimumElevation;
            ItemId = itemId;
        }

        public bool CanGenerate(float moisture, float elevation)
        {
            return moisture <= MaximumMoisture && moisture >= MinimumMoisture
                && elevation <= MaximumElevation && elevation >= MinimumElevation;
        }
    }
}
