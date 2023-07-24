using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[System.Serializable]
public class OwnedPowersAndAmounts
{
    public PowerupType powerType;
    public int amount;

    public OwnedPowersAndAmounts(PowerupType _InPower, int _InAmount)
    {
        powerType = _InPower;
        amount = _InAmount;
    }
}
public class PowerupManager : MonoBehaviour
{
    public static bool USING_POWER;


    public static PowerupManager instance;

    [Header("Owned")]
    [SerializeField] private List<PowerupScriptableObject> allPowerups;

    [Header("General")]
    [SerializeField] private List<OwnedPowersAndAmounts> ownedPowerups;
    public List<PowerupType> unlockedPowerups;







    [Header("Gameplay Usge")]
    [SerializeField] private PowerupType currentPowerUsing;
    [SerializeField] private Transform localObjectToUsePowerOn = null;
    [SerializeField] private IPowerUsable currentPowerLogic = null;
    [SerializeField] private TileParentLogic tile = null;
    [SerializeField] private Slice slice = null;








    [Header("Live Crafting")]
    [SerializeField] private RectTransform selectedPotionRect;
    [SerializeField] private PowerupScriptableObject currentPotionSelected;
    [SerializeField] private int currentNeededRubies;
    [SerializeField] private List<PotionIngredientSegment> spawnedDisplays;
    [SerializeField] public List<PotionCustomButton> customPotionButtons { get; private set; }

    [Header("Crafting refrences")]
    [SerializeField] private BasicCustomUIWindow potionWindow;
    [SerializeField] private PotionCustomButton potionButtonPrefab;
    [SerializeField] private PotionIngredientSegment potionMaterialPrefab;
    [SerializeField] private Transform[] potionsMaterialZones;
    [SerializeField] private Transform potionButtonsParent;
    [SerializeField] private UIElementDisplayerSegment buyPotionScreenMaterialPrefab; // go over with lior
    [SerializeField] private Transform buyPotionScreenMaterialParent; // go over with lior

    [Header("General refrences")]
    [SerializeField] private Player player; //go over with Lior, do we need the whole player?

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        customPotionButtons = new List<PotionCustomButton>();
    }
    public void InstantiatePowerButton(PowerupType powerType) // is it okay to take care of display and data of buttons here?
    {
        //summon and set button data
        PotionCustomButton createdButton = Instantiate(potionButtonPrefab, potionButtonsParent);
        createdButton.connecetdScriptableObjectType = powerType;

        customPotionButtons.Add(createdButton);
        //set button display and logic when it's summoned
        PowerupScriptableObject tempVar = allPowerups.Where(i => i.powerType == powerType).SingleOrDefault();
        if (tempVar == null)
        {
            Debug.LogError("Couldn't find potion!");
        }

        Sprite[] sprites = new Sprite[] { tempVar.potionSprite };
        System.Action[] actions = new System.Action[1];
        actions[0] += () => SetSelectedPotion(powerType);

        createdButton.OverrideSetMyElement(null, sprites, actions);
    }

    public void SetSelectedPotion(PowerupType powerType) // is it okay to take care of display and summon of dispay here? is index of also ok?
    {
        PowerupScriptableObject tempSelected = allPowerups.Where(i => i.powerType == powerType).SingleOrDefault();
        int indexOfNewPotion = allPowerups.IndexOf(tempSelected);


        AnimatePotionButton(customPotionButtons[indexOfNewPotion], true);

        if (currentPotionSelected == tempSelected)
        {
            return;
        }

        if(currentPotionSelected)
        {
            int precviousindexOfPotion = allPowerups.IndexOf(currentPotionSelected);
            AnimatePotionButton(customPotionButtons[precviousindexOfPotion], false);
        }


        currentPotionSelected = tempSelected;
        if (currentPotionSelected == null)
        {
            Debug.LogError("Couldn't find potion!");
            return;
        }

        string[] texsts = new string[] { currentPotionSelected.powerType.ToString(), currentPotionSelected.potionDescription };
        Sprite[] sprites = new Sprite[] { currentPotionSelected.potionSprite};
        potionWindow.OverrideSetMyElement(texsts, sprites, null);

        StartCoroutine(InstantiateNeededIngredients());
    }

    private void AnimatePotionButton(PotionCustomButton customButton, bool isUp)
    {
        RectTransform rect = customButton.GetComponent<RectTransform>();
        
        if (isUp)
        {
            LeanTween.moveY(rect, customButton.originalPos.y + 50, 0.2f);
        }
        else
        {
            LeanTween.moveY(rect, customButton.originalPos.y, 0.2f);
        }
    }
    private IEnumerator InstantiateNeededIngredients() // go over with lior
    {
        StartCoroutine(ClearGeneralData());

        yield return new WaitForEndOfFrame();

        SpawnIngredients();
    }

    private void SpawnIngredients()
    {
        int summonIndex = 0;
        foreach (IngredientsNeeded ingredientNeeded in currentPotionSelected.ingredientsNeeded)
        {
            PotionIngredientSegment displayer = Instantiate(potionMaterialPrefab, potionsMaterialZones[summonIndex]);
            spawnedDisplays.Add(displayer);

            int ownedAmountOfIngredient = 0;
            int neededAmount = ingredientNeeded.amountNeeded;

            if (player.returnownedIngredients.ContainsKey(ingredientNeeded.ingredient))
            {
                ownedAmountOfIngredient = player.returnownedIngredients[ingredientNeeded.ingredient].amount;
            }

            string amountRepresentation = ownedAmountOfIngredient.ToString() + "/" + neededAmount.ToString();

            string[] texsts = new string[] { amountRepresentation };
            Sprite[] sprites = new Sprite[] { ingredientNeeded.ingredient.ingredientSprite };
            displayer.OverrideSetMyElement(texsts, sprites);

            displayer.SetColorMissingIngredients(ownedAmountOfIngredient < neededAmount);

            if (ownedAmountOfIngredient < neededAmount) // can collapse this to funciton?
            {
                int needed = neededAmount - ownedAmountOfIngredient;

                int priceOfIngredient = ingredientNeeded.ingredient.amountToPrice.priceToPay;

                currentNeededRubies += needed * priceOfIngredient;
            }

            summonIndex++;
        }
    }

    public void CallClearPowerupScreenDataCoroutine() // go over with Lior
    {
        //called from DarkBackground under workshop
        ClearPowerupScreenDataComplete();
    }
    public void ClearPowerupScreenDataComplete()
    {
        for (int i = 0; i < potionButtonsParent.childCount; i++)
        {
            Destroy(potionButtonsParent.GetChild(i).gameObject);         
        }

        StartCoroutine(ClearGeneralData());
        currentPotionSelected = null;
        customPotionButtons.Clear();
    }

    private IEnumerator ClearGeneralData()
    {
        for (int i = 0; i < potionsMaterialZones.Length; i++) // go over with Lior
        {
            for (int k = 0; k < potionsMaterialZones[i].childCount; k++)
            {
                Destroy(potionsMaterialZones[i].GetChild(k).gameObject);
            }
        }

        currentNeededRubies = 0;
        spawnedDisplays.Clear();
        yield return new WaitForEndOfFrame();
    }

    public void TryBrewPotion()
    {
        if(currentNeededRubies == 0)
        {
            BrewPotion();
        }
        else
        {
            CleanBuyPotionWindow();
            UIManager.instance.DisplayBuyPotionWindow(currentNeededRubies);
            PopulateBuyPotionWindow();
            //ask if want buy potion
        }
    }

    private void PopulateBuyPotionWindow()
    {
        foreach (IngredientsNeeded ingredientNeeded in currentPotionSelected.ingredientsNeeded) // this action replicates several times in neveral other places in this script - how to minimize?
        {
            int ownedAmountOfIngredient = 0;
            int neededAmount = ingredientNeeded.amountNeeded;

            if (player.returnownedIngredients.ContainsKey(ingredientNeeded.ingredient))
            {
                ownedAmountOfIngredient = player.returnownedIngredients[ingredientNeeded.ingredient].amount;
            }

            //spawn the display - is it ok in powerup manager?
            if(neededAmount > ownedAmountOfIngredient)
            {
                int deltaAmount = neededAmount - ownedAmountOfIngredient;

                UIElementDisplayerSegment displayer = Instantiate(buyPotionScreenMaterialPrefab, buyPotionScreenMaterialParent);
                Sprite[] sprites = new Sprite[] { ingredientNeeded.ingredient.ingredientSprite };
                string[] texsts = new string[] { deltaAmount.ToString()};

                displayer.OverrideSetMyElement(texsts, sprites);
            }
        }

    }

    public void CleanBuyPotionWindow()
    {
        if(buyPotionScreenMaterialParent.childCount > 0)
        {
            for (int i = 0; i < buyPotionScreenMaterialParent.childCount; i++)
            {
                Destroy(buyPotionScreenMaterialParent.GetChild(i).gameObject);
            }
        }
    }
    private void RefreshIngredientDisplays() // go over with lior
    {
        currentNeededRubies = 0;

        int summonIndex = 0;
        foreach (IngredientsNeeded ingredientNeeded in currentPotionSelected.ingredientsNeeded)
        {
            int ownedAmountOfIngredient = 0;
            int neededAmount = ingredientNeeded.amountNeeded;

            if (player.returnownedIngredients.ContainsKey(ingredientNeeded.ingredient))
            {
                ownedAmountOfIngredient = player.returnownedIngredients[ingredientNeeded.ingredient].amount;
            }

            string amountRepresentation = ownedAmountOfIngredient.ToString() + "/" + neededAmount.ToString();
            spawnedDisplays[summonIndex].SetAmountsText(amountRepresentation);

            if (ownedAmountOfIngredient < neededAmount) // can collapse this to funciton?
            {
                int needed = neededAmount - ownedAmountOfIngredient;

                int priceOfIngredient = ingredientNeeded.ingredient.amountToPrice.priceToPay;

                currentNeededRubies += needed * priceOfIngredient;
            }

            summonIndex++;
        }

    }
    private void RemoveNeededIngredientsFromPlayerBrewAction() // go over with lior
    {
        foreach (IngredientsNeeded ingredientNeeded in currentPotionSelected.ingredientsNeeded)
        {
            player.RemoveIngredients(ingredientNeeded.ingredient, ingredientNeeded.amountNeeded);
        }
    }

    private void RemoveNeededIngredientsFromPlayerBuyAction() // go over with lior
    {
        foreach (IngredientsNeeded ingredientNeeded in currentPotionSelected.ingredientsNeeded)
        {
            int ownedAmountOfIngredient = 0;
            int neededAmount = ingredientNeeded.amountNeeded;
            if (player.returnownedIngredients.ContainsKey(ingredientNeeded.ingredient))
            {
                ownedAmountOfIngredient = player.returnownedIngredients[ingredientNeeded.ingredient].amount;
            }

            if(ownedAmountOfIngredient >= neededAmount)
            {
                //remove what we need
                player.RemoveIngredients(ingredientNeeded.ingredient, ingredientNeeded.amountNeeded);
            }
            else
            {
                //remove all we have
                player.RemoveIngredients(ingredientNeeded.ingredient, ownedAmountOfIngredient);
            }
        }
    }

    public void BuyPotion()
    {
        if (player.GetOwnedRubies < currentNeededRubies)
        {
            Debug.LogError("Not enough rubies!");
            return;
        }

        player.RemoveRubies(currentNeededRubies);

        RemoveNeededIngredientsFromPlayerBuyAction();

        RefreshIngredientDisplays();


        OwnedPowersAndAmounts temoVar = ownedPowerups.Where(i => i.powerType == currentPotionSelected.powerType).SingleOrDefault();

        if(temoVar == null)
        {
            OwnedPowersAndAmounts newPotion = new OwnedPowersAndAmounts(currentPotionSelected.powerType, 1);
            ownedPowerups.Add(newPotion);
        }
        else
        {
            temoVar.amount++;
        }

        CleanBuyPotionWindow();
    }
    public void BrewPotion()
    {
        RemoveNeededIngredientsFromPlayerBrewAction();

        RefreshIngredientDisplays();

        AddPotion(currentPotionSelected.powerType);
    }

    public void AddPotion(PowerupType powerType)
    {
        OwnedPowersAndAmounts temoVar = ownedPowerups.Where(i => i.powerType == powerType).SingleOrDefault();

        if (temoVar == null)
        {
            OwnedPowersAndAmounts newPotion = new OwnedPowersAndAmounts(powerType, 1);
            ownedPowerups.Add(newPotion);
        }
        else
        {
            temoVar.amount++;
        }
        Debug.Log("Added this power: " + powerType.ToString());
    }


    //Powerup zone

    public void InitPowerUsageData(Transform objectToUsePowerOn, IPowerUsable powerLogic)
    {
        if(!objectToUsePowerOn || powerLogic == null)
        {
            Debug.LogError("Error in power up usage init");
            return;
        }

        localObjectToUsePowerOn = objectToUsePowerOn;
        currentPowerLogic = powerLogic;

        ChoosePowerToUse();
    }

    private void ChoosePowerToUse()
    {
        switch (currentPowerUsing)
        {
            case PowerupType.Switch:
                currentPowerLogic.SwitchPower();
                break;
            case PowerupType.Bomb:
                currentPowerLogic.BombPower();
                break;
            case PowerupType.RefreshTiles:
                break;
            case PowerupType.Joker:
                break;
            default:
                break;
        }


        ResetPowerUpData();
    }

    public void ResetPowerUpData()
    {
        currentPowerUsing = PowerupType.None;
        localObjectToUsePowerOn = null;
        currentPowerLogic = null;
        tile = null;
        slice = null;




        USING_POWER = false;
    }

    [ContextMenu("Power Now")]
    public void UsePower()
    {
        USING_POWER = true;
    }
}
