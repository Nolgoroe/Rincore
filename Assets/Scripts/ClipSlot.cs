using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClipSlot : TileHolder, IGrabTileFrom
{
    public override void AcceptTileToHolder(TileParentLogic recievedTile)
    {
        recievedTile.transform.SetParent(tileGFXParent);

        recievedTile.transform.localPosition = Vector3.zero;
        recievedTile.transform.localRotation = Quaternion.identity;
        recievedTile.transform.localScale = Vector3.one;

        heldTile = recievedTile;
    }

    public void GrabTileFrom()
    {
    }

    public override void RemoveTile()
    {
        heldTile = null;
        GameManager.gameClip.RePopulateSpecificSlot(this);
    }

    public override void OnRemoveTileDisplay()
    {

    }

    public override void RecieveTileDisplayer(TileParentLogic recievedTile)
    {
        AcceptTileToHolder(recievedTile);
    }
}
