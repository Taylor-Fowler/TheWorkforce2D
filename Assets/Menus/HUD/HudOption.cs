using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TheWorkforce
{
    [RequireComponent(typeof(Image))]
    public class HudOption : MonoBehaviour, IPointerClickHandler
    {
        public Sprite ActiveSprite;
        public Sprite InactiveSprite;

        protected Image _image;
        protected HudMenuOptions _hudMenuOptions;

        protected virtual void Awake()
        {
            _image = GetComponent<Image>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _hudMenuOptions.SelectOption(this);
        }

        public void SetMenuOptions(HudMenuOptions hudMenuOptions)
        {
            _hudMenuOptions = hudMenuOptions;
        }

        public virtual void Activate()
        {
            _image.sprite = ActiveSprite;
        }

        public virtual void Deactivate()
        {
            _image.sprite = InactiveSprite;
        }
    }
}
