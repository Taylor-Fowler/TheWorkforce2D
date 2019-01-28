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
        [SerializeField] private TextMeshProUGUI _amount;
        [SerializeField] private Image _image;
        [SerializeField] private GameObject _amountImageObject;
        private EntityInstance m_viewingInstance;

        private void Start()
        {
            _link.View = this;
            Hide();
        }

        public void SetEntity(EntityInstance entityInstance)
        {
            if(m_viewingInstance != null)
            {
                ResetEntity();
            }
            if(entityInstance == null)
            {
                return;
            }
            m_viewingInstance = entityInstance;
            m_viewingInstance.DirtyHandler += EntityInstance_DirtyHandler;
            m_viewingInstance.OnEntityDestroy += ResetEntity;
            Display();
        }

        public void ResetEntity()
        {
            m_viewingInstance.DirtyHandler -= EntityInstance_DirtyHandler;
            m_viewingInstance.OnEntityDestroy -= ResetEntity;
            m_viewingInstance = null;
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

        public void SetImageAmount(ushort amount)
        {
            _amount.text = amount.ToString();
            _amountImageObject.SetActive(true);
        }

        public void Display()
        {
            gameObject.SetActive(true);
            EntityInstance_DirtyHandler();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _amountImageObject.SetActive(false);
        }

        private void EntityInstance_DirtyHandler()
        {
            m_viewingInstance.Display(this);
        }
    }

}