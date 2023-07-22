using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
class ColorsOnBoard
{
    public SubTileColor color;
    public int amount;
}

[System.Serializable]
class SymbolsOnBoard
{
    public SubTileSymbol symbol;
    public int amount;
}
public class SymbolAndColorCollector : MonoBehaviour
{
    public static SymbolAndColorCollector instance;

    [SerializeField] private List<ColorsOnBoard> colorsOnBoard;
    [SerializeField] private List<SymbolsOnBoard> symbolsOnBoard;

    private void Awake()
    {
        instance = this;
    }

    private void OnValidate()
    {
        colorsOnBoard = new List<ColorsOnBoard>();
        symbolsOnBoard = new List<SymbolsOnBoard>();

        for (int i = 0; i < Enum.GetValues(typeof(SubTileColor)).Length; i++)
        {
            ColorsOnBoard structColors = new ColorsOnBoard();
            structColors.color = (SubTileColor)i;
            structColors.amount = 0;
            colorsOnBoard.Add(structColors);
        }

        for (int i = 0; i < Enum.GetValues(typeof(SubTileSymbol)).Length; i++)
        {
            SymbolsOnBoard structSymbols = new SymbolsOnBoard();
            structSymbols.symbol = (SubTileSymbol)i;
            structSymbols.amount = 0;
            symbolsOnBoard.Add(structSymbols);
        }
    }

    public void AddColorsAndSymbolsToLists(TileParentLogic tile)
    {
        colorsOnBoard[(int)tile.subTileRight.subTileColor].amount++;
        colorsOnBoard[(int)tile.subTileLeft.subTileColor].amount++;

        symbolsOnBoard[(int)tile.subTileRight.subTileSymbol].amount++;
        symbolsOnBoard[(int)tile.subTileLeft.subTileSymbol].amount++;

    }

    public void RemoveColorsAndSymbolsToLists(TileParentLogic tile)
    {
        colorsOnBoard[(int)tile.subTileRight.subTileColor].amount--;
        colorsOnBoard[(int)tile.subTileLeft.subTileColor].amount--;

        symbolsOnBoard[(int)tile.subTileRight.subTileSymbol].amount--;
        symbolsOnBoard[(int)tile.subTileLeft.subTileSymbol].amount--;

    }

    public void ResetData()
    {
        // we only want to reset the amounts, not delete the lists. this saves on memory and is not very expensive.
        foreach (var item in colorsOnBoard)
        {
            item.amount = 0;
        }

        foreach (var item in symbolsOnBoard)
        {
            item.amount = 0;
        }
    }

    public void DoTotalCheck()
    {
        Ring ring = GameManager.gameRing;
        if (!ring) return;

        foreach (CellBase cell in ring.ringCells)
        {
            if(cell.heldTile)
            {
                AddColorsAndSymbolsToLists(cell.heldTile);
            }
        }
    }
}
