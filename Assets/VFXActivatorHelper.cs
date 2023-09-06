using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum VFX
{
    bomb,
    joker
}

[System.Serializable]
public class VFXTypeObjectConnection
{
    public VFX effectType;
    public GameObject connectedObject;
}

public class VFXActivatorHelper : MonoBehaviour
{
    [SerializeField] VFXTypeObjectConnection[] VFXarray;

    public void PlayVFX(VFX VFXToEnable)
    {
        VFXTypeObjectConnection connection = VFXarray.Where(x => x.effectType == VFXToEnable).FirstOrDefault();

        if(connection != null)
        {
            connection.connectedObject.SetActive(true);
        }
    }
}