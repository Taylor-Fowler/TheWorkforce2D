using TheWorkforce.Entities;

namespace TheWorkforce
{
    [System.Serializable]
    public sealed class ItemStack
    {
        /// <summary>
        /// The item contained within the stack
        /// </summary>
        public EntityData Item { get; private set; }

        /// <summary>
        /// The number of items in the stack
        /// </summary>
        public ushort Count { get; private set; }

        /// <summary>
        /// Constructor that initialises the number of items in the stack and the item contained in the stack
        /// </summary>
        /// <param name="item">The item that is stacked</param>
        /// <param name="count">The count of items in the stack</param>
        public ItemStack(EntityData item, ushort count)
        {
            Item = item;
            Count = count;
        }

        /// <summary>
        /// Copy constructor that copies the item and count from another stack of items
        /// </summary>
        /// <param name="toCopy">Item stack to copy</param>
        public ItemStack(ItemStack toCopy) : this(toCopy.Item, toCopy.Count)
        {
        }

        /// <summary>
        /// Copies the values of one item stack into the current
        /// </summary>
        /// <param name="toCopy">The item stack to copy</param>
        public void Copy(ItemStack toCopy)
        {
            Item = toCopy.Item;
            Count = toCopy.Count;
        }

        /// <summary>
        /// Determines whether the item stack is nullable
        /// </summary>
        /// <returns>True when the item stack has zero items</returns>
        public bool IsEmpty()
        {
            return Count == 0;
        }

        /// <summary>
        /// Tries to add an item stack to the current one, if the item in the stack being added does not match the current item stack then 
        /// the addition will fail. If there is no space in the current item stack then the addition will also fail, otherwise, as much of the
        /// other item stack will be added (without exceeding the current stacks max size). With the other being decreased accordingly.
        /// </summary>
        /// <param name="toAdd">The item stack to add</param>
        /// <param name="stackModifier">The stack modifier of the item stack, allows the item stack to potentially exceed the items base max stack size</param>
        /// <returns>True if any of the item stack is added</returns>
        public bool Add(ItemStack toAdd, float stackModifier = 1.0f)
        {
            // NOTE: This may cause headaches later...
            if (Item == null)
            {
                if(toAdd.Item == null)
                {
                    return false;
                }
                Item = toAdd.Item;
            }
            // Only add the item if it is the same as the one in the stack
            else if (Item != toAdd.Item)
            {
                return false;
            }

            var space = SpaceLeft(stackModifier);

            // If there is no space at all in the stack then return immediately 
            if (space == 0)
            {
                return false;
            }

            // Check whether only some of the item can be added to the stack
            if (toAdd.Count > space)
            {
                Count += space;
                toAdd.Count -= space;
                return true;
            }

            Count += toAdd.Count;
            toAdd.Reset();
            return true;
        }

        /// <summary>
        /// Calculates whether there is any space left in the current item stack
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
            return Item.Equals(compare.Item);
        }

        // TODO: Remove
        public void Reset()
        {
            Item = null;
            Count = 0;
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
                value.Count--;
            }

            if (value.Count == 0)
            {
                value.Item = null;
            }

            return value;
        }


        // TODO: Remove
        //public bool IsFull(uint stackSize)
        //{
        //    return Count == stackSize;
        //}

        // TODO: Remove/Reimplement
        //public void Display(InventoryButton button)
        //{
        //    button.ItemCount.text = this.Count.ToString();
        //    Item.Display(button);
        //}
    }
}