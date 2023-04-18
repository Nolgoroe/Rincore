using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum MainlootContainerType
{
    None,
    R1,
    R2,
    R3,
    R4,
    R5,
    LT1,
    LT2,
    LT3,
    LT4,
    LT5,
    LT6,
    LT7,
    LT8,
    LT9,
    LT10,
    LT11,
    LT12,
    LT13,
    LT14,
    LT15,
    LT16,
    LT17,
    LT18,
    LT19,
    LT20,
}

[System.Serializable]
public enum MainLootType
{
    R,
    L
}
[System.Serializable]
public class LootBagsChances
{
    public LootBags lootBag;
    public int chance;
}

[CreateAssetMenu(fileName = "Main Loot Container", menuName = "ScriptableObjects/Create Main Loot Container")]
public class LootTables : ScriptableObject
{
    public MainLootType mainLootType;
    public MainlootContainerType containerType;
    public LootBagsChances[] lootBagsAndChances;
    public int minRubies, maxRubies;

    private void OnValidate()
    {
        switch (containerType.ToString()[0])
        {
            case 'R':
                mainLootType = MainLootType.R;
                break;

            case 'L':
                mainLootType = MainLootType.L;
                break;

            default:
                Debug.LogError("Error in chest loot here");
                break;
        }
    }
}
