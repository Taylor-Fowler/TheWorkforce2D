using UnityEngine;
using UnityEngine.UI;

namespace TheWorkforce
{
    public class EntityInteractionDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject _harvestBackground;
        [SerializeField] private Image _harvestCurrentAmount;

        public void DisplayHarvest(ushort ticksLeft, ushort ticksTotal)
        {
            _harvestBackground.SetActive(true);
            float amount = 1.0f - ((float)ticksLeft / ticksTotal);
            _harvestCurrentAmount.fillAmount = amount;
            Debug.Log("[EntityInteractionDisplay] - DisplayHarvest(ushort, ushort) \n"
                    + "amount: " + amount.ToString());
        }

        public void HideHarvest()
        {
            _harvestBackground.SetActive(false);
        }
    } 
}
