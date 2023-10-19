using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BasicCustomButton : CustomButtonParent
{
    public override void OnClickButton()
    {
        SoundManager.instance.CallPlaySound(sounds.ButtonClick);

        buttonEvents?.Invoke();

        buttonEventsInspector?.Invoke();

        if (isUseOnce)
        {
            DeactivateSpecificButton(this);
        }
    }

    public override void OverrideSetMyElement(string[] texts, Sprite[] sprites, System.Action[] actions = null)
    {
        base.SetMyElement(texts, sprites);
    }

}
