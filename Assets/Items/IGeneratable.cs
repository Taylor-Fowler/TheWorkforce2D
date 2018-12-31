using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWorkforce.Items
{
    public interface IGeneratable
    {
        float MaximumMoisture { get; }
        float MinimumMoisture { get; }

        float MaximumElevation { get; }
        float MinimumElevation { get; }
        void InitialiseGeneratable(float maxMoisture, float minMoisture, float maxElevation, float minElevation);
        bool CanGenerate(float moisture, float elevation);
    }
}
