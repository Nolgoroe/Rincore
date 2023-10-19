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
    }

    public void BombPower()
    {
        if (cellParent)
        {
            StartCoroutine(PowerupManager.instance.PowerSucceededUsing());

            cellParent.ResetToDefault();
        }
        else
        {
            PowerupManager.instance.ResetPowerUpData(); // release power directly - no success
        }

        cellParent.CallPlayVFX(VFX.bomb);
    }

    public void JokerPower()
    {
        if (cellParent)
        {
            cellParent.GrabTileFrom();
        }

        TileParentLogic newTile = this;

        if (tileType == Tiletype.Corrupted8 || tileType == Tiletype.Corrupted12)
        {
            Tiletype type = Tiletype.NoType;

            type = GameManager.currentLevel.ringType == Ringtype.ring8 ? Tiletype.Normal : Tiletype.Normal12;

            newTile = GameManager.gameClip.tileCreatorPreset.CreateTile(type, subTileLeft.subTileSymbol, subTileRight.subTileSymbol, subTileLeft.subTileColor, subTileRight.subTileColor);

            // Make sure that the new 12 ring tile spawned is located exactly where the 8 ring tile was released.
            // we do this to make sure the tile activates the drop in animation from the correct position.
            newTile.transform.position = transform.position;
            newTile.transform.rotation = transform.rotation;
            newTile.transform.parent = transform.parent;

            Texture texLeft = GameManager.gameClip.tileCreatorPreset.returnSpecificTex(SubTileColor.Joker, SubTileSymbol.Joker, false);
            Texture texRight = GameManager.gameClip.tileCreatorPreset.returnSpecificTex(SubTileColor.Joker, SubTileSymbol.Joker, true);

            Material mat = null;
            mat = GameManager.gameClip.tileCreatorPreset.getjokerMat;

            newTile.subTileLeft.subtileMesh.material = mat;
            newTile.subTileRight.subtileMesh.material = mat;


            SetTileSpawnDisplayByTextures(newTile.subTileLeft, texLeft);
            SetTileSpawnDisplayByTextures(newTile.subTileRight, texRight);

            newTile.subTileLeft.subTileColor = SubTileColor.Joker;
            newTile.subTileLeft.subTileSymbol = SubTileSymbol.Joker;

            newTile.subTileRight.subTileColor = SubTileColor.Joker;
            newTile.subTileRight.subTileSymbol = SubTileSymbol.Joker;
        }
        else
        {
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
        }


        if (cellParent)
        {
            cellParent.DroppedOn(newTile, GameManager.gameRing);

            if(cellParent.isStone)
            {
                cellParent.SetAsStone(false);
            }
        }


        StartCoroutine(PowerupManager.instance.PowerSucceededUsing());


        if (tileType == Tiletype.Corrupted8 || tileType == Tiletype.Corrupted12)
        {
            Destroy(gameObject);
        }

        TileHolder holder = null;
        transform.parent.TryGetComponent<TileHolder>(out holder);

        if (holder)
        {
            holder.CallPlayVFX(VFX.joker);
        }
    }

    /// set subtile display function (maybe materials)

    public bool CheckCanUsePower(PowerupType type)
    {
        switch (type)
        {
            case PowerupType.Switch:
                return CheckSidesDifferent();
            case PowerupType.Bomb:
                return CheckCanBomb();
            case PowerupType.Joker:
                return CheckIsNotJoker();
            default:
                break;
        }


        return false;
    }

    private bool CheckCanBomb()
    {
        if(cellParent)
        {
            return true;
        }

        return false;
    }

    public bool CheckSidesDifferent()
    {
        return subTileLeft.subTileColor != subTileRight.subTileColor ||
            subTileLeft.subTileSymbol != subTileRight.subTileSymbol;
    }
    public bool CheckIsNotJoker()
    {
        return subTileLeft.subTileColor != SubTileColor.Joker ||
            subTileLeft.subTileSymbol != SubTileSymbol.Joker;
    }

    public void ShakeNow()
    {
        CameraShake shake;

        TryGetComponent<CameraShake>(out shake);
        if (shake == null) return;

        shake.ShakeOnce();
    }
}
