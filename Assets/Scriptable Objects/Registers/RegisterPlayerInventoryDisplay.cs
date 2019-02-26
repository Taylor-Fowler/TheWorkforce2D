using UnityEngine;

namespace TheWorkforce.SOs.Registers
{
    using References;

    public class RegisterPlayerInventoryDisplay : AbstractRegister<PlayerInventoryDisplay>
    {
        [SerializeField] private PlayerInventoryDisplayRef _reference;

        private void Awake()
        {
            Initialise(_reference);
        }
    } 
}
