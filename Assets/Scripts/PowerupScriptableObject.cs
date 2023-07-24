using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerupType
{
    Switch,
    Bomb,
    RefreshTiles,
    Joker,
    None
}

[System.Serializable]
public class IngredientsNeeded
{
    public Ingredients ingredient;
    public int amountNeeded;
}

[CreateAssetMenu(fileName = "Powerup", menuName = "ScriptableObjects/Create Powerup")]
public class PowerupScriptableObject : ScriptableObject
{
    public PowerupType powerType;
    public IngredientsNeeded[] ingredientsNeeded;
    public Sprite potionSprite;
    [TextArea(3, 7)]
    public string potionDescription;
}
