using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWorkforce.Items
{
    public interface IHarvestSettings
    {
        EToolType ToolRequired { get; }
        float Strength { get; }
        float BaseCapacity { get; }
        float CapacityModifier { get; }

        void InitialiseHarvestSettings(EToolType toolRequired, float strength, float baseCapacity, float capacityModifier);
    }
}
