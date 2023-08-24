using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;


public enum ChestTypes
{
    Basic,
    Premium,
    Elite,
    Legendary,
    Gold,
    Ruby,
    Emerald,
    Diamond
}

[System.Serializable]
public class powerLootData
{
    public PowerupScriptableObject powerSO;
    public PowerupType powerType;
    public int amount;

    public powerLootData(PowerupType _InPower, int _InAmount, PowerupScriptableObject _inPowerSO)
    {
        powerType = _InPower;
        amount = _InAmount;
        powerSO = _inPowerSO;
    }
}

public class LootManager : MonoBehaviour
{
    [Header("needed refs")]
    [SerializeField] private Player player;
    [SerializeField] private UIElementDisplayerSegment lootDisplayPrefab;
    //[SerializeField] private Ingredients[] allIngredients;

    [SerializeField] private Sprite coinSprite;

    [Header("give loot algo")]
    [SerializeField] private int currentCoinsToGive = 0;
    [SerializeField] private List<ChestSO> endClusterChestOptions;
    [SerializeField] private List<ChestSO> endZoneChestOptions;
    [SerializeField] private List<powerLootData> powersToGive;
    //[SerializeField] private List<LootToRecieve> ingredientsToGive;

    [Header("loot animations")]
    //[SerializeField] private float lootMoveSpeed;
    [SerializeField] private float delayBetweenLootDisplays;

    //[SerializeField] private GameObject rewardPrefab;
    [SerializeField] private Transform[] rewardsPoses;

    [Header("temp?")]
    //[SerializeField] private Transform[] lootPositions;
    [SerializeField] private int currentLootPos;
    [SerializeField] private TMP_Text chestText;

    private void Start()
    {
        powersToGive = new List<powerLootData>();
        //ingredientsToGive = new List<LootToRecieve>();
    }

    [ContextMenu("DO THIS")]
    public void PublicGiveLoot()
    {
        ManageLootReward(GameManager.instance.currentCluster);
    }


    public void ManageLootReward(ClusterSO cluster)
    {
        ChestSO chosenChest = null;

        if (cluster.isEndOfZone)
        {
            chosenChest = RollChestIndex(endZoneChestOptions);
        }
        else
        {
            chosenChest = RollChestIndex(endClusterChestOptions);
        }

        foreach (var entry in chosenChest.rewardEntries)
        {
            switch (entry.lootType)
            {
                case LootType.C:
                    UnpackToCoins(entry);
                    break;
                case LootType.P:
                    UnpackToPotions(entry);
                    break;
                default:
                    break;
            }
        }


        GiveLootToPlayer();
    }


    private ChestSO RollChestIndex(List<ChestSO> chestList)
    {
        ChestSO chosenChest = null;

        int randomNum = Random.Range(0, 101);

        if (randomNum >= chestList[0].ChanceToGetChest)
        {
            chosenChest = chestList[0];

            Debug.Log("Chosen Chest is: " + chosenChest);
            chestText.text = chosenChest.name + " " + "Chest";
            return chosenChest;
        }


        foreach (var chest in chestList)
        {
            if (randomNum <= chest.ChanceToGetChest)
            {
                chosenChest = chest;
            }
        }

        Debug.Log("Chosen Chest is: " + chosenChest);
        chestText.text = chosenChest.name + " " + "Chest";

        return chosenChest;
    }
    private void UnpackToCoins(RewardEntry entry)
    {
        int randomNum = UnityEngine.Random.Range(0, 101);

        int amount = 0;

        if (randomNum >= entry.chancesForAmount[0].chance)
        {
            amount = entry.chancesForAmount[0].amount;

            currentCoinsToGive += amount;

            return;
        }

        foreach (var entryChance in entry.chancesForAmount)
        {
            if (randomNum < entryChance.chance)
            {
                amount = entryChance.amount;
            }
        }


        currentCoinsToGive += amount;
    }

    private void UnpackToPotions(RewardEntry entry)
    {
        powerLootData owned = null;

        int randomNum = UnityEngine.Random.Range(0, 101);

        int amount = 0;

        if (randomNum >= entry.chancesForAmount[0].chance)
        {
            amount = entry.chancesForAmount[0].amount;

            if (amount == 0) return;

            owned = new powerLootData(entry.powerReward.powerType, amount, entry.powerReward);

            powersToGive.Add(owned);

            return;
        }

        foreach (var entryChance in entry.chancesForAmount)
        {
            if (randomNum < entryChance.chance)
            {
                amount = entryChance.amount;
            }
        }

        if (amount == 0) return;

        owned = new powerLootData(entry.powerReward.powerType, amount, entry.powerReward);

        powersToGive.Add(owned);
    }

    private void GiveLootToPlayer()
    {
        if(currentCoinsToGive > 0)
        {
            player.AddCoins(currentCoinsToGive);
        }

        if(powersToGive.Count > 0)
        {
            foreach (var power in powersToGive)
            {
                PowerupManager.instance.AddPotion(power.powerType, power.amount);
            }
        }

        StartCoroutine(DisplayLootFromChest()); 
    }

    private IEnumerator DisplayLootFromChest()
    {
        if (currentCoinsToGive > 0)
        {
            string[] texts = new string[] { currentCoinsToGive.ToString() };
            Sprite[] sprites = new Sprite[] { coinSprite };

            InstantiateLootDisplay(texts, sprites, rewardsPoses[currentLootPos]);

            yield return new WaitForSeconds(delayBetweenLootDisplays);
        }


        if (powersToGive.Count > 0)
        {
            foreach (var power in powersToGive)
            {
                currentLootPos++;

                string[] texts = new string[] { power.amount.ToString() };
                Sprite[] sprites = new Sprite[] { power.powerSO.potionSprite};

                InstantiateLootDisplay(texts, sprites, rewardsPoses[currentLootPos]);

                yield return new WaitForSeconds(delayBetweenLootDisplays);

                //reset positions so we can still spawn with no error.
            }
        }


        powersToGive.Clear();
        currentCoinsToGive = 0;
        currentLootPos = 0;
    }

    private void InstantiateLootDisplay(string[] texts, Sprite[] sprites, Transform target)
    {
        UIElementDisplayerSegment displayer = Instantiate(lootDisplayPrefab, target);

        displayer.SetMyElement(texts, sprites);

        //LeanTween.move(displayer.gameObject, lootPositions[currentLootPos], lootMoveSpeed).setOnComplete(() => displayer.transform.parent = target);
    }

    public void DestroyAllLootChildren()
    {
        foreach (Transform lootPos in rewardsPoses)
        {
            for (int i = 0; i < lootPos.childCount; i++)
            {
                Destroy(lootPos.GetChild(i).gameObject);
            }
        }
    }




    /**/
    // GETTERS!
    /**/
    //public Ingredients[] GetAllIngredientSprites => allIngredients;
    
}
