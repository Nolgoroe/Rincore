using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerupType
{
    Switch,
    Bomb,
    RefreshTiles,
    Joker,
    Undo,
    None
}

[CreateAssetMenu(fileName = "Powerup", menuName = "ScriptableObjects/Create Powerup")]
public class PowerupScriptableObject : ScriptableObject
{
    public PowerupType powerType;
    public Sprite potionSprite;
    public Sprite potionMaterialMap;
    public int price;
    [TextArea(3, 7)]
    public string potionDescription;
}
