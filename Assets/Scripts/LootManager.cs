using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class LootToRecieve
{
    public Ingredients ingredient;
    public int amount;

    public LootToRecieve(Ingredients ingredient_In, int amount_In)
    {
        ingredient = ingredient_In;
        amount = amount_In;
    }
}

public class LootManager : MonoBehaviour
{
    [Header("needed refs")]
    [SerializeField] private Player player;
    [SerializeField] private UIElementDisplayerSegment lootDisplayPrefab;
    [SerializeField] private Ingredients[] allIngredients;

    [SerializeField] private Sprite rubySprite;

    [Header("give loot algo")]
    [SerializeField] private int currentRubiesToGive = 0;
    [SerializeField] private List<LootToRecieve> ingredientsToGive;

    [Header("loot animations")]
    [SerializeField] private float lootMoveSpeed;
    [SerializeField] private float delayBetweenLootDisplays;

    [Header("temp?")]
    [SerializeField] private Transform[] lootPositions;
    [SerializeField] private int currentLootPos;

    private void Start()
    {
        ingredientsToGive = new List<LootToRecieve>();
    }

    public void ManageLootReward(ClusterSO cluster)
    {
        for (int i = 0; i < cluster.clusterLootTables.Count; i++)
        {
            LootTables lootTable = cluster.clusterLootTables[i];

            switch (lootTable.mainLootType)
            {
                case MainLootType.R:
                    UnpackToRubiesChest(lootTable);
                    break;

                case MainLootType.L:
                    UnpackToMaterialsChest(lootTable);
                    break;

                default:
                    Debug.LogError("Error in chest loot here");
                    break;
            }
        }

        GiveLootToPlayer(); //go over this with Lior
    }

    private void UnpackToRubiesChest(LootTables lootTable)
    {
        int randomNum = UnityEngine.Random.Range(lootTable.minRubies, lootTable.maxRubies + 1);

        currentRubiesToGive += randomNum;
    }

    private void UnpackToMaterialsChest(LootTables lootTable)
    {
        List<Ingredients> ingredientsFromTables = new List<Ingredients>();

        for (int i = 0; i < lootTable.lootBagsAndChances.Length; i++)
        {
            int chance = UnityEngine.Random.Range(1, 101);

            if (chance > lootTable.lootBagsAndChances[i].chance)
            {
                Debug.Log("Failed to give loot");
            }
            else
            {
                ingredientsFromTables.AddRange(lootTable.lootBagsAndChances[i].lootBag.bagIngredients);

                int randomIngredient = UnityEngine.Random.Range(0, ingredientsFromTables.Count);
                int randomAmount = UnityEngine.Random.Range(1, 6);

                LootToRecieve LTR_exsists = ingredientsToGive.Where(p => p.ingredient.ingredientName == ingredientsFromTables[randomIngredient].ingredientName).SingleOrDefault();

                if (LTR_exsists == null)
                {
                    LootToRecieve LTR = new LootToRecieve(ingredientsFromTables[randomIngredient], randomAmount);
                    ingredientsToGive.Add(LTR);
                }
                else
                {
                    LTR_exsists.amount += randomAmount;
                }
            }

            ingredientsFromTables.Clear();
        }
    }

    private void GiveLootToPlayer()
    {
        if(currentRubiesToGive > 0)
        {
            player.AddRubies(currentRubiesToGive);
        }

        if(ingredientsToGive.Count > 0)
        {
            foreach (LootToRecieve loot in ingredientsToGive)
            {
                player.AddIngredient(loot);
            }
        }

        StartCoroutine(DisplayLootFromChest()); //go over this with Lior
    }

    private IEnumerator DisplayLootFromChest()
    {
        if (currentRubiesToGive > 0)
        {
            string[] texts = new string[] { currentRubiesToGive.ToString() };
            Sprite[] sprites = new Sprite[] { rubySprite };

            InstantiateLootDisplay(texts, sprites, lootPositions[currentLootPos]);

            yield return new WaitForSeconds(delayBetweenLootDisplays);
        }

        if (ingredientsToGive.Count > 0)
        {
            foreach (LootToRecieve loot in ingredientsToGive)
            {
                currentLootPos++;

                if (currentLootPos == lootPositions.Length)
                {
                    currentLootPos = 0;
                }

                string[] texts = new string[] { loot.amount.ToString() };
                Sprite[] sprites = new Sprite[] { loot.ingredient.ingredientSprite };

                InstantiateLootDisplay(texts, sprites, lootPositions[currentLootPos]);

                yield return new WaitForSeconds(delayBetweenLootDisplays);

                //reset positions so we can still spawn with no error.
            }
        }


        ingredientsToGive.Clear();
        currentRubiesToGive = 0;
        currentLootPos = 0;

        GameManager.instance.AdvanceLootChestAnimation(); //go over this with Lior
    }

    private void InstantiateLootDisplay(string[] texts, Sprite[] sprites, Transform target)
    {
        UIElementDisplayerSegment displayer = Instantiate(lootDisplayPrefab /*,GameManager.instance.summonedChest.transform*/);

        displayer.SetMyElement(texts, sprites);

        LeanTween.move(displayer.gameObject, lootPositions[currentLootPos], lootMoveSpeed).setOnComplete(() => displayer.transform.parent = target);
    }

    public void DestroyAllLootChildren()
    {
        foreach (Transform lootPos in lootPositions)
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
    public Ingredients[] GetAllIngredientSprites => allIngredients;
    
}
