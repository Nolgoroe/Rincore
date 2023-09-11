using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum SubTileColor
{
    Blue,
    Green,
    Orange,
    Pink,
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
public class TextureHolder
{

}

[System.Serializable]
public class ColorsAndMats
{
    public SubTileColor matColor;
    public Texture[] colorTexLeft;
    public Texture[] colorTexRight;
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
    [SerializeField] private Texture jokerTexLeft, jokerTexRight;
    [SerializeField] private Material jokerMat;

    [SerializeField] private GameObject[] tilePrefabs;

    public Tile CreateTile(Tiletype tileType, SubTileSymbol[] availableSymbols, SubTileColor[] availableColors)
    {
        Tile tile = Instantiate(tilePrefabs[(int)tileType]).GetComponent<Tile>(); ;

        if(tile == null)
        {
            Debug.LogError("Error with tile generation");
            return null;
        }

        tile.tileType = tileType;


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

        if (tile == null)
        {
            Debug.LogError("Error with tile generation");
            return null;
        }

        tile.tileType = tileType;

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

    public void ReRollTile(TileParentLogic tile, Tiletype tileType, SubTileSymbol[] availableSymbols, SubTileColor[] availableColors)
    {
        //data set, then decide on textures, then display set - Left
        tile.SetSubTileSpawnData(tile.subTileLeft, RollTileSymbol(availableSymbols), RollTileColor(availableColors));
        Texture[] tempArray = ReturnTexturesByData(tile.subTileLeft, tileType);
        tile.SetTileSpawnDisplayByTextures(tile.subTileLeft, tempArray[0]);

        //data set, then decide on textures, then display set - Right
        tile.SetSubTileSpawnData(tile.subTileRight, RollTileSymbol(availableSymbols), RollTileColor(availableColors));
        tempArray = ReturnTexturesByData(tile.subTileRight, tileType);
        tile.SetTileSpawnDisplayByTextures(tile.subTileRight, tempArray[0]);
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

    public Texture[] ReturnTexturesByData(SubTileData tileData, Tiletype tileType)
    {
        SubTileSymbol tileSymbol = tileData.subTileSymbol;
        SubTileColor tileColor = tileData.subTileColor;

        Texture colorSymbolTexture = null;

        switch (tileType)
        {
            case Tiletype.Normal:
                if(tileData.isRight)
                {
                    colorSymbolTexture = colorsToMats[(int)tileColor].colorTexRight[(int)tileSymbol];
                }
                else
                {
                    colorSymbolTexture = colorsToMats[(int)tileColor].colorTexLeft[(int)tileSymbol];
                }
                break;
            case Tiletype.Normal12:
                if (tileData.isRight)
                {
                    colorSymbolTexture = colorsToMats12[(int)tileColor].colorTexRight[(int)tileSymbol];
                }
                else
                {
                    colorSymbolTexture = colorsToMats12[(int)tileColor].colorTexLeft[(int)tileSymbol];
                }
                break;
            case Tiletype.Corrupted8:
                if (tileData.isRight)
                {
                    colorSymbolTexture = colorsToMats[(int)tileColor].colorTexRight[(int)tileSymbol];
                }
                else
                {
                    colorSymbolTexture = colorsToMats[(int)tileColor].colorTexLeft[(int)tileSymbol];
                }
                break;
            case Tiletype.Corrupted12:
                if (tileData.isRight)
                {
                    colorSymbolTexture = colorsToMats12[(int)tileColor].colorTexRight[(int)tileSymbol];
                }
                else
                {
                    colorSymbolTexture = colorsToMats12[(int)tileColor].colorTexLeft[(int)tileSymbol];
                }
                break;
            case Tiletype.NoType:
                break;
            default:
                break;
        }

        return new Texture[] { colorSymbolTexture/*, connectionTex*/ };
    }

    public Texture returnSpecificTex(SubTileColor tileColor, SubTileSymbol tileSymbol, bool isRight)
    {
        switch (tileColor)
        {
            case SubTileColor.Blue:
                if(isRight)
                {
                    return colorsToMats[(int)tileColor].colorTexRight[(int)tileSymbol];
                }
                else
                {
                    return colorsToMats[(int)tileColor].colorTexLeft[(int)tileSymbol];
                }
            case SubTileColor.Green:
                if (isRight)
                {
                    return colorsToMats[(int)tileColor].colorTexRight[(int)tileSymbol];
                }
                else
                {
                    return colorsToMats[(int)tileColor].colorTexLeft[(int)tileSymbol];
                }

            case SubTileColor.Orange:
                if (isRight)
                {
                    return colorsToMats[(int)tileColor].colorTexRight[(int)tileSymbol];
                }
                else
                {
                    return colorsToMats[(int)tileColor].colorTexLeft[(int)tileSymbol];
                }

            case SubTileColor.Pink:
                if (isRight)
                {
                    return colorsToMats[(int)tileColor].colorTexRight[(int)tileSymbol];
                }
                else
                {
                    return colorsToMats[(int)tileColor].colorTexLeft[(int)tileSymbol];
                }

            case SubTileColor.Yellow:
                if (isRight)
                {
                    return colorsToMats[(int)tileColor].colorTexRight[(int)tileSymbol];
                }
                else
                {
                    return colorsToMats[(int)tileColor].colorTexLeft[(int)tileSymbol];
                }

            case SubTileColor.Stone:
                if (isRight)
                {
                    return colorsToMats[(int)tileColor].colorTexRight[(int)tileSymbol];
                }
                else
                {
                    return colorsToMats[(int)tileColor].colorTexLeft[(int)tileSymbol];

                }

            case SubTileColor.NoColor:
                if (isRight)
                {
                    return colorsToMats[(int)tileColor].colorTexRight[(int)tileSymbol];
                }
                else
                {
                    return colorsToMats[(int)tileColor].colorTexLeft[(int)tileSymbol];
                }

            case SubTileColor.Joker:
                if (isRight)
                {
                    return jokerTexRight;
                }
                else
                {
                    return jokerTexLeft;
                }

            default:
                break;
        }

        return null;
    }









    public Material getjokerMat => jokerMat;
}
