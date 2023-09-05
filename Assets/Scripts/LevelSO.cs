using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
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
    public bool RandomSliceValues = true;

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
    public bool isStone;
    public SubTileSymbol rightTileSymbol;
    public SubTileSymbol leftTileSymbol;
    public SubTileColor rightTileColor;
    public SubTileColor leftTileColor;
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
    public bool isLevelColorOnly;
    //public bool allowRepeatSlices;
    public sliceToSpawnDataStruct[] slicesToSpawn;

    [Header("PowerUps")]
    public PowerupType[] powerupsForLevel;

    [Header("Stone Tiles")]
    public stoneTileDataStruct[] stoneTiles;

    [Header("Percise Position Settings")]
    public tileDataStruct[] arrayOfSpecificTilesInClip;

    [Header("Tutorial")]
    public TutorialSO levelTutorial;



    private void OnValidate()
    {
        foreach (stoneTileDataStruct data in stoneTiles)
        {
            if(!data.randomValues && data.isStone)
            {
                data.leftTileColor = SubTileColor.Stone;
                data.rightTileColor = SubTileColor.Stone;
            }
        }

        //for (int i = 0; i < levelAvailableColors.Length; i++)
        //{
        //    levelAvailableColors[i] = (SubTileColor)i;
        //}

        //for (int i = 0; i < levelAvailablesymbols.Length; i++)
        //{
        //    levelAvailablesymbols[i] = (SubTileSymbol)i;
        //}

        //for (int i = 0; i < levelAvailableColors.Length; i++)
        //{
        //    SubTileColor originCOlorColor = levelAvailableColors[i];

        //    SubTileColor tempColor = (SubTileColor)Random.Range(0, 5);

        //    for (int k = i + 1; k < levelAvailableColors.Length; k++)
        //    {
        //        SubTileColor currentColor = levelAvailableColors[k];

        //        while()

        //    }
        //}
    }
}
