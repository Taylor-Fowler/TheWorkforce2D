using System;
using System.Collections.Generic;

namespace TheWorkforce
{
    public struct PlayerData
    {
        public int Id { get; }
        public float X { get; }
        public float Y { get; }
        public List<Tuple<ushort, ushort>> InventoryItems;


        public PlayerData(int id, float x, float y)
        {
            Id = id;
            X = x;
            Y = y;

            InventoryItems = null;
        }

        public PlayerData(byte[] fileData)
        {
            int byteOffset = 0;
            // Read the Id field from file
            Id = BitConverter.ToInt32(fileData, 0);
            byteOffset += sizeof(int);

            X = BitConverter.ToSingle(fileData, byteOffset);
            byteOffset += sizeof(float);

            Y = BitConverter.ToSingle(fileData, byteOffset);
            byteOffset += sizeof(float);

            InventoryItems = new List<Tuple<ushort, ushort>>();

            // Loop through the rest of the file reading the itemId and itemCount for each item
            for (; byteOffset < fileData.Length;)
            {
                ushort itemId = BitConverter.ToUInt16(fileData, byteOffset);
                byteOffset += sizeof(ushort);

                ushort itemCount = BitConverter.ToUInt16(fileData, byteOffset);
                byteOffset += sizeof(ushort);

                InventoryItems.Add(new Tuple<ushort, ushort>(itemId, itemCount));
            }
        }

        public byte[] ByteArray()
        {
            // Make sure the inventory is initialised before including it as part of the player data
            int inventoryByteSize = (InventoryItems == null) ? 0 : (sizeof(ushort) * 2 * InventoryItems.Count);

            // id + x + y + items
            byte[] bytes = new byte[sizeof(int) + sizeof(float) + sizeof(float) + inventoryByteSize];

            // TODO: Revisit this to see how inefficient it is...
            int byteOffset = 0;
            BitConverter.GetBytes(Id).CopyTo(bytes, 0);
            byteOffset += sizeof(int);

            BitConverter.GetBytes(X).CopyTo(bytes, byteOffset);
            byteOffset += sizeof(float);
            BitConverter.GetBytes(Y).CopyTo(bytes, byteOffset);
            byteOffset += sizeof(float);

            int i = 0;
            for (; byteOffset < bytes.Length;)
            {
                BitConverter.GetBytes(InventoryItems[i].Item1).CopyTo(bytes, byteOffset);
                byteOffset += sizeof(ushort);

                BitConverter.GetBytes(InventoryItems[i].Item2).CopyTo(bytes, byteOffset);
                byteOffset += sizeof(ushort);
                ++i;
            }

            return bytes;
        }
    }

}