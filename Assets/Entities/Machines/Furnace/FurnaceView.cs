using UnityEngine;

namespace TheWorkforce.Entities.Views
{
    // inherit from a view interface...
    // if the player is x away from object that interface is for, then close the interface
    // if the player presses key `k` then close the interface
    // if the player tries to open another interface, close the current view
    public class FurnaceView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private SlotButton _ingredientInput;
        [SerializeField] private SlotButton _fuelInput;
        [SerializeField] private SlotButton _produceOutput;

        private FurnaceEntity _furnace;

        public void SetFurnace(FurnaceEntity furnace)
        {
            _furnace = furnace;
            _ingredientInput.SetSlot(_furnace.Input);
            _fuelInput.SetSlot(_furnace.FuelSlot);
            _produceOutput.SetSlot(_furnace.Output);
        }

        public void Open()
        {
            _panel.SetActive(true);
        }

        public void Close()
        {
            _panel.SetActive(false);
        }



        // hover over it displays the ui in the corner with description
        // click on it opens the interface
        // one button for item input
        // one button for fuel input
        // one button for 
    }
}
