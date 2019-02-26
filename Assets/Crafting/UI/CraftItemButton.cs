using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheWorkforce.Crafting.UI
{
    [RequireComponent(typeof(Image))]
    public class CraftItemButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Action<CraftingRecipe> OnCraft;
        public Action<CraftingRecipe, CraftItemButton> OnInspect;

        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _recipeImage;
        [SerializeField] private Sprite _hoverSprite;
        [SerializeField] private Sprite _defaultSprite;
        [SerializeField] private Sprite _activeSprite;

        private CraftingRecipe _attachedRecipe;
        private bool _isInspecting = false;
        
        public void AttachRecipe(CraftingRecipe recipeToAttach)
        {
            _attachedRecipe = recipeToAttach;
            _recipeImage.sprite = recipeToAttach.ItemProduced.Item.Sprite;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Left && !_isInspecting)
            {
                // Inspect
                OnInspect?.Invoke(_attachedRecipe, this);
                _backgroundImage.sprite = _activeSprite;
            }
            else if(eventData.button == PointerEventData.InputButton.Right)
            {
                // Craft
                OnCraft?.Invoke(_attachedRecipe);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(!_isInspecting)
            {
                _backgroundImage.sprite = _hoverSprite;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(!_isInspecting)
            {
                Default();
            }
        }

        public void Default()
        {
            _backgroundImage.sprite = _defaultSprite;
        }
    }

}