using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPowerUsable
{
    public virtual void SwitchPower()
    {
        //if we reach here it's default behaviour which is just release data directly
        PowerupManager.instance.ResetPowerUpData();
    }

    public virtual void BombPower()
    {
        //if we reach here it's default behaviour which is just release data directly
        PowerupManager.instance.ResetPowerUpData();
    }

    public virtual void JokerPower()
    {
        //if we reach here it's default behaviour which is just release data directly
        PowerupManager.instance.ResetPowerUpData();
    }

    public virtual bool CheckCanUsePower(PowerupType type)
    {
        PowerupManager.instance.ResetPowerUpData();

        return false;
    }
}
