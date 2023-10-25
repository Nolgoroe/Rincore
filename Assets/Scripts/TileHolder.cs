using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileHolder : MonoBehaviour
{
    public TileParentLogic heldTile;
    public Transform tileGFXParent;
    public bool isLocked;
    public bool isStone;

    public VFXActivatorHelper vfxHelper;

    // think about creating an action system here aswell for "on remove" + "on recieve" - look at gamemanger as example.

    public abstract void RemoveTile();
    public abstract void OnRemoveTileDisplay();
    public abstract void RecieveTileDisplayer(TileParentLogic tileToPlace);
    public abstract void AcceptTileToHolder(TileParentLogic recievedTile);


    public void CallPlayVFX(VFX vfxType, float delayTime)
    {
        if(isActiveAndEnabled)
        {
            StartCoroutine(PlayVFX(vfxType, delayTime));
        }
    }
    public IEnumerator PlayVFX(VFX vfxType, float delayTime)
    {
           yield return new WaitForSeconds(delayTime);

        if (vfxHelper)
        {
            vfxHelper.PlayVFX(vfxType, true);
        }
    }
}
