namespace TheWorkforce.Items
{
    public class BaseGenerationSettings : IGenerationSettings
    {
        #region IGeneratable Members
        public float MaximumMoisture { get; private set; }
        public float MinimumMoisture { get; private set; }
        public float MaximumElevation { get; private set; }
        public float MinimumElevation { get; private set; }

        public void InitialiseGenerationSettings(float maxMoisture, float minMoisture, float maxElevation, float minElevation)
        {
            MaximumMoisture = maxMoisture;
            MinimumMoisture = minMoisture;
            MaximumElevation = maxElevation;
            MinimumElevation = minElevation;
        }

        public bool CanGenerate(float moisture, float elevation)
        {
            return moisture >= MinimumMoisture
                && moisture <= MaximumMoisture
                && elevation >= MinimumElevation
                && elevation <= MaximumElevation;
        }
        #endregion
    }
}
