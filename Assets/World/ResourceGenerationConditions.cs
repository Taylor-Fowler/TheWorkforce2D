using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce
{
    public struct ResourceGenerationConditions
    {
        public Resource Resource { get; private set; }

        public ResourceGenerationConditions(Resource resource)
        {
            Resource = resource;
        }
    }
}