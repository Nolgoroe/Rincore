using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : TileParentLogic
{
    public override void SetPlaceTileData(bool place, CellBase _cellParent)
    {
        partOfBoard = place;
        cellParent = _cellParent;
    }

    public override void SetSubTileSpawnData(SubTileData subTile, SubTileSymbol resultSymbol, SubTileColor resultColor)
    {
        subTile.subTileSymbol = resultSymbol;
        subTile.subTileColor = resultColor;
    }

    public override void SetTileSpawnDisplayByTextures(SubTileData subTile, Texture colorSymbolTexture/*, Texture connectionTexture*/)
    {
        Material matToChange = subTile.subtileMesh.material;

        matToChange.SetTexture("_BaseMap", colorSymbolTexture);
    }


}
