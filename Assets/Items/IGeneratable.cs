using TheWorkforce.World;

namespace TheWorkforce.Items
{
    public interface IGenerationRequirements
    {
        BasicGenerationRequirements GenerationRequirements { get; }
    }

    public class BasicGenerationRequirements
    {
        private float _maximumMoisture;
        private float _minimumMoisture;
        private float _maximumElevation;
        private float _minimumElevation;

        public ItemInstance ItemToGenerate;
        

        public BasicGenerationRequirements(float maxMoisture, float minMoisture, float maxElevation, float minElevation)
        {
            _maximumMoisture = maxMoisture;
            _minimumMoisture = minMoisture;
            _maximumElevation = maxElevation;
            _minimumElevation = minElevation;
        }

        public bool CanGenerate(Tile tile)
        {
            return tile.Moisture >= _minimumMoisture
                && tile.Moisture <= _maximumMoisture
                && tile.Elevation >= _minimumElevation
                && tile.Elevation <= _maximumElevation;
        }
    }

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
