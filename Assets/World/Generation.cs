using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce
{
    public static class Generation
    {
        private static readonly List<Generatable> _ascendingElevationRequirements = new List<Generatable>();
        private static readonly List<Generatable> _ascendingMoistureRequirements = new List<Generatable>();

        public static void Register(Generatable generatable)
        {
            int i = 0;

            for(; i < _ascendingElevationRequirements.Count; i++)
            {
                if(generatable.MinimumElevation < _ascendingElevationRequirements[i].MinimumElevation)
                {
                    break;
                }
                if(generatable.MinimumElevation == _ascendingElevationRequirements[i].MinimumElevation
                    && generatable.MaximumElevation < _ascendingElevationRequirements[i].MaximumElevation)
                {
                    break;
                }
            }

            _ascendingElevationRequirements.Insert(i, generatable);
            i = 0;

            for (; i < _ascendingMoistureRequirements.Count; i++)
            {
                if (generatable.MinimumMoisture < _ascendingMoistureRequirements[i].MinimumMoisture)
                {
                    break;
                }
                if (generatable.MinimumMoisture == _ascendingMoistureRequirements[i].MinimumMoisture
                    && generatable.MaximumMoisture < _ascendingMoistureRequirements[i].MaximumMoisture)
                {
                    break;
                }
            }
        }

        public static List<Generatable> GetGeneratables(float moisture, float elevation)
        {
            List<Generatable> _recentGeneratables = new List<Generatable>();
            //Debug.Log("[Generation] - GetGeneration(float, float) \n" 
            //        + "_ascendingElevationRequirements.Count: " + _ascendingElevationRequirements.Count.ToString());
            foreach(var generatable in _ascendingElevationRequirements)
            {
                if(generatable.CanGenerate(moisture, elevation))
                {
                    _recentGeneratables.Add(generatable);
                }
            }

            return _recentGeneratables;
        }
    }
}
