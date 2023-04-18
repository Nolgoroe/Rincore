using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIElementDisplayerSegment : BasicUIElement
{    
    public override void OverrideSetMyElement(string[] texts, Sprite[] sprites, System.Action[] actions = null)
    {
        base.SetMyElement(texts, sprites);
    }
}
