using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptedTile : TileParentLogic
{
    [SerializeField] private Texture originalTexRight;
    [SerializeField] private Texture originalTexLeft;

    [SerializeField] private Texture completedTexRight;
    [SerializeField] private Texture completedTexLeft;
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

    public override void SetTileSpawnDisplayByTextures(SubTileData subTile, Texture colorSymbolTexture, Texture connectionTexture = null)
    {
        Material matToChange = subTile.subtileMesh.material;

        matToChange.SetTexture("_BaseMap", colorSymbolTexture);

        if(subTile.isRight)
        {
            originalTexRight = colorSymbolTexture;
            completedTexRight = connectionTexture;
        }
        else
        {
            originalTexLeft = colorSymbolTexture;
            completedTexLeft = connectionTexture;
        }
    }


    public override void ToggleConnectedDisplayON(bool _On, bool isRight)
    {
        Material matLeft = subTileLeft.subtileMesh.material;
        Material matRight = subTileRight.subtileMesh.material;

        if (_On)
        {
            if(isRight)
            {
                matRight.SetTexture("_BaseMap", completedTexRight);
            }
            else
            {
                matLeft.SetTexture("_BaseMap", completedTexLeft);
            }
        }
        else
        {
            if (isRight)
            {
                matRight.SetTexture("_BaseMap", originalTexRight);
            }
            else
            {
                matLeft.SetTexture("_BaseMap", originalTexLeft);
            }
        }
    }
}
