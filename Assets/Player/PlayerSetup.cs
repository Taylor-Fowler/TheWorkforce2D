using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UObject = UnityEngine.Object;
using UnityEngine;

namespace TheWorkforce
{
    using Entities; using Crafting; using SOs.References;

    [System.Serializable]
    public class PlayerSetup
    {
        public GameObject CameraPrefab;
        public GameObject MouseControllerPrefab;
        public GameObject PlayerCraftingPrefab;
        public EntityViewLink EntityViewLink;
        public PlayerInventoryDisplayRef PlayerInventoryDisplayRef;


        public MouseController AddComponents(PlayerController playerController, WorldController worldController)
        {
            var camera = UObject.Instantiate(CameraPrefab, playerController.transform).GetComponent<Camera>();
            var mouseController = UObject.Instantiate(MouseControllerPrefab, playerController.transform).GetComponent<MouseController>();
            var playerCrafting = UObject.Instantiate(PlayerCraftingPrefab, playerController.transform).GetComponent<PlayerCrafting>();

            mouseController.Initialise(playerController.Player, camera, worldController, EntityViewLink.View);
            PlayerInventoryDisplayRef.Get().SetInventory(playerController.Player.Inventory);
            PlayerInventoryDisplayRef.Get().Hide();

            playerCrafting.Initialise(playerController.Player.Inventory);

            return mouseController;
        }
    }
}
