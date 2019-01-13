namespace TheWorkforce.Scalars
{
    public struct Fuel
    {
        public float ConsumptionTime;
        public float Value;

        public Fuel(float time, float value)
        {
            ConsumptionTime = time;
            Value = value;
        }

        public float Rate()
        {
            return Value / ConsumptionTime;
        }
    }
}
