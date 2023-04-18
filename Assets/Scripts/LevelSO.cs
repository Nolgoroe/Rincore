using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.ComponentModel;


[System.Serializable]
public class sliceToSpawnDataStruct
{
    public SliceConditionsEnums sliceToSpawn;
    public UnityEvent onConnectionEvents;
    public int specificSliceIndex;
    public SubTileColor specificSlicesColor;
    public SubTileSymbol specificSlicesShape;

    public bool isLock;
    public bool RandomSliceValues;

}

[System.Serializable]
public class tileDataStruct
{
    public SubTileColor rightTileColor;
    public SubTileSymbol rightTileSymbol;
    public SubTileColor leftTileColor;
    public SubTileSymbol leftTileSymbol;
}

[System.Serializable]
public class stoneTileDataStruct
{
    public int cellIndex;
    public bool randomValues;
    public SubTileSymbol rightTileSymbol;
    public SubTileSymbol leftTileSymbol;
}

[CreateAssetMenu(fileName = "Level", menuName ="ScriptableObjects/Create Level")]
public class LevelSO : ScriptableObject
{
    [Header("Level Setup Settings")]
    public WorldEnum worldName;
    public int levelNumInZone;
    public Ringtype ringType;

    [Space]
    public UnityEvent beforeRingSpawnActions; // each function that will be called here will "subscribe" to it's relevant stage in the gamemanger action
    public UnityEvent ringSpawnActions;
    public UnityEvent afterRingSpawnActions; 

    public SubTileColor[] levelAvailableColors;
    public SubTileSymbol[] levelAvailablesymbols;

    [Header("Slices")]
    public bool isRandomSlicePositions;
    //public bool allowRepeatSlices;
    public sliceToSpawnDataStruct[] slicesToSpawn;

    [Header("PowerUps")]
    public PowerupType[] powerupsForLevel;

    [Header("Stone Tiles")]
    public stoneTileDataStruct[] stoneTiles;

    [Header("Percise Position Settings")]
    public tileDataStruct[] arrayOfSpecificTilesInClip;
}
