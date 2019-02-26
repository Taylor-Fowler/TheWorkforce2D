using UnityEngine;

namespace TheWorkforce.SOs.Registers
{
    using References;
    using Crafting;

    public class RegisterPlayerCraftingDisplay : AbstractRegister<PlayerCraftingDisplay>
    {
        [SerializeField] private PlayerCraftingDisplayRef _reference;

        private void Awake()
        {
            Initialise(_reference);
        }
    }

}