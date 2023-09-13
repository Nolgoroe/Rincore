using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionClickableZoneCustomButton : ClickableZoneCustomButton
{
    public void CallUICloseElement()
    {
        PowerupManager.instance.ResetPowerUpData();
        if (connectedParent == null)
        {
            Debug.LogError("Parent isn't a basic ui element!!!");
            return;
        }

        UIManager.instance.CloseElement(connectedParent);
    }

}
