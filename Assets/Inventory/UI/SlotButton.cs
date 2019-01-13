﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using TheWorkforce.Interfaces;

namespace TheWorkforce
{
    /// <summary>
    /// The SlotButton is a UI element that provides the user with an interface with the inventory currently
    /// associated with the button. An inventory button always exists as part of a `UI_Inventory` object; which
    /// is responsible for updating this buttons associated InventorySlot. The button allows the player to
    /// place and remove items in the slot and to display the details of the current slot to the player.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class SlotButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        //public SingleTextTooltipLinker TooltipView;
        //public ItemDescriptionLinker Linker;

        /// <summary>
        /// The Slot that is attached to this button, this button interfaces with the slot to
        /// update the contents of the slot
        /// </summary>
        public ISlot CorrespondingSlot { get; protected set; }

        /// <summary>
        /// The image component that displays to the user as the item held in the associated InventorySlot
        /// </summary>
        [SerializeField] protected Image _itemImage;

        /// <summary>
        /// The total count of the itme held in the associated InventorySlot
        /// </summary>
        [SerializeField] protected TextMeshProUGUI _itemCount;

        /// <summary>
        /// The image that gives a border and background image to the item count text
        /// </summary>
        [SerializeField] protected Image _itemCountBackgroundImage;

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

        protected bool _hasPointerFocus = false;

        #region Unity API
        private void Start()
        {
            EmptySlot();
        }
        #endregion

        public void LinkSlot(ISlot slot)
        {
            if (CorrespondingSlot != null)
            {
                CorrespondingSlot.SubscribeToDirty(UpdateDisplay);
            }

            CorrespondingSlot = slot;

            if (CorrespondingSlot != null)
            {
                CorrespondingSlot.UnsubscribeToDirty(UpdateDisplay);
            }

            UpdateDisplay(this);
        }

        public void UpdateDisplay(object source)
        {
            if (CorrespondingSlot != null && !CorrespondingSlot.IsEmpty())
            {
                ItemInSlot();
            }
            else
            {
                EmptySlot();
            }
        }

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

            //if (CorrespondingSlot != null && !CorrespondingSlot.IsEmpty())
            //{
            //    UpdateTooltip();
            //    Linker.ItemDescription.Show(this.CorrespondingSlot.ItemStack.Item);
            //}
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hasPointerFocus = false;
            SetBackgroundImage();
            // TooltipView.Tooltip.Disable();
            // Linker.ItemDescription.Hide();
        }

        protected virtual void EmptySlot()
        {
            _itemImage.enabled = false;
            _itemCount.text = "";
            _itemCountBackgroundImage.enabled = false;

            SetBackgroundImage();
        }

        protected virtual void ItemInSlot()
        {
            _itemImage.enabled = true;
            //_itemImage.sprite = CorrespondingSlot.ItemStack.Item.Icon;

            _itemCountBackgroundImage.enabled = true;
            _itemCount.text = CorrespondingSlot.ItemStack.Count.ToString();

            SetBackgroundImage();
        }

        protected void SetBackgroundImage()
        {
            _backgroundImage.sprite = (_hasPointerFocus) ? _focusedBackground : _defaultBackground;
        }

        //protected void UpdateTooltip()
        //{
        //    this.TooltipView.Tooltip.Title.text = this.CorrespondingSlot.ItemStack.Item.Name;
        //    this.TooltipView.Tooltip.Align((UnityEngine.RectTransform)this.transform);
        //    this.TooltipView.Tooltip.gameObject.SetActive(true);
        //}

        //protected virtual void GiveItem()
        //{
        //    //Player.Instance.Hand.HoldItem(this.UI_Inventory.Inventory, this.SlotID);
        //    Player.Instance.Hand.HoldItem(this.CorrespondingSlot);
        //}
    }
}