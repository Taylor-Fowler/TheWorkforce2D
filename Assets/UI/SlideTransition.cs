using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce.UI
{
    public class SlideTransition
    {
        public IEnumerator StartTransitionHorizontally(RectTransform rectTransform, float timer, bool moveLeft, float distance)
        {
            float speedPerSecond = distance / timer * ((moveLeft)? -1f : 1f);

            while(timer > 0f)
            {
                timer -= Time.deltaTime;
                rectTransform.Translate(new Vector3(speedPerSecond * Time.deltaTime, 0f, 0f));
                yield return null;       
            }
        }
    }
}
