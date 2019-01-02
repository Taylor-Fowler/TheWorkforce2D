namespace TheWorkforce.Items
{
    public interface IGenerationSettings
    {
        float MaximumMoisture { get; }
        float MinimumMoisture { get; }

        float MaximumElevation { get; }
        float MinimumElevation { get; }
        void InitialiseGenerationSettings(float maxMoisture, float minMoisture, float maxElevation, float minElevation);
        bool CanGenerate(float moisture, float elevation);
    }
}
