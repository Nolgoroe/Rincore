using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum LootBagTypes
{
    LB1,
    LB2,
    LB3,
    LB4,
    LB5,
    LB6,
    LB7,
    LB8,
    LB9,
    LB10,
    LB11,
}

[CreateAssetMenu(fileName = "Loot Bag", menuName = "ScriptableObjects/Create Loot Bag")]
public class LootBags : ScriptableObject
{
    public LootBagTypes bagType;
    public Ingredients[] bagIngredients;
}
