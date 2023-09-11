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


    public void CallPlayVFX(VFX vfxType)
    {
        StartCoroutine(PlayVFX(vfxType));
    }
    public IEnumerator PlayVFX(VFX vfxType)
    {
        yield return new WaitForSeconds(PowerupManager.instance.delayPotionEffectOnObject);

        if (vfxHelper)
        {
            vfxHelper.PlayVFX(vfxType);
        }
    }
}
