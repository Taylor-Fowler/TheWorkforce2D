using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TheWorkforce.UI
{
    public class FillTransition
    {
        //public IEnumerator StartTransitionHorizontally(RectTransform rectTransform, float timer, bool moveLeft, float distance)
        //{
        //    float speedPerSecond = distance / timer * ((moveLeft) ? -1f : 1f);

        //    while (timer > 0f)
        //    {
        //        timer -= Time.deltaTime;
        //        rectTransform.Translate(new Vector3(speedPerSecond * Time.deltaTime, 0f, 0f));
        //        yield return null;
        //    }
        //}

        public IEnumerator StartTransitionVertically(Image image, float timer)
        {
            float speedPerSecond = 1.0f / timer;
            float fillAmount = 0.0f;

            while(timer > 0f)
            {
                timer -= Time.deltaTime;
                fillAmount += speedPerSecond * Time.deltaTime;
                image.fillAmount = fillAmount;
                yield return null;
            }
        }

        public void ManualTransition(Image image, float timeRequired, float timeProcessed)
        {
            image.fillAmount = timeProcessed / timeRequired;
        }
    }
}
