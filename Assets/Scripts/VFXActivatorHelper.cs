using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum VFX
{
    bomb,
    joker,
    sliceON
}

[System.Serializable]
public class VFXTypeObjectConnection
{
    public VFX effectType;
    public ParticleSystem connectedObject;
}

public class VFXActivatorHelper : MonoBehaviour
{
    [SerializeField] VFXTypeObjectConnection[] VFXarray;
    [SerializeField] SpriteRenderer boosterHighlightOnUse;

    public void PlayVFX(VFX VFXToEnable, bool _On)
    {
        VFXTypeObjectConnection connection = VFXarray.Where(x => x.effectType == VFXToEnable).FirstOrDefault();

        if (connection != null)
        {
            connection.connectedObject.gameObject.SetActive(_On);
            connection.connectedObject.Play();
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