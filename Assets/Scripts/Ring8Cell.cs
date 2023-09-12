using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring8Cell : CellBase
{
    public override bool DroppedOn(TileParentLogic tileToPlace, Ring ring)
    {
        SoundManager.instance.CallPlaySound(sounds.TilePlace);

        UndoSystem.instance.AddNewUndoEntry(tileToPlace.transform.parent, transform, tileToPlace);

        bool successfulDrop = DroopedOnDispatch(tileToPlace, ring);
        return successfulDrop;
    }
}
