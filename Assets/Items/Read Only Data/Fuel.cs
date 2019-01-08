namespace TheWorkforce.Items.Read_Only_Data
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
