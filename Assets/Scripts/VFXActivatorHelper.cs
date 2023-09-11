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
    [SerializeField] SpriteRenderer boosterHighlightOnUse;

    public void PlayVFX(VFX VFXToEnable)
    {
        VFXTypeObjectConnection connection = VFXarray.Where(x => x.effectType == VFXToEnable).FirstOrDefault();

        if(connection != null)
        {
            connection.connectedObject.SetActive(true);
        }
    }

    public void EnableBoosterHighlight(bool _isEnabled, Color wantedColor)
    {
        boosterHighlightOnUse.gameObject.SetActive(_isEnabled);
        boosterHighlightOnUse.color = wantedColor;
    }

    public void SetHighlightSprite(Sprite sprite)
    {
        boosterHighlightOnUse.sprite = sprite;
    }
}