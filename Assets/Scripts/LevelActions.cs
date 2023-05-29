using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "Level Action", menuName = "ScriptableObjects/Create Level Action")]
public class LevelActions : ScriptableObject
{
    [Header("Required refrences")]
    [SerializeField] private TileCreator tileCreatorPreset;
    [SerializeField] private SliceActionVariations sliceActions;


    private List<int> tempIndexArray;

    private int currentSummonIndex;
    private List<Slice> ringSlicesList;

    private LevelSO currentLevel = null;
    private Ring currentRing = null;

    public void SetCurrentLevel()
    {
        currentLevel = MapLogic.currentLevel;
        currentRing = MapLogic.currentRing;
    }

    public void SummonStoneTiles()
    {
        SubTileColor[] availableColors = new SubTileColor[] { SubTileColor.Stone };

        foreach (stoneTileDataStruct stoneTile in currentLevel.stoneTiles)
        {
            Tile tile = null;

            if (stoneTile.randomValues)
            {
                tile = tileCreatorPreset.CreateTile(returnTileTypeStone(), currentLevel.levelAvailablesymbols, availableColors);
            }
            else
            {
                tile = tileCreatorPreset.CreateTile(returnTileTypeStone(), stoneTile.leftTileSymbol, stoneTile.rightTileSymbol, SubTileColor.Stone, SubTileColor.Stone);
            }

            if(!tile)
            {
                Debug.LogError("Problem with stone tiles");
                return;
            }

            currentRing.SpawnTileInCell(stoneTile.cellIndex, tile, true);
        }
    }

    public void SummonSlices()
    {
        tempIndexArray = new List<int>();

        currentSummonIndex = -1;

        List<sliceToSpawnDataStruct> allSlices = currentLevel.slicesToSpawn.ToList();

        ringSlicesList = new List<Slice>();
        ringSlicesList.AddRange(currentRing.ringSlices);


        // first summon is always random on ring
        currentSummonIndex = Random.Range(0, ringSlicesList.Count);
        tempIndexArray.Add(currentSummonIndex);
        ringSlicesList.Remove(currentRing.ringSlices[currentSummonIndex]);

        // this for takes care of deciding indexes for slices
        // start at index k = 1 since we already summoned first slice
        for (int k = 1; k < allSlices.Count; k++)
        {
            // Decide on position for slices
            if (!currentLevel.isRandomSlicePositions)
            {
                currentSummonIndex = allSlices[k].specificSliceIndex;
            }
            else
            {
                currentSummonIndex += ReturnSliceSummonIndex();

                if (currentSummonIndex >= currentRing.ringSlices.Length)
                {
                    currentSummonIndex -= currentRing.ringSlices.Length;
                }
            }


            tempIndexArray.Add(currentSummonIndex);

            ringSlicesList.Remove(currentRing.ringSlices[currentSummonIndex]);
        }

        for (int i = 0; i < allSlices.Count; i++)
        {
            #region Set Slice Data
            //initial data
            ConditonsData sliceConnectionData = null;
            SubTileSymbol symbol = SubTileSymbol.NoShape;
            SubTileColor color = SubTileColor.NoColor;

            switch (allSlices[i].sliceToSpawn)
            {
                case SliceConditionsEnums.GeneralColor:
                    sliceConnectionData = new GeneralColorCondition();
                    break;
                case SliceConditionsEnums.GeneralSymbol:
                    sliceConnectionData = new GeneralSymbolCondition();
                    break;
                case SliceConditionsEnums.SpecificColor:
                    sliceConnectionData = new SpecificColorCondition();

                    if(allSlices[i].RandomSliceValues)
                    {
                        int randomColorIndex = Random.Range(0, currentLevel.levelAvailableColors.Length);

                        (sliceConnectionData as SpecificColorCondition).requiredColor = currentLevel.levelAvailableColors[randomColorIndex];
                        color = currentLevel.levelAvailableColors[randomColorIndex];
                    }
                    else
                    {
                        (sliceConnectionData as SpecificColorCondition).requiredColor = currentLevel.slicesToSpawn[i].specificSlicesColor;
                        color = currentLevel.slicesToSpawn[i].specificSlicesColor;
                    }
                    break;
                case SliceConditionsEnums.SpecificSymbol:
                    sliceConnectionData = new SpecificSymbolCondition();

                    if (allSlices[i].RandomSliceValues)
                    {
                        int randomSymbolIndex = Random.Range(0, currentLevel.levelAvailablesymbols.Length);

                        (sliceConnectionData as SpecificSymbolCondition).requiredSymbol = currentLevel.levelAvailablesymbols[randomSymbolIndex];
                        symbol = currentLevel.levelAvailablesymbols[randomSymbolIndex];
                    }
                    else
                    {
                        (sliceConnectionData as SpecificSymbolCondition).requiredSymbol = currentLevel.slicesToSpawn[i].specificSlicesShape;
                        symbol = currentLevel.slicesToSpawn[i].specificSlicesShape;
                    }

                    break;
                default:
                    break;
            }
            #endregion

            #region Spawn Slice According to data
            if (!currentRing.ringSlices[tempIndexArray[i]].CheckHasSliceData())
            {
                Debug.LogError("Tried to summon on exsisting slice");
                return;
            }

            CellBase sameIndexCell = currentRing.ringCells[tempIndexArray[i]];

            CellBase leftNeighborCell = GetLeftOfCell(tempIndexArray[i]);

            if (sliceConnectionData == null)
            {
                Debug.LogError("No slice data accepted.");
                return;
            }

            sameIndexCell.leftSlice.sliceData = sliceConnectionData;
            leftNeighborCell.rightSlice.sliceData = sliceConnectionData;

            int tempInt = i;
            // we use a "temp int" here because of the way actions work - "Variable capture"
            // "Capturing" the variable i in your lambda.
            // C# captures the VARIABLE, not the VALUE at that moment.
            // when run, the lambda uses the final post-for-loop exit value of i, which wil be beyond the index range."

            //slice is the same for both "same index cell" and "left neighbor cell" - so no need to invoke event twice.
            sameIndexCell.leftSlice.sliceData.onGoodConnectionActions += () => currentLevel.slicesToSpawn[tempInt].onConnectionEvents?.Invoke();

            sliceActions.SetOnConnectEventsSlice(sliceConnectionData, allSlices[i], sameIndexCell, leftNeighborCell, tempIndexArray[i]);

            currentRing.ringSlices[tempIndexArray[i]].InitSlice(sliceConnectionData, allSlices[i].sliceToSpawn, symbol, color, allSlices[i].isLock);


            // summon slice displays under slice transforms;
            currentRing.SetSliceDisplay(currentRing.ringSlices[tempIndexArray[i]], tempIndexArray[i]);
            #endregion
        }
    }
    
    private CellBase GetLeftOfCell(int index)
    {
        index -= 1;

        if (index < 0)
        {
            index = currentRing.ringCells.Length - 1;
        }

        return currentRing.ringCells[index];
    }

    private int ReturnSliceSummonIndex()
    {
        int spacing = -1;

        if (currentLevel.slicesToSpawn.Length == 0)
        {
            Debug.LogError("Tried to summon 0 sices");
            return -1;
        }

        switch (currentLevel.ringType)
        {
            case Ringtype.ring8:
                return Ring8SlicesAlgo();
            case Ringtype.ring12:
                return Ring12SlicesAlgo();
            case Ringtype.NoType:
                break;
            default:
                break;
        }

        //if we're here it's an error
        return spacing;
    }

    private int Ring8SlicesAlgo()
    {
        int spacing = -1;

        if (currentLevel.slicesToSpawn.Length == 2)
        {
            spacing = 4;
        }
        else if (currentLevel.slicesToSpawn.Length == 3)
        {
            spacing = 3;
        }
        else if (currentLevel.slicesToSpawn.Length == 4)
        {
            spacing = 2;
        }
        else
        {
            if(tempIndexArray.Count > 3)
            {
                spacing = FindEmptyIndexSliceSlot();
            }
            else
            {
                spacing = 2;
            }
        }

        return spacing;
    }
    private int Ring12SlicesAlgo()
    {
        int spacing = -1;

        if (currentLevel.slicesToSpawn.Length == 2)
        {
            spacing = 6;
        }
        else if (currentLevel.slicesToSpawn.Length == 3)
        {
            spacing = 4;
        }
        else if (currentLevel.slicesToSpawn.Length == 4)
        {
            spacing = 3;
        }
        else if(currentLevel.slicesToSpawn.Length == 5)
        {
            if (tempIndexArray.Count > 2 && tempIndexArray.Count < 5)
            {
                spacing = 2;
            }
            else
            {
                spacing = 3;
            }
        }
        else
        {
            if (tempIndexArray.Count > 5)
            {
                spacing = FindEmptyIndexSliceSlot();
            }
            else
            {
                spacing = 2;
            }
        }
        return spacing;
    }

    private int FindEmptyIndexSliceSlot()
    {
        int randomNum = Random.Range(0, ringSlicesList.Count());
        int chosenSliceIndex = ringSlicesList[randomNum].index;

        int spacing = chosenSliceIndex - currentSummonIndex;

        return spacing;
    }

    private Tiletype returnTileTypeStone()
    {
        Tiletype type = Tiletype.Normal;

        switch (currentLevel.ringType)
        {
            case Ringtype.ring8:
                type = Tiletype.Stone8;
                break;
            case Ringtype.ring12:
                type = Tiletype.Stone12;
                break;
            case Ringtype.NoType:
                break;
            default:
                break;
        }

        return type;
    }

    public void CallLevelSetupData()
    {
        //called from level actions events

        GameManager.instance.LevelSetupData();
    }

    public void CallSpawnLevelStatue()
    {
        GameManager.instance.SpawnLevelStatue();
    }
}

