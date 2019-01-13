using System;

namespace TheWorkforce.Entities
{
    public class StoneEntity : EntityInstance
    {
        private readonly StoneData _data;

        public ushort Amount;

        public StoneEntity(uint id, StoneData data) : base(id)
        {
            _data = data;
        }

        public override void Display()
        {
            throw new NotImplementedException();
        }

        public override void Hide()
        {
            throw new NotImplementedException();
        }
    }
}
