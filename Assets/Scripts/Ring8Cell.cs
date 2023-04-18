using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring8Cell : CellBase
{
    public override bool DroppedOn(TileParentLogic tileToPlace)
    {
        bool successfulDrop = DroopedOnDispatch(tileToPlace);
        return successfulDrop;
    }
}
