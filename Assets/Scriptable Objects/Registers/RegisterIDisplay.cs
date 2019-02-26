using UnityEngine;
using UnityEditor;

namespace TheWorkforce.SOs.Registers
{
    using References;

    public class RegisterIDisplay<T> : AbstractRegister<IDisplay, T> where T : IDisplay
    {
        [SerializeField] private IDisplayRef _reference;

        private void Awake()
        {
            Initialise(_reference);
        }
    }
}
