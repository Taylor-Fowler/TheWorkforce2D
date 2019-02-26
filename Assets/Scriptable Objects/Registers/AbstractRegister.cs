using UnityEngine;

namespace TheWorkforce.SOs.Registers
{
    using References;

    public abstract class AbstractRegister<T> : MonoBehaviour
    {
        [SerializeField] protected T m_ObjectReference;

        protected void Initialise(AbstractReference<T> m_ScriptableReference)
        {
            if (m_ScriptableReference != null)
            {
                m_ScriptableReference.Set(m_ObjectReference);
            }
        }
    }

    public abstract class AbstractRegister<TInterface, TConcrete> : MonoBehaviour where TConcrete : TInterface
    {
        [SerializeField] protected TConcrete m_ObjectReference;

        protected void Initialise(AbstractReference<TInterface> m_ScriptableReference)
        {
            if (m_ScriptableReference != null)
            {
                m_ScriptableReference.Set(m_ObjectReference);
            }
        }
    }
}
