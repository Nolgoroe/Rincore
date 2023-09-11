using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum LootType
{
    C,
    P
}


[System.Serializable]
public class ChanceToReward
{
    public int chance;
    public int amount;
}

[System.Serializable]
public class RewardEntry
{
    public LootType lootType;
    public List<ChanceToReward> chancesForAmount;
    public PowerupScriptableObject powerReward;

}

[CreateAssetMenu(fileName = "Chest", menuName = "ScriptableObjects/Create Chest")]
public class ChestSO : ScriptableObject
{
    public int ChanceToGetChest;
    public List<RewardEntry> rewardEntries;
}
