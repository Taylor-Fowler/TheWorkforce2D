using System;
using UnityEngine;

namespace TheWorkforce.SOs.References
{
    public abstract class AbstractReference<T> : ScriptableObject
    {
        public event Action<T, T> ReferenceUpdated;
        [SerializeField] private T m_reference;

        public T Get()
        {
            return m_reference;
        }

        public void Set(T newReference)
        {
            T oldReference = m_reference;
            m_reference = newReference;

            if (ReferenceUpdated != null)
            {
                ReferenceUpdated.Invoke(oldReference, m_reference);
            }
        }
    }
}