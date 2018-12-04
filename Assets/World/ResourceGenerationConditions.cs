using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheWorkforce.Items;

namespace TheWorkforce.World
{
    public struct ResourceGenerationConditions
    {
        public RawMaterial RawMaterial { get; private set; }

        public ResourceGenerationConditions(RawMaterial rawMaterial)
        {
            RawMaterial = rawMaterial;
        }
    }
}