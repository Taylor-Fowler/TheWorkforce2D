using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TheWorkforce.Entities
{
    public class EntityView : MonoBehaviour
    {
        [SerializeField] private EntityViewLink _link;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _description;
        [SerializeField] private Image _image;

        private void Start()
        {
            _link.View = this;
            Hide();
        }

        public void SetTitle(string title)
        {
            _title.text = title;
        }

        public void SetDescription(string description)
        {
            _description.text = description;
        }

        public void SetImage(Sprite sprite)
        {
            _image.sprite = sprite;
        }

        public void Display()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }

}