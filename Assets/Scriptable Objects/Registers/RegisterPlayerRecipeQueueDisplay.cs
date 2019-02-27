using UnityEngine;

namespace TheWorkforce.SOs.Registers
{
    using References;
    using Crafting;

    public class RegisterPlayerRecipeQueueDisplay : AbstractRegister<PlayerRecipeQueueDisplay>
    {
        [SerializeField] private PlayerRecipeQueueDisplayRef _reference;

        private void Awake()
        {
            Initialise(_reference);
        }
    } 
}
