using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public abstract class CustomButtonParent : BasicUIElement, IPointerDownHandler
{
    /// <summary>
    /// only approachable/changable through code.
    /// the actions connected to this are reset when "ResetAllButtonEvents" is called
    /// </summary>
    public System.Action buttonEvents;


    /// <summary>
    /// this is not necessary, only if you want to use it, you can from the inspector.
    /// the actions connected to this are never reset! use this if you do not want the action to reset on window close
    /// </summary>
    public UnityEvent buttonEventsInspector; 



    [SerializeField] protected bool isUseOnce;

    private void OnMouseDown()
    {
        if (isInteractable && !UIManager.IS_DURING_TRANSITION && !UIManager.IS_USING_UI)
        {
            Debug.Log("HERE");
            // play click sound
            OnClickButton();
        }
    }

    public abstract void OnClickButton();

    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    if (isInteractable && !UIManager.IS_DURING_TRANSITION)
    //    {
    //        // play click sound
    //        OnClickButton();
    //    }

    //}

    public virtual void DeactivateSpecificButton(CustomButtonParent button)
    {
        button.isInteractable = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isInteractable && !UIManager.IS_DURING_TRANSITION)
        {
            // play click sound
            OnClickButton();
        }
    }
}
