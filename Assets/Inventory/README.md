# Inventory

The inventory system consists of:
- ItemStack
- Slots
- SlotCollections

___
## ItemStack
An ItemStack is the simplest form of representing a "held" item. 

An ItemStack can be:
- Something that the player is holding
- Contained within an inventory
   - Player Inventory
   - Furnace Inventory

The ItemStack is directly dependant on EntityData from the Entity system as the data is what defines the content of the stack.

An ItemStack has two components:
- Item - what is in the stack
- Count - the 'size' of the stack

For example, 5 pieces of stone are represented as:
- Stone
- 5

An ItemStack can be merged with another ItemStack if both Items match.
I.E. 5 pieces of stone can be merged with 5 pieces of stone to result in 10 pieces.

___
## Slots


___
## SlotCollections