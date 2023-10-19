using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CellBase : TileHolder, IGrabTileFrom, IPowerUsable
{
    public CellBase leftCell, rightCell;

    public Slice leftSlice, rightSlice;

    public ImageSwapHelper connectVFXLeft, connectVFXRight;

    // by default when we put a tile and there is no contested tile - it will be considered a "bad" connection
    // to prevent hightlights of "good connections".
    [SerializeField] private bool goodConnectLeft, goodConnectRight;
    [SerializeField] private int amountUnsuccessfullConnections;

    //TEMP
    [SerializeField]
    int maxDistanceToAnimate;
    [SerializeField]
    int maxAnimateSpeed;

    // think about creating an action system here aswell for "on good connection" + "on bad connection" - look at gamemanger as example.

    public override void RecieveTileDisplayer(TileParentLogic tileToPlace)
    {
        AcceptTileToHolder(tileToPlace); 

        CheckConnections();
    }

    public override void OnRemoveTileDisplay()
    {
        if (leftCell.heldTile)
        {
            heldTile.SetSubtilesConnectedGFX(false, heldTile.subTileLeft, leftCell.heldTile.subTileRight);
            connectVFXLeft.gameObject.SetActive(false);
        }

        if (rightCell.heldTile)
        {
            heldTile.SetSubtilesConnectedGFX(false, heldTile.subTileRight, rightCell.heldTile.subTileLeft);
            connectVFXRight.gameObject.SetActive(false);
        }
    }

    public override void RemoveTile()
    {
    }

    public void CheckConnections()
    {
        amountUnsuccessfullConnections = 0;

        bool goodRight = false;
        bool goodLeft = false;

        if (leftCell.heldTile)
        {
            connectVFXLeft.gameObject.SetActive(true);

            goodLeft = leftSlice.sliceData.CheckCondition(heldTile.subTileLeft, leftCell.heldTile.subTileRight);
            if (!goodLeft)
            {
                //bad connection if we're inside here.
                amountUnsuccessfullConnections++;

                heldTile.ShakeNow();
                leftCell.heldTile.ShakeNow();
            }

            SetConnectDataOnPlace(goodLeft, true, heldTile.subTileLeft, leftCell.heldTile.subTileRight, leftSlice);
        }
        else
        {
            connectVFXLeft.gameObject.SetActive(false);
        }

        if (rightCell.heldTile)
        {
            connectVFXRight.gameObject.SetActive(true);

            goodRight = rightSlice.sliceData.CheckCondition(heldTile.subTileRight, rightCell.heldTile.subTileLeft);
            if (!goodRight)
            {
                //bad connection if we're inside here.
                amountUnsuccessfullConnections++;
                heldTile.ShakeNow();
                rightCell.heldTile.ShakeNow();
            }

            SetConnectDataOnPlace(goodRight, false, heldTile.subTileRight, rightCell.heldTile.subTileLeft, rightSlice);     
        }
        else
        {
            connectVFXRight.gameObject.SetActive(false);
        }

        if (goodRight || goodLeft)
        {
            SoundManager.instance.CallPlaySound(sounds.TileConnect);
        }
    }

    private void SetConnectDataOnPlace(bool isGood, bool isLeft, SubTileData mySubtile, SubTileData contestedSubTile, Slice mySlice)
    {
        heldTile.SetSubtilesConnectedGFX(isGood, mySubtile, contestedSubTile);

        if (isLeft)
        {
            goodConnectLeft = isGood;

            leftCell.goodConnectRight = isGood;

            if (!leftCell.goodConnectRight)
            {
                leftCell.amountUnsuccessfullConnections++;
            }

        }
        else
        {
            goodConnectRight = isGood;

            rightCell.goodConnectLeft = isGood;

            if (!rightCell.goodConnectLeft)
            {
                rightCell.amountUnsuccessfullConnections++;
            }
        }


        SetConnectionVFX(isGood, isLeft);

        if (isGood)
        {
            mySlice.sliceData.onGoodConnectionActions?.Invoke();
        }
    }

    private void SetConnectionVFX(bool isGood, bool isLeft)
    {
        if(isLeft)
        {
            if (isGood)
            {
                connectVFXLeft.SetActivatedChild(); // set as good connection
            }
            else
            {
                connectVFXLeft.SetDeActivatedChild(); // set as bad connection
            }

        }
        else
        {
            if (isGood)
            {
                connectVFXRight.SetActivatedChild(); // set as good connection
            }
            else
            {
                connectVFXRight.SetDeActivatedChild(); // set as bad connection
            }

        }
    }

    private void SetConnectDataOnRemove(bool isGood, bool isLeft, SubTileData mySubtile, SubTileData contestedSubTile, Slice mySlice)
    {
        if (isLeft)
        {
            goodConnectLeft = isGood;

            if(!leftCell.goodConnectRight)
            {
                leftCell.amountUnsuccessfullConnections--;
            }

            leftCell.goodConnectRight = isGood;
        }
        else
        {
            goodConnectRight = isGood;

            if (!rightCell.goodConnectLeft)
            {
                rightCell.amountUnsuccessfullConnections--;
            }

            rightCell.goodConnectLeft = isGood;
        }
    }

    public override void AcceptTileToHolder(TileParentLogic recievedTile)
    {
        recievedTile.transform.SetParent(tileGFXParent);

        // all of this should happen in an animation manager?? or something that will manage animations


        float distanceFromTarget = Vector3.Distance(recievedTile.transform.localPosition, Vector3.zero);

        if(distanceFromTarget > maxDistanceToAnimate)
        {
            recievedTile.transform.localPosition = Vector3.zero;
            recievedTile.transform.localRotation = Quaternion.Euler(Vector3.zero);
            recievedTile.transform.localScale = GameManager.GENERAL_TILE_SIZE;
        }
        else
        {
            float timeToAnimate = distanceFromTarget / maxAnimateSpeed;
            LeanTween.moveLocal(recievedTile.gameObject, Vector3.zero, timeToAnimate);
            LeanTween.rotateLocal(recievedTile.gameObject, Vector3.zero, timeToAnimate);
            LeanTween.scale(recievedTile.gameObject, GameManager.GENERAL_TILE_SIZE, timeToAnimate);
        }

        heldTile = recievedTile;
    }

    public int GetUnsuccessfullConnections()
    {
        return amountUnsuccessfullConnections;
    }

    public abstract bool DroppedOn(TileParentLogic tileToPlace, Ring currentRing);
    public bool DroopedOnDispatch(TileParentLogic tileToPlace, Ring currentRing)
    {
        if (!heldTile)
        {
            RecieveTileDisplayer(tileToPlace);

            tileToPlace.SetPlaceTileData(true, this);

            currentRing.CallOnAddTileActions();

            SymbolAndColorCollector.instance.AddColorsAndSymbolsToLists(tileToPlace);

            return true;
        }

        return false;
    }

    public void GrabTileFrom()
    {
        ResetLockData();

        OnRemoveTileDisplay();

        amountUnsuccessfullConnections = 0;

        if (leftCell.heldTile)
        {
            SetConnectDataOnRemove(false, true, heldTile.subTileLeft, leftCell.heldTile.subTileRight, leftSlice);
        }

        if (rightCell.heldTile)
        {
            SetConnectDataOnRemove(false, false, heldTile.subTileRight, rightCell.heldTile.subTileLeft, rightSlice);
        }

        SymbolAndColorCollector.instance.RemoveColorsAndSymbolsToLists(heldTile);

        heldTile = null;

        GameManager.gameRing.CallOnRemoveTileFromRing();

    }

    public void SetAsLocked(bool locked)
    {
        isLocked = locked;
    }
    public void SetAsStone(bool _isStone)
    {
        isLocked = _isStone;
        isStone = _isStone;
    }

    public void ResetLockData()
    {
        // this is called only when we grab a tile from this cell

        if (isLocked)
        {
            //current cell data
            if (leftSlice.isLock)
            {
                leftSlice.DoLockAnim(false);
            }

            if (rightSlice.isLock)
            {
                rightSlice.DoLockAnim(false);
            }

            isLocked = false; // the one i'm in currently is not locked anymore - this will be true if when we place this tile again it's connected well with a limiter
            leftCell.SetAsLocked(leftCell.CheckIsLockedLeft()); // we check if the right and left cells should stay locked
            rightCell.SetAsLocked(rightCell.CheckIsLockedRight()); // we check if the right and left cells should stay locked

        }
    }

    private bool CheckIsLockedLeft()
    {
        if (!heldTile) return false;

        if(isStone)
        {
            return true;
        }

        if (!leftCell.heldTile) // checks to see if theres a slice that might lock from the left
        {
            return false;
        }

        if(leftSlice.isLock)
        {
            return leftSlice.sliceData.CheckCondition(heldTile.subTileLeft, leftCell.heldTile.subTileRight); // check to see if the slice from the left locks
        }
        else
        {
            return false;
        }

    }

    private bool CheckIsLockedRight()
    {
        if (!heldTile) return false;

        if (isStone)
        {
            return true;
        }

        if(!rightCell.heldTile)// checks to see if theres a slice that might lock from the right
        {
            return false;
        }

        if(rightSlice.isLock)
        {
            return rightSlice.sliceData.CheckCondition(heldTile.subTileRight, rightCell.heldTile.subTileLeft); // check to see if the slice from the right locks
        }
        else
        {
            return false;
        }
    }


    public void ResetToDefault()
    {
        TileParentLogic heldTemp = heldTile;

        UndoSystem.instance.RemoveSpecificEntryTile(heldTile);

        GrabTileFrom();

        SetAsLocked(false);
        SetAsStone(false);
        goodConnectLeft = false;
        goodConnectRight = false;

        amountUnsuccessfullConnections = 0;

        Destroy(heldTemp.gameObject);
    }
}
