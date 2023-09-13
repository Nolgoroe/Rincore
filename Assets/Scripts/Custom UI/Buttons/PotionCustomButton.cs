using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionCustomButton : CustomButtonParent
{
    [SerializeField] public Vector3 originalPos { get; private set; }

    public void SetOriginalPos()
    {
        originalPos = GetComponent<RectTransform>().anchoredPosition; // is this good to expect a rect transform?
    }
    public override void OnClickButton()
    {
        //SoundManager.instance.CallPlaySound(sounds.ButtonClick);

        buttonEvents?.Invoke();

        buttonEventsInspector?.Invoke();

        if (isUseOnce)
        {
            DeactivateSpecificButton(this);
        }
    }

    public override void OverrideSetMyElement(string[] texts, Sprite[] sprites, Action[] actions)
    {
        base.SetMyElement(texts, sprites);

        if (ButtonRefrences.Length > 0 && actions != null)
        {
            for (int i = 0; i < ButtonRefrences.Length; i++)
            {
                ButtonRefrences[i].buttonEvents += actions[i];
                ButtonRefrences[i].isInteractable = true;
            }
        }
    }
}
