﻿using UnityEngine;

namespace TheWorkforce
{
    [System.Serializable]
    public class Generatable
    {
        public float MaximumMoisture => _maximumMoisture;
        [Range(0, 1), SerializeField] private float _maximumMoisture;

        public float MinimumMoisture => _minimumMoisture;
        [Range(0, 1), SerializeField] private float _minimumMoisture;

        public float MaximumElevation => _maximumElevation;
        [Range(0, 1), SerializeField] private float _maximumElevation;

        public float MinimumElevation => _minimumElevation;
        [Range(0, 1), SerializeField] private float _minimumElevation;

        public ushort ItemId => _itemId;
        [SerializeField, ReadOnly] private ushort _itemId;

        public void Initialise(ushort itemId)
        {
            _itemId = itemId;
            Generation.Register(this);
        }

        public bool CanGenerate(float moisture, float elevation)
        {
            return moisture <= MaximumMoisture && moisture >= MinimumMoisture
                && elevation <= MaximumElevation && elevation >= MinimumElevation;
        }
    }
}
