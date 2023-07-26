using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum SubTileColor
{
    Blue,
    Green,
    Purple,
    Red,
    Yellow,
    Stone,
    NoColor,
    Joker,
}
public enum SubTileSymbol
{
    Cloud,
    Lightning,
    Moon,
    Rainbow,
    Sun,
    NoShape,
    Joker,
}
public enum Tiletype
{
    Normal,
    Normal12,
    Corrupted8,
    Corrupted12,
    NoType
}
public enum Ringtype
{
    ring8,
    ring12,
    NoType
}

[System.Serializable]
public class ColorsAndMats
{
    public SubTileColor matColor;
    public Texture[] colorTex;
}

[System.Serializable]
public class SymbolToMat
{
    public SubTileSymbol mat;
    public Texture symbolTex;
}

[CreateAssetMenu(fileName = "Tile creator preset", menuName = "ScriptableObjects/Create tile creator")]
public class TileCreator : ScriptableObject
{
    [Header("Textures and Emission Maps")]
    [SerializeField] private ColorsAndMats[] colorsToMats;
    [SerializeField] private SymbolToMat[] symbolToMat;
    [SerializeField] private ColorsAndMats[] colorsToMats12;
    [SerializeField] private SymbolToMat[] symbolToMat12;
    [SerializeField] private Texture jokerTex;

    [SerializeField] private GameObject[] tilePrefabs;

    public Tile CreateTile(Tiletype tileType, SubTileSymbol[] availableSymbols, SubTileColor[] availableColors)
    {
        Tile tile = Instantiate(tilePrefabs[(int)tileType]).GetComponent<Tile>(); ;

        if(tile == null)
        {
            Debug.LogError("Error with tile generation");
            return null;
        }

        //data set, then decide on textures, then display set - Left
        tile.SetSubTileSpawnData(tile.subTileLeft, RollTileSymbol(availableSymbols), RollTileColor(availableColors));
        Texture[] tempArray = ReturnTexturesByData(tile.subTileLeft, tileType);
        tile.SetTileSpawnDisplayByTextures(tile.subTileLeft, tempArray[0]);

        //data set, then decide on textures, then display set - Right
        tile.SetSubTileSpawnData(tile.subTileRight, RollTileSymbol(availableSymbols), RollTileColor(availableColors));
        tempArray = ReturnTexturesByData(tile.subTileRight, tileType);
        tile.SetTileSpawnDisplayByTextures(tile.subTileRight, tempArray[0]);

        return tile;
    }
    public Tile CreateTile(Tiletype tileType, SubTileSymbol symbolLeft, SubTileSymbol symbolRight, SubTileColor colorLeft, SubTileColor colorRight)
    {
        Tile tile = Instantiate(tilePrefabs[(int)tileType]).GetComponent<Tile>();

        //data set, then decide on textures, then display set - Left
        tile.SetSubTileSpawnData(tile.subTileLeft, symbolLeft, colorLeft);
        Texture[] tempArray = ReturnTexturesByData(tile.subTileLeft, tileType);
        tile.SetTileSpawnDisplayByTextures(tile.subTileLeft, tempArray[0]);

        //data set, then decide on textures, then display set - Right
        tile.SetSubTileSpawnData(tile.subTileRight, symbolRight, colorRight);
        tempArray = ReturnTexturesByData(tile.subTileRight, tileType);
        tile.SetTileSpawnDisplayByTextures(tile.subTileRight, tempArray[0]);

        return tile;
    }

    private SubTileSymbol RollTileSymbol(SubTileSymbol[] availableSymbols)
    {
        SubTileSymbol randomSymbol = SubTileSymbol.NoShape;

        if(availableSymbols != null && availableSymbols.Length > 0)
        {
            int random = Random.Range(0, availableSymbols.Length);

            randomSymbol = availableSymbols[random];
        }

        return randomSymbol;
    }
    private SubTileColor RollTileColor(SubTileColor[] availableColors)
    {
        SubTileColor randomColor = SubTileColor.NoColor;

        if (availableColors!= null && availableColors.Length > 0)
        {
            int random = Random.Range(0, availableColors.Length);

            randomColor = availableColors[random];
        }

        return randomColor;
    }

    private Texture[] ReturnTexturesByData(SubTileData tileData, Tiletype tileType)
    {
        SubTileSymbol tileSymbol = tileData.subTileSymbol;
        SubTileColor tileColor = tileData.subTileColor;

        Texture colorSymbolTexture = null;
        //Texture connectionTex = null;

        switch (tileType)
        {
            case Tiletype.Normal:
                colorSymbolTexture = colorsToMats[(int)tileColor].colorTex[(int)tileSymbol];
                //connectionTex = symbolToMat[(int)tileSymbol].symbolTex;
                break;
            case Tiletype.Normal12:
                colorSymbolTexture = colorsToMats12[(int)tileColor].colorTex[(int)tileSymbol];
                //connectionTex = symbolToMat12[(int)tileSymbol].symbolTex;
                break;
            case Tiletype.Corrupted8:
                colorSymbolTexture = colorsToMats[(int)tileColor].colorTex[(int)tileSymbol];
                //connectionTex = symbolToMat[(int)tileSymbol].symbolTex;
                break;
            case Tiletype.Corrupted12:
                colorSymbolTexture = colorsToMats12[(int)tileColor].colorTex[(int)tileSymbol];
               // connectionTex = symbolToMat12[(int)tileSymbol].symbolTex;
                break;
            case Tiletype.NoType:
                break;
            default:
                break;
        }

        return new Texture[] { colorSymbolTexture/*, connectionTex*/ };
    }

    public Texture returnSpecificTex(SubTileColor tileColor, SubTileSymbol tileSymbol)
    {
        switch (tileColor)
        {
            case SubTileColor.Blue:
                return colorsToMats[(int)tileColor].colorTex[(int)tileSymbol];
            case SubTileColor.Green:
                return colorsToMats[(int)tileColor].colorTex[(int)tileSymbol];
            case SubTileColor.Purple:
                return colorsToMats[(int)tileColor].colorTex[(int)tileSymbol];
            case SubTileColor.Red:
                return colorsToMats[(int)tileColor].colorTex[(int)tileSymbol];
            case SubTileColor.Yellow:
                return colorsToMats[(int)tileColor].colorTex[(int)tileSymbol];
            case SubTileColor.Stone:
                return colorsToMats[(int)tileColor].colorTex[(int)tileSymbol];
            case SubTileColor.NoColor:
                return colorsToMats[(int)tileColor].colorTex[(int)tileSymbol];
            case SubTileColor.Joker:
                return jokerTex;
            default:
                break;
        }

        return null;
    }
}
