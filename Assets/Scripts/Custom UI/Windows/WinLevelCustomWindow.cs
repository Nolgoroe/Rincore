using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WinLevelCustomWindow : BasicCustomUIWindow
{
    public override void OverrideSetMyElement(string[] texts, Sprite[] sprites, Action[] actions)
    {
        base.SetMyElement(texts, sprites);

        if (ButtonRefrences.Length > 0)
        {
            ResetAllButtonEvents();

            for (int i = 0; i < ButtonRefrences.Length; i++)
            {
                ButtonRefrences[i].buttonEvents += actions[i];
                ButtonRefrences[i].isInteractable = true;
            }
        }
    }
}
