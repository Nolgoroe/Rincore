using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SubTileData
{
    public bool isRight;
    public SubTileSymbol subTileSymbol;
    public SubTileColor subTileColor;
    public MeshRenderer subtileMesh;
}
public abstract class TileParentLogic : MonoBehaviour, IPowerUsable
{
    public SubTileData subTileLeft, subTileRight;
    public CellBase cellParent;
    public bool partOfBoard;
    public Tiletype tileType;

    public abstract void SetPlaceTileData(bool place, CellBase cellParent);
    public abstract void SetSubTileSpawnData(SubTileData subTile, SubTileSymbol resultSymbol, SubTileColor resultColor);

    public abstract void SetTileSpawnDisplayByTextures(SubTileData subTile, Texture colorSymbolTexture/*, Texture connectionTexture*/);
    public virtual void SetSubtilesConnectedGFX(bool isGoodConnect, SubTileData ownSubTile, SubTileData contestedSubTile)
    {
        //Material matToChangeOwn = ownSubTile.subtileMesh.material;
        //Material matToChangeContested = contestedSubTile.subtileMesh.material;

        //matToChangeOwn.SetInt("Is_Piece_Match", isGoodConnect? 1 : 0);
        //matToChangeContested.SetInt("Is_Piece_Match", isGoodConnect ? 1 : 0);
    }

    public void SwitchPower()
    {

        if(CheckBothSidesSame())
        {
            PowerupManager.instance.ResetPowerUpData(); // release power directly - no success
            return;
        }

        if (cellParent)
        {
            cellParent.GrabTileFrom();
        }

        SubTileSymbol newSymbol;
        SubTileColor newColor;

        newSymbol = subTileLeft.subTileSymbol;
        newColor = subTileLeft.subTileColor;

        subTileLeft.subTileSymbol = subTileRight.subTileSymbol;
        subTileLeft.subTileColor = subTileRight.subTileColor;

        subTileRight.subTileSymbol = newSymbol;
        subTileRight.subTileColor = newColor;

        Texture[] tempArray = GameManager.gameClip.tileCreatorPreset.ReturnTexturesByData(subTileLeft, tileType);
        SetTileSpawnDisplayByTextures(subTileLeft, tempArray[0]);

        tempArray = GameManager.gameClip.tileCreatorPreset.ReturnTexturesByData(subTileRight, tileType);
        SetTileSpawnDisplayByTextures(subTileRight, tempArray[0]); 

        if (cellParent)
        {
            cellParent.DroppedOn(this, GameManager.gameRing);
        }

        StartCoroutine(PowerupManager.instance.PowerSucceededUsing());
        //PowerupManager.instance.PowerSucceededUsing();
    }

    public void BombPower()
    {
        if(cellParent)
        {
            cellParent.ResetToDefault();
            StartCoroutine(PowerupManager.instance.PowerSucceededUsing());
            //PowerupManager.instance.PowerSucceededUsing();
        }
        else
        {
            PowerupManager.instance.ResetPowerUpData(); // release power directly - no success
        }
    }


    public void JokerPower()
    {

        if(CheckAlreadyJoker())
        {
            PowerupManager.instance.ResetPowerUpData(); // release power directly - no success
            return;
        }

        if (cellParent)
        {
            cellParent.GrabTileFrom();
        }

        Texture texLeft = GameManager.gameClip.tileCreatorPreset.returnSpecificTex(SubTileColor.Joker, SubTileSymbol.Joker, false);
        Texture texRight = GameManager.gameClip.tileCreatorPreset.returnSpecificTex(SubTileColor.Joker, SubTileSymbol.Joker, true);

        Material mat = null;
        mat = GameManager.gameClip.tileCreatorPreset.getjokerMat;

        subTileLeft.subtileMesh.material = mat;
        subTileRight.subtileMesh.material = mat;


        SetTileSpawnDisplayByTextures(subTileLeft, texLeft);
        SetTileSpawnDisplayByTextures(subTileRight, texRight);

        subTileLeft.subTileColor = SubTileColor.Joker;
        subTileLeft.subTileSymbol = SubTileSymbol.Joker;

        subTileRight.subTileColor = SubTileColor.Joker;
        subTileRight.subTileSymbol = SubTileSymbol.Joker;

        if (cellParent)
        {
            cellParent.DroppedOn(this, GameManager.gameRing);
        }


        StartCoroutine(PowerupManager.instance.PowerSucceededUsing());
        //PowerupManager.instance.PowerSucceededUsing();
    }

    /// set subtile display function (maybe materials)
    

    private bool CheckBothSidesSame()
    {
        return subTileLeft.subTileColor == subTileRight.subTileColor &&
            subTileLeft.subTileSymbol == subTileRight.subTileSymbol;
    }
    private bool CheckAlreadyJoker()
    {
        return subTileLeft.subTileColor == SubTileColor.Joker &&
            subTileLeft.subTileSymbol == SubTileSymbol.Joker;
    }
}
