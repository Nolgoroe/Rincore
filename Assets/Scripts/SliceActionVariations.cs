using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level Action", menuName = "ScriptableObjects/Create Slice Action")]
public class SliceActionVariations : ScriptableObject
{
    [SerializeField] private Sprite lockSprite;

    public void SetOnConnectEventsSlice(ConditonsData sliceConnectionData, sliceToSpawnDataStruct sliceData, CellBase sameIndexCell, CellBase leftNeighborCell, Slice slice)
    {
        if (sliceData.isLock)
        {
            sliceConnectionData.onGoodConnectionActions += () => sameIndexCell.SetAsLocked(true);
            sliceConnectionData.onGoodConnectionActions += () => leftNeighborCell.SetAsLocked(true);
            sliceConnectionData.onGoodConnectionActions += () => slice.DoLockAnim(true);
            slice.SetMidSprite(lockSprite); // this is called on system init - when we start the map, each ring uses this the "current" ring we're summoning and it's info. DO NOT TOUCH!
        }
    }
}
