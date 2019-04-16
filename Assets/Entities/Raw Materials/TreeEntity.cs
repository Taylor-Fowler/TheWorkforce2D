using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce.Entities
{
    using Interactions; using Interfaces;

    public class TreeEntity : EntityInstance, IInteract
    {
        public ushort Amount;
        private readonly TreeData _data;

        public TreeEntity(uint id, int x, int y, Action<uint> onDestroy, TreeData data, ushort amount) : base(id, x, y, onDestroy)
        {
            Amount = amount;
            _data = data;
        }

        public TreeEntity(uint id, int x, int y, Action<uint> onDestroy, TreeData data)
            : this(id, x, y, onDestroy, data, 10)
        {
        }

        public override byte[] GetPacket()
        {
            byte[] bytes = new byte[_data.PacketSize()];
            Array.Copy(BitConverter.GetBytes(X), bytes, sizeof(int));
            Array.Copy(BitConverter.GetBytes(Y), 0, bytes, 4, sizeof(int));
            Array.Copy(BitConverter.GetBytes(Amount), 0, bytes, 8, sizeof(ushort));
            return bytes;
        }

        public override uint GetDataTypeId()
        {
            return _data.Id;
        }

        public override GameObject Spawn()
        {
            return _data.Template();
        }

        public override void Display(EntityView entityView)
        {
            _data.Display(entityView);
            entityView.SetDescription("Harvest Time: " + _data.TicksToHarvest.ToString());
            entityView.SetImageAmount(Amount);
        }

        public Interaction Interact(EntityInstance initiator)
        {
            IInventory inventory = initiator as IInventory;
            if (inventory != null)
            {
                return new HarvestInteraction(this, _data.Drop, DecreaseAmount, inventory, _data.TicksToHarvest);
            }
            return null;
        }

        public Interaction Interact(Player initiator)
        {
            IInventory inventory = initiator as IInventory;
            if (inventory != null)
            {
                return new HarvestInteraction(this, _data.Drop, DecreaseAmount, inventory, _data.TicksToHarvest);
            }
            return null;
        }

        private bool DecreaseAmount()
        {
            if (Amount == 0)
            {
                return false;
            }
            --Amount;
            OnDirty();

            if (Amount == 0)
            {
                Destroy();
            }
            return true;
        }

        public override EntityData GetData()
        {
            return _data;
        }
    }
}