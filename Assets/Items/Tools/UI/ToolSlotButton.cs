using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheWorkforce.Items
{
    [RequireComponent(typeof(Image))]
    public class ToolSlotButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        #region Public Properties
        public ToolSlot Slot { get; private set; }
        #endregion

        #region Protected Members
        /// <summary>
        /// The image component that displays to the user as the tool held in the associated toolSlot
        /// </summary>
        [SerializeField] protected Image _toolImage;


        /// <summary>
        /// The image that displays the button background (container)
        /// </summary>
        [SerializeField] protected Image _backgroundImage;

        /// <summary>
        /// The default sprite used to display the button background
        /// </summary>
        [SerializeField] protected Sprite _defaultBackground;

        /// <summary>
        /// The sprite used to display the button background when focused
        /// </summary>
        [SerializeField] protected Sprite _focusedBackground;

        /// <summary>
        /// The sprite that indicates what type of tool is available to put in the slot
        /// </summary>
        protected Sprite _toolTypeSprite;

        protected bool _hasPointerFocus = false;
        #endregion

        public void LinkSlot(ToolSlot slot)
        {
            Slot = slot;
            _toolTypeSprite = Static_Classes.AssetProvider.EToolTypeSprites[(int)slot.Allowed];
            _toolImage.sprite = _toolTypeSprite;
        }

        protected void SetBackgroundImage()
        {
            _backgroundImage.sprite = (_hasPointerFocus) ? _focusedBackground : _defaultBackground;
        }

        #region IPointer Interface Implementation
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                //GiveItem();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hasPointerFocus = true;
            SetBackgroundImage();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hasPointerFocus = false;
            SetBackgroundImage();
            // TooltipView.Tooltip.Disable();
            // Linker.ItemDescription.Hide();
        }
        #endregion
    }
}
