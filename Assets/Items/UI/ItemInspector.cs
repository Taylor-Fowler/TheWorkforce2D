using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheWorkforce.UI;
using TheWorkforce.Items.Read_Only_Data;

namespace TheWorkforce
{
    public class ItemInspector : MonoBehaviour, IDisplay
    {
        public Image ItemImage;
        public TextMeshProUGUI ItemName;
        public TextMeshProUGUI ItemDescription;

        public void Display()
        {
            gameObject.SetActive(true);
        }

        public void Display(ItemData itemData)
        {

        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
