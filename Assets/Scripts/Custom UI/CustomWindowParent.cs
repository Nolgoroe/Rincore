using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public abstract class CustomWindowParent : BasicUIElement
{
    //ui window is what we call the general parent of the UI prefab

    public virtual void ResetAllButtonEvents()
    {
        foreach (CustomButtonParent button in ButtonRefrences)
        {
            if(button != null)
            {
                button.buttonEvents = null;
            }
        }
    }
    public virtual void ActivateSpecificButton(CustomButtonParent button)
    {
        button.isInteractable = true;
    }
}
