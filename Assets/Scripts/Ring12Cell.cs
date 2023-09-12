using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring12Cell : CellBase
{
    public override bool DroppedOn(TileParentLogic tileToPlace, Ring ring)
    {
        if (GameManager.IS_IN_LEVEL)
        {
            SoundManager.instance.CallPlaySound(sounds.TilePlace);
        }

        UndoSystem.instance.AddNewUndoEntry(tileToPlace.transform.parent, transform, tileToPlace);

        Tile tile12Ring = GameManager.gameClip.tileCreatorPreset.CreateTile(Tiletype.Normal12, tileToPlace.subTileLeft.subTileSymbol, tileToPlace.subTileRight.subTileSymbol, tileToPlace.subTileLeft.subTileColor, tileToPlace.subTileRight.subTileColor);

        // Make sure that the new 12 ring tile spawned is located exactly where the 8 ring tile was released.
        // we do this to make sure the tile activates the drop in animation from the correct position.
        tile12Ring.transform.position = tileToPlace.transform.position;
        tile12Ring.transform.rotation = tileToPlace.transform.rotation;

        if (DroopedOnDispatch(tile12Ring, ring))
        {
            Destroy(tileToPlace.gameObject);
            return true;
        }

        Destroy(tile12Ring.gameObject);
        return false;
    }
}
