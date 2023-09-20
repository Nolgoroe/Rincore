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

    public void SetCurrentLevel() // this is init - when we start the map, each ring uses this info to summon stone tiles and limiters. DO NOT TOUCH!
    {
        currentLevel = MapLogic.currentLevel;
        currentRing = MapLogic.currentRing;
    }

    public void SummonStoneTiles() //predetermined tiles on board
    {
        SubTileColor[] availableColors = null;

        Tiletype type;


        foreach (stoneTileDataStruct stoneTile in currentLevel.stoneTiles)
        {

            if (stoneTile.isStone)
            {
                availableColors = new SubTileColor[] { SubTileColor.Stone };

                type = returnTileTypeStone();
            }
            else
            {
                availableColors = currentLevel.levelAvailableColors;

                type = Tiletype.Normal;
            }

            Tile tile = null;

            if (stoneTile.randomValues)
            {
                tile = tileCreatorPreset.CreateTile(type, currentLevel.levelAvailablesymbols, availableColors);
            }
            else
            {
                tile = tileCreatorPreset.CreateTile(type, stoneTile.leftTileSymbol, stoneTile.rightTileSymbol, stoneTile.leftTileColor, stoneTile.rightTileColor);
            }

            if (!tile)
            {
                Debug.LogError("Problem with stone tiles");
                return;
            }
            tile.transform.localPosition = currentRing.transform.localPosition;
            currentRing.SpawnStoneTileInCell(stoneTile.cellIndex, tile, stoneTile.isStone);
        }
    }

    public void SummonSlices()
    {
        CheckSetDefaultSliceConditions();

        tempIndexArray = new List<int>();

        currentSummonIndex = -1;

        List<sliceToSpawnDataStruct> allSlices = currentLevel.slicesToSpawn.ToList();

        if (allSlices.Count == 0) return;


        ringSlicesList = new List<Slice>();
        ringSlicesList.AddRange(currentRing.ringSlices);



        if (!currentLevel.isRandomSlicePositions)
        {
            // take the first slice data
            currentSummonIndex = allSlices[0].specificSliceIndex;
            tempIndexArray.Add(currentSummonIndex);
            ringSlicesList.Remove(currentRing.ringSlices[currentSummonIndex]);
        }
        else
        {
            currentSummonIndex = Random.Range(0, ringSlicesList.Count);
            tempIndexArray.Add(currentSummonIndex);
            ringSlicesList.Remove(currentRing.ringSlices[currentSummonIndex]);
        }

        // this for taking care of deciding indexes for slices
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




        List<SubTileColor> colorList = new List<SubTileColor>();
        colorList.AddRange(currentLevel.levelAvailableColors);

        List<SubTileSymbol> symbolList = new List<SubTileSymbol>();
        symbolList.AddRange(currentLevel.levelAvailablesymbols);

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
                        int randomColorIndex = -1;

                        if (colorList.Count <= 0)
                        {
                            randomColorIndex = Random.Range(0, currentLevel.levelAvailableColors.Length);
                            (sliceConnectionData as SpecificColorCondition).requiredColor = currentLevel.levelAvailableColors[randomColorIndex];
                            color = currentLevel.levelAvailableColors[randomColorIndex];
                        }
                        else
                        {
                            randomColorIndex = Random.Range(0, colorList.Count);

                            (sliceConnectionData as SpecificColorCondition).requiredColor = colorList[randomColorIndex];
                            color = colorList[randomColorIndex];
                            colorList.RemoveAt(randomColorIndex);
                        }


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
                        int randomSymbolIndex = -1;

                        if (symbolList.Count <= 0)
                        {
                            randomSymbolIndex = Random.Range(0, currentLevel.levelAvailablesymbols.Length);
                            (sliceConnectionData as SpecificSymbolCondition).requiredSymbol = currentLevel.levelAvailablesymbols[randomSymbolIndex];
                            symbol = currentLevel.levelAvailablesymbols[randomSymbolIndex];
                        }
                        else
                        {
                            randomSymbolIndex = Random.Range(0, symbolList.Count);
                            (sliceConnectionData as SpecificSymbolCondition).requiredSymbol = symbolList[randomSymbolIndex];
                            symbol = symbolList[randomSymbolIndex];
                            symbolList.RemoveAt(randomSymbolIndex);
                        }

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

            LevelSO currentLevelTemp = currentLevel;

            sameIndexCell.leftSlice.sliceData.onGoodConnectionActions += () => currentLevelTemp.slicesToSpawn[tempInt].onConnectionEvents?.Invoke();

            sliceActions.SetOnConnectEventsSlice(sliceConnectionData, allSlices[i], sameIndexCell, leftNeighborCell, sameIndexCell.leftSlice);

            currentRing.ringSlices[tempIndexArray[i]].InitSlice(sliceConnectionData, allSlices[i].sliceToSpawn, symbol, color, sameIndexCell, leftNeighborCell, allSlices[i].isLock);


            // summon slice displays under slice transforms;
            currentRing.SetSliceDisplay(currentRing.ringSlices[tempIndexArray[i]], tempIndexArray[i]);
            #endregion
        }
    }
    
    private void CheckSetDefaultSliceConditions()
    {
        if (currentLevel.isLevelColorOnly)
        {
            // make sure the default of all is color connection
            ConditonsData sliceConnectionData = null;
            sliceConnectionData = new GeneralColorCondition();

            foreach (var slice in currentRing.ringSlices)
            {
                slice.sliceData = sliceConnectionData;
            }
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
                type = Tiletype.Corrupted8;
                break;
            case Ringtype.ring12:
                type = Tiletype.Corrupted12;
                break;
            case Ringtype.NoType:
                break;
            default:
                break;
        }

        return type;
    }
}

