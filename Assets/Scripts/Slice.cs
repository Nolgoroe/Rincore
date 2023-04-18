using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum SliceConditionsEnums
{
    None,
    GeneralColor,
    GeneralSymbol,
    SpecificColor,
    SpecificSymbol,
}

public class Slice : MonoBehaviour
{
    public int index;

    [Header("permanent data")]
    public ConditonsData sliceData;
    public SliceConditionsEnums connectionType;
    public SubTileSymbol requiredSymbol;
    public SubTileColor requiredColor;


    [Header("temp here?")]
    //TEMP - will maybe change to lock sprite animation.
    [SerializeField] private SpriteRenderer midIcon;

    public void InitSlice(ConditonsData data, SliceConditionsEnums type, SubTileSymbol symbol, SubTileColor color, bool isLock)
    {
        sliceData = data;
        connectionType = type;
        requiredSymbol = symbol;
        requiredColor = color;
    }

    public void SetMidSprite(Sprite sprite)
    {
        midIcon.sprite = sprite;
        midIcon.gameObject.SetActive(true);
    }

    public bool CheckHasSliceData()
    {
        if (sliceData.onGoodConnectionActions == null)
        {
            return true;
        }

        return false;
    }
}
