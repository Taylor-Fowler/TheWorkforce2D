using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class MyButton : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool Interactable = true;
    // Determine whether we want to use the exit functionality for the pointer up state
    public bool UseExitAsUp = true;
    
    public UnityEvent OnClick;
    public UnityEvent OnUp;
    public UnityEvent OnMouseEnter;
    public UnityEvent OnMouseExit;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(Interactable)
        {
            BeforeOnClick();
            OnClick?.Invoke();
            AfterOnClick();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(Interactable)
        {
            BeforeOnEnter();
            OnMouseEnter?.Invoke();
            AfterOnEnter();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Interactable)
        {
            BeforeOnExit();
            OnMouseExit?.Invoke();
            AfterOnExit(); 
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Interactable)
        {
            BeforeOnUp();
            if (UseExitAsUp)
            {
                OnPointerExit(eventData);
            }
            OnUp?.Invoke();
            AfterOnUp(); 
        }
    }

    protected virtual void BeforeOnClick() { }
    protected virtual void AfterOnClick() { }

    protected virtual void BeforeOnEnter() { }
    protected virtual void AfterOnEnter() { }

    protected virtual void BeforeOnExit() { }
    protected virtual void AfterOnExit() { }

    protected virtual void BeforeOnUp() { }
    protected virtual void AfterOnUp() { }

}
