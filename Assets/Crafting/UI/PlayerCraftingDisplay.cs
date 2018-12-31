using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TheWorkforce.UI;

namespace TheWorkforce.Crafting
{
    //using TheWorkforce;

    public class PlayerCraftingDisplay : MonoBehaviour, IDisplay
    {
        #region Public Members
        public float TimeToDisplay = 1f;
        #endregion

        #region Private Members
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private RectTransform _craftingPanel;
        [SerializeField] private Button _displayButton;
        private Coroutine _slidingDisplay;
        private bool _isDisplaying;
        #endregion

        #region Unity API
        private void Start()
        {
            if(_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
            _isDisplaying = false;
            _slidingDisplay = null;
            _displayButton.onClick.AddListener(ToggleDisplay);
        }
        #endregion

        private void ToggleDisplay()
        {
            if(_slidingDisplay == null)
            {
                _isDisplaying = !_isDisplaying;
                _slidingDisplay = this.StartSafeCoroutine(new SlideTransition().StartTransitionHorizontally(_rectTransform, TimeToDisplay, _isDisplaying, _craftingPanel.rect.width),
                    delegate
                    {
                        _slidingDisplay = null;
                    }); 
            }
        }

        #region IDisplay Implementation
        public void Display()
        {
            
        }

        public void Hide()
        {
            
        }
        #endregion
    }
}