using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SubTileData
{
    public SubTileSymbol subTileSymbol;
    public SubTileColor subTileColor;
    public MeshRenderer subtileMesh;
}
public abstract class TileParentLogic : MonoBehaviour, IPowerUsable
{
    public SubTileData subTileLeft, subTileRight;
    public CellBase cellParent;
    public bool partOfBoard;

    public abstract void SetPlaceTileData(bool place, CellBase cellParent);
    public abstract void SetSubTileSpawnData(SubTileData subTile, SubTileSymbol resultSymbol, SubTileColor resultColor);

    public abstract void SetTileSpawnDisplayByTextures(SubTileData subTile, Texture colorSymbolTexture, Texture connectionTexture);
    public virtual void SetSubtilesConnectedGFX(bool isGoodConnect, SubTileData ownSubTile, SubTileData contestedSubTile)
    {
        Material matToChangeOwn = ownSubTile.subtileMesh.material;
        Material matToChangeContested = contestedSubTile.subtileMesh.material;

        matToChangeOwn.SetInt("Is_Piece_Match", isGoodConnect? 1 : 0);
        matToChangeContested.SetInt("Is_Piece_Match", isGoodConnect ? 1 : 0);
    }

    public void SwitchPower()
    {
        if(cellParent)
        {
            cellParent.GrabTileFrom();
        }

        Material mat = null;

        Texture tempColorSymbolTex = null;
        Texture tempConnectionTex = null;
        SubTileSymbol newSymbol;
        SubTileColor newColor;

        newSymbol = subTileLeft.subTileSymbol;
        newColor = subTileLeft.subTileColor;


        mat = subTileLeft.subtileMesh.material;
        tempColorSymbolTex = mat.GetTexture("Tile_Albedo_Map");
        tempConnectionTex = mat.GetTexture("MatchedSymbolTex");
        mat = subTileRight.subtileMesh.material;

        // we cash a pair of textures to make this code a tiny bit more readable - rethink later.
        SetTileSpawnDisplayByTextures(subTileLeft, mat.GetTexture("Tile_Albedo_Map"), mat.GetTexture("MatchedSymbolTex")); // left to right
        SetTileSpawnDisplayByTextures(subTileRight, tempColorSymbolTex, tempConnectionTex); // right to cached left

        subTileLeft.subTileSymbol = subTileRight.subTileSymbol;
        subTileLeft.subTileColor = subTileRight.subTileColor;

        subTileRight.subTileSymbol = newSymbol;
        subTileRight.subTileColor = newColor;

        if (cellParent)
        {
            cellParent.DroppedOn(this, GameManager.gameRing);
        }
    }

    public void BombPower()
    {
        if(cellParent)
        {
            cellParent.ResetToDefault();
        }
    }


    public void JokerPower()
    {
    }

    /// set subtile display function (maybe materials)
}
