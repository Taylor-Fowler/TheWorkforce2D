using System.Collections;
using UnityEngine;

namespace TheWorkforce
{
    public class HudMenuOptions : MonoBehaviour
    {
        public DisplayHudOption InventoryHudOption => _inventoryHudOption;
        public DisplayHudOption CraftingHudOption => _craftingHudOption;

        [SerializeField] private DisplayHudOption _inventoryHudOption;
        [SerializeField] private DisplayHudOption _craftingHudOption;

        //private HudOption[] _hudOptions;
        private HudOption _selectedOption = null;

        #region Unity API
        private void Start()
        {
            _inventoryHudOption.Startup(this);
            _craftingHudOption.Startup(this);
            //_hudOptions = gameObject.GetComponentsInChildren<HudOption>();
            //Debug.Log("[HudMenuOptions] - Start() \n" 
            //        + "Number of Options: " + _hudOptions.Length.ToString());

            //foreach(var option in _hudOptions)
            //{
            //    option.SetMenuOptions(this);
            //    option.Deactivate();
            //}
        }
        #endregion

        public void SelectOption(HudOption hudOption)
        {
            if(_selectedOption != null)
            {
                _selectedOption.Deactivate();
            }

            if(_selectedOption == hudOption)
            {
                _selectedOption = null;
                return;
            }

            _selectedOption = hudOption;

            if(_selectedOption != null)
            {
                _selectedOption.Activate();
            }
        }
    }
}