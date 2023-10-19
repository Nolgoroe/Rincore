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
    [SerializeField] protected bool effectDisplayOnClick = true;


    private float timeToShrink = 1;
    private float timeToGrow = 1;
    private float timeToRevertClick = 1;
    private float shrinkBySize = 0.8f;

    private void Start()
    {
        effectDisplayOnClick = true;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
    private void OnMouseDown()
    {
        if (isInteractable && 
            !UIManager.IS_DURING_TRANSITION &&
            !UIManager.IS_USING_UI && 
            !UIManager.IS_DURING_POTION_USAGE)
        {
            // play click sound
            //SoundManager.instance.CallPlaySound(sounds.ButtonClick);
            OnClickButton();

            if (effectDisplayOnClick && isActiveAndEnabled)
            {
                StartCoroutine(AnimateClick());
            }
        }
    }

    public abstract void OnClickButton();

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

            if (effectDisplayOnClick && isActiveAndEnabled)
            {
                StartCoroutine(AnimateClick());
            }
        }
    }

    private IEnumerator AnimateClick()
    {
        // these numbers were tested in the scene - these are the decided values.
        // if we wish to change these values again, make these values serializable again.
        // we did this so we don't need to change every single button in the scene.
        timeToShrink = 0.075f;
        timeToGrow = 0.075f;
        timeToRevertClick = 0.1f;
        shrinkBySize = 0.95f;

        Vector3 currentSize = transform.localScale;
        Vector3 newSize = currentSize * shrinkBySize;

        LeanTween.scale(gameObject, newSize, timeToShrink);
        yield return new WaitForSeconds(timeToRevertClick);
        LeanTween.scale(gameObject, currentSize, timeToGrow);

    }
}
