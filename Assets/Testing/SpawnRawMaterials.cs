using UnityEngine;
using TheWorkforce.Items;
using TheWorkforce.Crafting;
using TheWorkforce.World;

namespace TheWorkforce.Testing
{
    public class SpawnRawMaterials
    {
        public void Spawn(ItemManager itemManager, WorldController worldController)
        {
            if(worldController != null)
            {
                worldController.ChunkControllers[12]._tileControllers[30].SetItem(itemManager.RandomItem());
                Debug.Log("[SpawnRawMaterials] - Spawn(CraftingManager, WorldController");
            }
        }
    }
}