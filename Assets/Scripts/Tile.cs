using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : TileParentLogic
{
    public override void SetPlaceTileData(bool place)
    {
        partOfBoard = place;
    }

    public override void SetSubTileSpawnData(SubTileData subTile, SubTileSymbol resultSymbol, SubTileColor resultColor)
    {
        subTile.subTileSymbol = resultSymbol;
        subTile.subTileColor = resultColor;
    }

    public void SetTileSpawnDisplayByTextures(SubTileData subTile, Texture colorSymbolTexture, Texture connectionTexture)
    {
        Material matToChange = subTile.subtileMesh.material;

        matToChange.SetTexture("Tile_Albedo_Map", colorSymbolTexture);
        matToChange.SetTexture("MatchedSymbolTex", connectionTexture);
    }
}
