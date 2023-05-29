using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level Action", menuName = "ScriptableObjects/Create Slice Action")]
public class SliceActionVariations : ScriptableObject
{
    [SerializeField] private Sprite lockSprite;

    public void SetOnConnectEventsSlice(ConditonsData sliceConnectionData, sliceToSpawnDataStruct sliceData, CellBase sameIndexCell, CellBase leftNeighborCell, int spawnIndex)
    {
        if (sliceData.isLock)
        {
            sliceConnectionData.onGoodConnectionActions += () => sameIndexCell.SetAsLocked(true);
            sliceConnectionData.onGoodConnectionActions += () => leftNeighborCell.SetAsLocked(true);
            MapLogic.currentRing.ringSlices[spawnIndex].SetMidSprite(lockSprite);
        }
    }

}
