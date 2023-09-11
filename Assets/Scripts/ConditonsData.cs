using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConditonsData // this is the main class - parent of all other checks.
{
    public System.Action onGoodConnectionActions;

    public virtual bool CheckCondition(SubTileData subTileCurrent, SubTileData subTileContested)
    {
        // if there is no override spawned for the slice - this is what will be called.
        // empty slices will also have this basic check by default
        Debug.Log("Coulden't find override for conditions - Doing basic");

        ConditonsData sliceData = new ColorAndShapeCondition();

        return sliceData.CheckCondition(subTileCurrent, subTileContested);
    }
}

[System.Serializable]
public class ColorAndShapeCondition : ConditonsData
{

    public override bool CheckCondition(SubTileData subTileCurrent, SubTileData subTileContested)
    {

        if(subTileCurrent.subTileColor == SubTileColor.Joker || subTileContested.subTileColor == SubTileColor.Joker)
        {
            return true;
        }

        //if we are stone color - check symbol.
        if (subTileCurrent.subTileColor == SubTileColor.Stone || subTileContested.subTileColor == SubTileColor.Stone)
        {
            if (subTileCurrent.subTileSymbol == subTileContested.subTileSymbol)
            {
                return true;
            }


            return false;
        }

        // if we are "normal" tile color
        if (subTileCurrent.subTileColor == subTileContested.subTileColor)
        {
            return true;
        }

        //if we are "normal" tile symbol
        if(subTileCurrent.subTileSymbol == subTileContested.subTileSymbol)
        {
            return true;
        }

        return false;
    }
}

[System.Serializable]
public class GeneralColorCondition : ConditonsData
{
    public override bool CheckCondition(SubTileData subTileCurrent, SubTileData subTileContested)
    {
        if (subTileCurrent.subTileColor == SubTileColor.Joker || subTileContested.subTileColor == SubTileColor.Joker)
        {
            return true;
        }


        if ((subTileCurrent.subTileColor == subTileContested.subTileColor && subTileCurrent.subTileColor != SubTileColor.Stone && subTileContested.subTileColor != SubTileColor.Stone) 
            
            ||

            (subTileCurrent.subTileColor == SubTileColor.Joker || subTileContested.subTileColor == SubTileColor.Joker))
        {
            return true;
        }

        return false;
    }
}

[System.Serializable]
public class GeneralSymbolCondition : ConditonsData
{
    public override bool CheckCondition(SubTileData subTileCurrent, SubTileData subTileContested)
    {
        if (subTileCurrent.subTileSymbol == subTileContested.subTileSymbol || (subTileCurrent.subTileSymbol == SubTileSymbol.Joker || subTileContested.subTileSymbol == SubTileSymbol.Joker))
        {
            return true;
        }

        return false;
    }
}

[System.Serializable]
public class SpecificColorCondition : ConditonsData
{
    public SubTileColor requiredColor;

    public override bool CheckCondition(SubTileData subTileCurrent, SubTileData subTileContested)
    {
        if ((subTileCurrent.subTileColor == requiredColor || subTileCurrent.subTileColor == SubTileColor.Joker) && (subTileContested.subTileColor == requiredColor || subTileContested.subTileColor == SubTileColor.Joker))
        {
            return true;
        }

        return false;
    }
}

[System.Serializable]
public class SpecificSymbolCondition : ConditonsData
{
    public SubTileSymbol requiredSymbol;

    public override bool CheckCondition(SubTileData subTileCurrent, SubTileData subTileContested)
    {
        if ((subTileCurrent.subTileSymbol == requiredSymbol || subTileCurrent.subTileSymbol == SubTileSymbol.Joker) && (subTileContested.subTileSymbol == requiredSymbol || subTileContested.subTileSymbol == SubTileSymbol.Joker))
        {
            return true;
        }

        return false;
    }
}

