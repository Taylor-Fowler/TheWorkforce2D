using System;
using UnityEngine;

namespace TheWorkforce
{
    using Entities;

    [System.Serializable]
    public sealed class ItemStack
    {
        /// <summary>
        /// The item contained within the stack
        /// </summary>
        public EntityData Item => _item;
        [SerializeField] private EntityData _item;

        /// <summary>
        /// The number of items in the stack
        /// </summary>
        public ushort Count => _count;
        [SerializeField] private ushort _count;

        /// <summary>
        /// Constructor that initialises the number of items in the stack and the item contained in the stack
        /// </summary>
        /// <param name="item">The item that is stacked</param>
        /// <param name="count">The count of items in the stack</param>
        public ItemStack(EntityData item, ushort count)
        {
            if(item == null)
            {
                throw new NullReferenceException("ItemStack constructor parameter 'EntityData => item' is null");
            }
            _item = item;
            _count = count;
        }

        /// <summary>
        /// Copy constructor that copies the item and count from another stack of items
        /// </summary>
        /// <param name="toCopy">Item stack to copy</param>
        public ItemStack(ItemStack toCopy)
        {
            if(toCopy == null)
            {
                throw new NullReferenceException("ItemStack constructor parameter 'ItemStack => toCopy' is null");
            }
            Copy(toCopy);
        }

        /// <summary>
        /// Copies the values of one item stack into the current
        /// </summary>
        /// <param name="toCopy">The item stack to copy</param>
        public void Copy(ItemStack toCopy)
        {
            _item = toCopy.Item;
            _count = toCopy.Count;
        }

        /// <summary>
        /// Determines whether the item stack is empty
        /// </summary>
        /// <returns>True when the item stack has zero items</returns>
        public bool IsEmpty() => Count == 0;

        ///// <summary>
        ///// Tries to add an item stack to the current one, if the item in the stack being added does not match the current item stack then 
        ///// the addition will fail. If there is no space in the current item stack then the addition will also fail, otherwise, as much of the
        ///// other item stack will be added (without exceeding the current stacks max size). With the other being decreased accordingly.
        ///// </summary>
        ///// <param name="toAdd">The item stack to add</param>
        ///// <param name="stackModifier">The stack modifier of the item stack, allows the item stack to potentially exceed the items base max stack size</param>
        ///// <returns>True if any of the item stack is added</returns>
        //public bool Add(ItemStack toAdd, float stackModifier = 1.0f)
        //{
        //    // NOTE: This may cause headaches later...
        //    if (Item == null)
        //    {
        //        if(toAdd.Item == null)
        //        {
        //            return false;
        //        }
        //        _item = toAdd.Item;
        //    }
        //    // Only add the item if it is the same as the one in the stack
        //    else if (Item != toAdd.Item)
        //    {
        //        return false;
        //    }

        //    var space = SpaceLeft(stackModifier);

        //    // If there is no space at all in the stack then return immediately 
        //    if (space == 0)
        //    {
        //        return false;
        //    }

        //    // Check whether only some of the item can be added to the stack
        //    if (toAdd.Count > space)
        //    {
        //        _count += space;
        //        toAdd.Count -= space;
        //        return true;
        //    }

        //    Count += toAdd.Count;
        //    toAdd.Reset();
        //    return true;
        //}

        public ushort Add(ushort addAmount, float modifier = 1.0f)
        {
            // If there is not enough space to add the full amount, then reduce the amount
            // being added to the amount of space left
            addAmount = Math.Min(addAmount, SpaceLeft(modifier));

            _count += addAmount;
            return addAmount;
        }

        public ushort Subtract(ushort subtractAmount)
        {
            // If there are not enough items in the stack that are required:
            // Reduce the valid subtraction amount to be the count that we can provide
            if(_count < subtractAmount)
            {
                subtractAmount = _count;
            }
            _count -= subtractAmount;

            return subtractAmount;
        }

        /// <summary>
        /// Calculates how much space is left in the current item stack
        /// </summary>
        /// <param name="modifier">The modifier of the stack size, defaults to 1.0f</param>
        /// <returns>The number of spaces left in the stack</returns>
        public ushort SpaceLeft(float modifier = 1.0f)
        {
            return (ushort)((Item.MaxStackSize * modifier) - Count);
        }

        /// <summary>
        /// Compares the current item stack with another
        /// </summary>
        /// <param name="compare">The other item stack to compare</param>
        /// <returns>True if the two items in the item stack are the same</returns>
        public bool Equals(ItemStack compare)
        {
            return compare != null && Item.Equals(compare.Item);
        }

        /// <summary>
        /// Decrement operator which decreases the item stack count by 1
        /// </summary>
        /// <param name="value">The item stack to decrement</param>
        /// <returns></returns>
        public static ItemStack operator --(ItemStack value)
        {
            if (value.Count != 0)
            {
                value._count--;
            }
            return value;
        }
    }
}