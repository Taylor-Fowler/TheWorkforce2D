using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce
{
    public class HudMenuOptions : MonoBehaviour
    {
        #region Properties
        public DisplayHudOption InventoryHudOption { get { return _inventoryHudOption; } }
        public DisplayHudOption ToolbeltHudOption { get { return _toolbeltHudOption; } }
        #endregion

        #region Private Members
        [SerializeField] private DisplayHudOption _inventoryHudOption;
        [SerializeField] private DisplayHudOption _toolbeltHudOption;

        private HudOption[] _hudOptions;
        private HudOption _selectedOption = null;
        #endregion

        #region Unity API
        private void Start()
        {
            _hudOptions = gameObject.GetComponentsInChildren<HudOption>();

            foreach(var option in _hudOptions)
            {
                option.SetMenuOptions(this);
                option.Deactivate();
            }
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