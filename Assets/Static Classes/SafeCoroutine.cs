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

    public static class Vector2Conversions
    {
        public static Vector2Int Vec2Int(this Vector2 vector2)
        {
            return new Vector2Int((int)vector2.x, (int)vector2.y);
        }

        public static Vector2Int Vec2Int(this Vector3 vector3)
        {
            return new Vector2Int((int)vector3.x, (int)vector3.y);
        }
    }
}
