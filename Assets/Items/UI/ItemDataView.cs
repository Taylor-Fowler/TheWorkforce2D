using TMPro;
using TheWorkforce.Items.Read_Only_Data;
using UnityEngine;
using UnityEngine.UI;

public class ItemDataView : MonoBehaviour
{
    public Image ItemImage;
    public TextMeshProUGUI ItemName;
    public TextMeshProUGUI ItemDescription;

    public TextMeshProUGUI FuelAmount;

    public void Display(ItemData itemData)
    {
        ItemImage.sprite = itemData.Sprite;
        ItemName.text = itemData.Name;
        ItemDescription.text = itemData.Description;
    }

    public void Display(FuelData fuelData)
    {
        Display((ItemData)fuelData);
        FuelAmount.text = fuelData.GetFuel().Value.ToString();
    }
}
