using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CellBase : TileHolder, IGrabTileFrom
{
    public CellBase leftCell, rightCell;

    public Slice leftSlice, rightSlice;

    // by default when we put a tile and there is no contested tile - it will be considered a "bad" connection
    // to prevent hightlights of "good connections".
    [SerializeField] bool goodConnectLeft, goodConnectRight;
    [SerializeField] int amountUnsuccessfullConnections;

    Collider2D cellCollider;

    //TEMP

    [SerializeField]
    int maxDistanceToAnimate;
    [SerializeField]
    int maxAnimateSpeed;

    // think about creating an action system here aswell for "on good connection" + "on bad connection" - look at gamemanger as example.

    private void Awake()
    {
        cellCollider = GetComponent<Collider2D>();
    }

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
        }

        if (rightCell.heldTile)
        {
            heldTile.SetSubtilesConnectedGFX(false, heldTile.subTileRight, rightCell.heldTile.subTileLeft);
        }
    }

    public override void RemoveTile()
    {
    }

    private void CheckConnections()
    {
        amountUnsuccessfullConnections = 0;

        bool good = false;

        if (leftCell.heldTile)
        {
            good = leftSlice.sliceData.CheckCondition(heldTile.subTileLeft, leftCell.heldTile.subTileRight);
            if (!good)
            {
                //bad connection if we're inside here.
                amountUnsuccessfullConnections++;
            }

            SetConnectDataOnPlace(good, true, heldTile.subTileLeft, leftCell.heldTile.subTileRight, leftSlice);
        }

        if (rightCell.heldTile)
        {
            good = rightSlice.sliceData.CheckCondition(heldTile.subTileRight, rightCell.heldTile.subTileLeft);
            if (!good)
            {
                //bad connection if we're inside here.
                amountUnsuccessfullConnections++;
            }

            SetConnectDataOnPlace(good, false, heldTile.subTileRight, rightCell.heldTile.subTileLeft, rightSlice);     
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

        if (isGood)
        {
            mySlice.sliceData.onGoodConnectionActions?.Invoke();
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
            recievedTile.transform.localScale = Vector3.one;
        }
        else
        {
            float timeToAnimate = distanceFromTarget / maxAnimateSpeed;
            LeanTween.moveLocal(recievedTile.gameObject, Vector3.zero, timeToAnimate);
            LeanTween.rotateLocal(recievedTile.gameObject, Vector3.zero, timeToAnimate);
            LeanTween.scale(recievedTile.gameObject, Vector3.one, timeToAnimate);
        }

        heldTile = recievedTile;
    }

    public int GetUnsuccessfullConnections()
    {
        return amountUnsuccessfullConnections;
    }

    public abstract bool DroppedOn(TileParentLogic tileToPlace);
    public bool DroopedOnDispatch(TileParentLogic tileToPlace)
    {
        if (!heldTile)
        {
            RecieveTileDisplayer(tileToPlace);

            tileToPlace.SetPlaceTileData(true);

            GameManager.gameRing.CallOnAddTileActions();

            SymbolAndColorCollector.instance.AddColorsAndSymbolsToLists(tileToPlace);

            return true;
        }

        return false;
    }

    public void GrabTileFrom()
    {
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

        SymbolAndColorCollector.instance.AddColorsAndSymbolsToLists(heldTile);

        heldTile = null;

        GameManager.gameRing.CallOnRemoveTileFromRing();

    }

    public void SetAsLocked(bool locked)
    {
        isLocked = locked;

        cellCollider.enabled = !locked;
    }

}
