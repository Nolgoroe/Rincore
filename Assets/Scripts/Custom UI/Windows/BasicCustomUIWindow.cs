using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BasicCustomUIWindow : CustomWindowParent
{
    public override void OverrideSetMyElement(string[] texts, Sprite[] sprites, Action[] actions)
    {
        base.SetMyElement(texts, sprites);

        if (ButtonRefrences.Length > 0 && actions != null)
        {
            ResetAllButtonEvents();

            for (int i = 0; i < ButtonRefrences.Length; i++)
            {
                if(ButtonRefrences[i] != null)
                {
                    ButtonRefrences[i].buttonEvents += actions[i];
                    ButtonRefrences[i].isInteractable = true;
                }
            }
        }
    }
}
