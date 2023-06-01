using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring8Cell : CellBase
{
    public override bool DroppedOn(TileParentLogic tileToPlace, Ring ring)
    {
        bool successfulDrop = DroopedOnDispatch(tileToPlace, ring);
        return successfulDrop;
    }
}
