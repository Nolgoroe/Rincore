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

public class Slice : MonoBehaviour, IPowerUsable
{
    public int index;

    [Header("permanent data")]
    public ConditonsData sliceData;
    public SliceConditionsEnums connectionType;
    public SubTileSymbol requiredSymbol;
    public SubTileColor requiredColor;
    public CellBase sameIndexCell;
    public CellBase leftNeighborCell;

    [Header("Dynamic Data")]
    public SliceDisplay3D connectedDisplay;

    [Header("temp here?")]
    //TEMP - will maybe change to lock sprite animation.
    [SerializeField] private SpriteRenderer midIcon;

    public void InitSlice(ConditonsData data, SliceConditionsEnums type, SubTileSymbol symbol, SubTileColor color, CellBase _sameIndexCell, CellBase _leftNeighborCell,  bool isLock)
    {
        sliceData = data;
        connectionType = type;
        requiredSymbol = symbol;
        requiredColor = color;
        sameIndexCell = _sameIndexCell;
        leftNeighborCell = _leftNeighborCell;
    }

    private void DestroySliceData()
    {
        Destroy(connectedDisplay.gameObject);

        ConditonsData sliceData = new ColorAndShapeCondition();
        connectionType = SliceConditionsEnums.None;
        requiredSymbol = SubTileSymbol.NoShape;
        requiredColor = SubTileColor.NoColor;

        if (midIcon.gameObject.activeInHierarchy) // TEMP
        {
            midIcon.gameObject.SetActive(false);
        }

        InitSlice(sliceData, connectionType, requiredSymbol, requiredColor, sameIndexCell, leftNeighborCell, false);

        //refresh all data by manually grabbing and placing each tile
        TileParentLogic tile = sameIndexCell.heldTile;
        sameIndexCell.GrabTileFrom();
        sameIndexCell.DroppedOn(tile, GameManager.gameRing);

        tile = leftNeighborCell.heldTile;
        leftNeighborCell.GrabTileFrom();
        leftNeighborCell.DroppedOn(tile, GameManager.gameRing);

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

    public void BombPower()
    {
        DestroySliceData();
    }

}
