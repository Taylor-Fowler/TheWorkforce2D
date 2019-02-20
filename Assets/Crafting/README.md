# Crafting

The crafting system consists of:
- Recipes
- Processors (i.e. the objects that perform the crafting)

___
### Recipe
A recipe determines which items are required and which item is produced.

All crafting recipes will only produce one **_type_** of item, albeit multiple instances of that item; for example:

:heavy_check_mark: 1 x Furnace

:heavy_check_mark: 5 x Furnace

:heavy_check_mark: 1 x Iron Ingot

:x: 1 x Furnace, 1 x Iron Ingot

___
### Processor
A processor provides the interface for consuming recipe requirements in exchange for the recipe produce.
A processor may also have its own requirements which need to be fulfilled in conjunction with the recipe requirements.

For example:

- A furnace needs a fuel source in its fuel slot to process the recipe.
- A powered furnace needs power in its battery to process the receipe.
