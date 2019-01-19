using System;
using UnityEngine;

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

        public override GameObject Spawn()
        {
            return _data.Template();
        }

        public override void Display()
        {
            _data.Display();
        }

        public override void Hide()
        {
            _data.Hide();
        }
    }
}
