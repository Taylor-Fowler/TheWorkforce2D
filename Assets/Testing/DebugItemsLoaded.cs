using System.Collections.Generic;
using TheWorkforce.Items;
using UnityEngine;
using UnityEngine.UI;

namespace TheWorkforce.Testing
{
    [System.Serializable]
    public class DebugItemsLoaded
    {
        public Transform IdAnchor;
        public Transform NameAnchor;
        public Font TextFont;
        
        public void UpdateItems(IEnumerable<IItem> items)
        {
            foreach (var item in items)
            {
                {
                    GameObject gameObject = new GameObject();
                    Text text = gameObject.AddComponent<Text>();
                    text.text = item.Id.ToString();
                    text.font = TextFont;
                    text.fontSize = 18;
                    text.alignment = TextAnchor.MiddleRight;
                    text.color = Color.black;
                    gameObject.transform.SetParent(IdAnchor);
                    gameObject.transform.localScale = Vector3.one;
                }

                {
                    GameObject gameObject = new GameObject();
                    Text text = gameObject.AddComponent<Text>(); 
                    text.text = item.Name;
                    text.font = TextFont;
                    text.fontSize = 18;
                    text.alignment = TextAnchor.MiddleCenter;
                    text.color = Color.black;
                    gameObject.transform.SetParent(NameAnchor);
                    gameObject.transform.localScale = Vector3.one;
                }
            }
        }
    }
}