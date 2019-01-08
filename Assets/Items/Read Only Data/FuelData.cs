using UnityEngine;

namespace TheWorkforce.Items.Read_Only_Data
{
    public class FuelData : ItemData, IFuel
    {
        private Fuel _fuel;

        public FuelData(string name, string description, Sprite sprite, Fuel fuel, byte maxStackSize = 1) 
            : base(name, description, sprite, maxStackSize)
        {
            _fuel = fuel;
        }

        public Fuel GetFuel()
        {
            return _fuel;
        }
    }
}
