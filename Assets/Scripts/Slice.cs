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
    public bool isLock;
    public VFXActivatorHelper vfxHelper;

    [Header("Dynamic Data")]
    public SliceDisplay3D connectedDisplay;

    [Header("temp here?")]
    //TEMP - will maybe change to lock sprite animation.
    public Animator lockIconAnim;

    public void InitSlice(ConditonsData data, SliceConditionsEnums type, SubTileSymbol symbol, SubTileColor color, CellBase _sameIndexCell, CellBase _leftNeighborCell,  bool _isLock)
    {
        sliceData = data;
        connectionType = type;
        requiredSymbol = symbol;
        requiredColor = color;
        sameIndexCell = _sameIndexCell;
        leftNeighborCell = _leftNeighborCell;
        isLock = _isLock;
    }

    private void DestroySliceData()
    {
        Destroy(connectedDisplay.gameObject);

        ConditonsData sliceData = new ColorAndShapeCondition();
        connectionType = SliceConditionsEnums.None;
        requiredSymbol = SubTileSymbol.NoShape;
        requiredColor = SubTileColor.NoColor;

        if (lockIconAnim.gameObject.activeInHierarchy) // TEMP
        {
            lockIconAnim.gameObject.SetActive(false);
        }

        InitSlice(sliceData, connectionType, requiredSymbol, requiredColor, sameIndexCell, leftNeighborCell, false);


        if(sameIndexCell.heldTile)
        {
            //refresh all data by manually grabbing and placing each tile
            TileParentLogic tile = sameIndexCell.heldTile;
            sameIndexCell.GrabTileFrom();
            sameIndexCell.DroppedOn(tile, GameManager.gameRing);
        }

        if (leftNeighborCell.heldTile)
        {            
            //refresh all data by manually grabbing and placing each tile
            TileParentLogic tile = leftNeighborCell.heldTile;
            tile = leftNeighborCell.heldTile;
            leftNeighborCell.GrabTileFrom();
            leftNeighborCell.DroppedOn(tile, GameManager.gameRing);
        }

    }

    public void SetMidSprite(Sprite sprite)
    {
        lockIconAnim.gameObject.SetActive(true);
    }

    public void DoLockAnim(bool isLock)
    {
        if(isLock)
        {
            SoundManager.instance.CallPlaySound(sounds.Lock);

            lockIconAnim.SetTrigger("Lock Now");
            lockIconAnim.ResetTrigger("Unlock Now");
        }
        else
        {
            SoundManager.instance.CallPlaySound(sounds.UnLock);

            lockIconAnim.SetTrigger("Unlock Now");
            lockIconAnim.ResetTrigger("Lock Now");
        }
    }

    public bool CheckHasSliceData()
    {
        if (sliceData.onGoodConnectionActions == null)
        {
            return true;
        }

        return false;
    }


    public bool CheckCanUsePower(PowerupType type)
    {
        switch (type)
        {
            case PowerupType.Bomb:
                return CheckCanBomb();
            default:
                break;
        }


        return false;
    }

    public void BombPower()
    {
        StartCoroutine(PlayVFX(VFX.bomb));

        DestroySliceData();
        StartCoroutine(PowerupManager.instance.PowerSucceededUsing());
    }

    private IEnumerator PlayVFX(VFX vfxType)
    {
        yield return new WaitForSeconds(PowerupManager.instance.delayPotionEffectOnObject);

        if (vfxHelper)
        {
            vfxHelper.PlayVFX(vfxType);
        }
    }

    private bool CheckCanBomb()
    {
        if (connectedDisplay)
        {
            return true;
        }

        return false;
    }

}
