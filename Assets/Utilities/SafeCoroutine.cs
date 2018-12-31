using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce
{
    public static class SafeCoroutine
    {
        public static Coroutine StartSafeCoroutine(this MonoBehaviour monoBehaviour, IEnumerator routine, Action callback)
        {
            return monoBehaviour.StartCoroutine(RunSafeCoroutine(monoBehaviour, routine, callback));
        }

        private static IEnumerator RunSafeCoroutine(MonoBehaviour monoBehaviour, IEnumerator routine, Action callback)
        {
            yield return monoBehaviour.StartCoroutine(routine);
            callback();
        }
    }
}
