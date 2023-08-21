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

    public OwnedPowersAndAmounts(PowerupType _InPower, int _InAmount, int in_Price)
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
    [SerializeField] private Transform[] potionPositions;
    [SerializeField] private GameObject potionDisplayPrefab;







    [Header("Gameplay Usge")]
    [SerializeField] private PowerupType currentPowerUsing;
    [SerializeField] private IPowerUsable currentPowerLogic = null;
    [SerializeField] private PowerupScriptableObject currentChosenPowerSO = null;
    [SerializeField] private OwnedPowersAndAmounts currentPowerData = null;
    [SerializeField] private PotionInLevelHelper currentPotionDisplay = null;
    [SerializeField] private Transform localObjectToUsePowerOn = null;
    [SerializeField] private List<PotionInLevelHelper> spawnedHelpers;
    [SerializeField] private float usePotionTime;






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
    [SerializeField] private UIElementDisplayerSegment buyPotionScreenMaterialPrefab; 
    [SerializeField] private Transform buyPotionScreenMaterialParent; 

    [Header("General refrences")]
    [SerializeField] private Player player; 

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
        if (player.GetOwnedCoins < currentNeededRubies)
        {
            Debug.LogError("Not enough rubies!");
            return;
        }

        player.RemoveCoins(currentNeededRubies);

        RemoveNeededIngredientsFromPlayerBuyAction();

        RefreshIngredientDisplays();


        OwnedPowersAndAmounts temoVar = ownedPowerups.Where(i => i.powerType == currentPotionSelected.powerType).SingleOrDefault();

        if(temoVar == null)
        {
            OwnedPowersAndAmounts newPotion = new OwnedPowersAndAmounts(currentPotionSelected.powerType, 1, currentPotionSelected.price);
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
        OwnedPowersAndAmounts tempVar = ownedPowerups.Where(i => i.powerType == powerType).SingleOrDefault();
        PowerupScriptableObject temoVar2 = allPowerups.Where(i => i.powerType == powerType).SingleOrDefault();

        if(temoVar2 == null)
        {
            Debug.LogError("Problem here!");
            return;
        }

        if (tempVar == null )
        {
            OwnedPowersAndAmounts newPotion = new OwnedPowersAndAmounts(powerType, 1, temoVar2.price);
            ownedPowerups.Add(newPotion);
        }
        else
        {
            tempVar.amount++;
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

        //StartCoroutine(ChoosePowerToUse(false));


        //Check if can use the power
        if(CheckCanUsePotion())
        {
            StartCoroutine(ChoosePowerToUse(currentPowerData.amount == 0));
        }
        else
        {
            //Shake all parties involved
            ErrorPotionUse();

            // Show can't use power screen
            ResetPowerUpData(); //can't use power
        }
    }

    private void ErrorPotionUse()
    {
        ShakeInvolved();
    }

    private void ShakeInvolved()
    {
        //tile shake
        if (localObjectToUsePowerOn == null) return;
        CameraShake shake;

        localObjectToUsePowerOn.TryGetComponent<CameraShake>(out shake);
        if (shake == null) return;

        shake.ShakeOnce();

        if (currentPotionDisplay == null) return;
        currentPotionDisplay.ShakeNow();
        //powerup ball shake
    }

    public void ResetPowerUpData()
    {
        currentPowerUsing = PowerupType.None;
        currentPowerLogic = null;
        currentPowerData = null;
        currentPotionDisplay = null;
        localObjectToUsePowerOn = null;
        currentChosenPowerSO = null;

        USING_POWER = false;
    }


    private void RenewClipManager()
    {
        GameManager.gameClip.RenewClip();
        StartCoroutine(PowerSucceededUsing());
    }

    public void SpawnPotions()
    {
        for (int i = 0; i < ownedPowerups.Count; i++)
        {

            int tempIndex = i; // we do this since action subsccribing remembers the value in a memory unity.
            // meaning in this case it would have remembered the last value of the iterator (i)




            if (i > potionPositions.Length - 1)
            {
                Debug.Log("have more powerups than po sitions");
                return;
            }

            PowerupScriptableObject chosenPower = allPowerups.Where(k => k.powerType == ownedPowerups[i].powerType).SingleOrDefault();
            if (!chosenPower)
            {
                Debug.LogError("No power SO!");
                return;
            }


            GameObject go = Instantiate(potionDisplayPrefab, potionPositions[i]);
            PotionInLevelHelper potionData = null;

            go.TryGetComponent<PotionInLevelHelper>(out potionData);

            if(potionData)
            {
                potionData.buyButton.buttonEvents += () => StartCoroutine(CheckUseCoinsToUsePower(chosenPower, ownedPowerups[tempIndex], potionData));
                //potionData.buyButton.buttonEvents += () => CheckUseCoinsToUsePower(chosenPower, ownedPowerups[tempIndex], potionData);

                potionData.SetPotionDisplay(ownedPowerups[i].amount.ToString(), chosenPower.price.ToString(), chosenPower.potionMaterialMap.texture);

                spawnedHelpers.Add(potionData);
            }



            BasicCustomButton potionButton = null;

            go.TryGetComponent<BasicCustomButton>(out potionButton);


            if (potionButton)
            {
                potionButton.buttonEvents += () => StartCoroutine(SetUsingPotion(ownedPowerups[tempIndex], potionData));
            }
        }
    }

    public void DestroyPotions()
    {
        for (int i = 0; i < potionPositions.Length; i++)
        {
            if(potionPositions[i].childCount > 0)
            {
                for (int k = 0; k < potionPositions[i].childCount; k++)
                {
                    Destroy(potionPositions[i].transform.GetChild(k).gameObject);
                }
            }
        }

        spawnedHelpers.Clear();
    }

    private IEnumerator SetUsingPotion(OwnedPowersAndAmounts ownedPower, PotionInLevelHelper potionHelper)
    {
        if (currentPowerUsing != PowerupType.None)
        {
            ResetPowerUpData();

            foreach (var helper in spawnedHelpers)
            {
                helper.ToggleHoverWindow(false);
            }
            yield break;
        }

        if (ownedPower.amount == 0)
        {
            //if toggle a new window - close all others
            foreach (var helper in spawnedHelpers)
            {
                helper.ToggleHoverWindow(false);
            }

            potionHelper.ToggleHoverWindow(true);
            yield break;
        }
        else
        {
            //make sure there are no windows if trying to use a "good" potion
            foreach (var helper in spawnedHelpers)
            {
                helper.ToggleHoverWindow(false);
            }
        }

        currentChosenPowerSO = allPowerups.Where(k => k.powerType == ownedPower.powerType).SingleOrDefault();
        currentPowerData = ownedPower;
        currentPowerUsing = ownedPower.powerType;
        currentPotionDisplay = potionHelper;




        //yield return new WaitForSeconds(0.1f); //add small delay before setting the USING_POWER to true for the rest of the system to catch up.
        yield return new WaitForEndOfFrame();
        UsePower(false);
    }

    private void UsePower(bool is_Paid)
    {
        USING_POWER = true;

        if (currentPowerUsing == PowerupType.RefreshTiles)
        {
            StartCoroutine(ChoosePowerToUse(is_Paid));

            //if (!GameManager.gameClip.isFullClip())
            //{
            //    ChoosePowerToUse(is_Paid);

            //    //StartCoroutine(ChoosePowerToUse(is_Paid));
            //}
            //else
            //{
            //    ResetPowerUpData();
            //}
        }
    }

    public IEnumerator PowerSucceededUsing()
    {
        //StartCoroutine(UIManager.instance.DisplayPotionUsageWindow(currentPowerData.amount == 0));

        //if (currentPowerData.amount == 0)
        //{
        //    yield return new WaitForSeconds(0.3f); //small delay for visual catchup
        //    OnUseCoins();
        //}

        if (currentPowerData != null)
        {
            currentPowerData.amount--;

            if(currentPowerData.amount < 0)
            {
                currentPowerData.amount = 0;
            }

            currentPotionDisplay.SetTextCustom(currentPowerData.amount.ToString());
        }

        ResetPowerUpData();

        yield return null; //temp here
    }
    private IEnumerator ChoosePowerToUse(bool is_Paid)
    {
        StartCoroutine(UIManager.instance.DisplayPotionUsageWindow(currentPowerData.amount == 0));

        if (is_Paid)
        {
            yield return new WaitForSeconds(0.3f); //small delay for visual catchup
            OnUseCoins();
        }

        yield return new WaitUntil(() => !UIManager.IS_DURING_POTION_USAGE);

        switch (currentPowerUsing)
        {
            case PowerupType.Switch:
                currentPowerLogic.SwitchPower();
                break;
            case PowerupType.Bomb:
                currentPowerLogic.BombPower();
                break;
            case PowerupType.RefreshTiles:
                RenewClipManager();
                break;
            case PowerupType.Joker:
                currentPowerLogic.JokerPower();
                break;
            default:
                break;
        }
    }

    private void OnUseCoins()
    {
        //Manage Coin Display and data
        int currentCoins = player.GetOwnedCoins;
        int newCoins = player.GetOwnedCoins - currentChosenPowerSO.price;
        StartCoroutine(UIManager.instance.CounterText(currentCoins, newCoins, UIManager.instance.publicCoinText));
        player.RemoveCoins(currentChosenPowerSO.price);
    }
    private IEnumerator CheckUseCoinsToUsePower(PowerupScriptableObject currentSO, OwnedPowersAndAmounts ownedPower, PotionInLevelHelper potionHelper)
    {
        if(player.GetOwnedCoins >= currentSO.price)
        {
            //yield return new WaitForSeconds(0.1f); //add small delay before setting the USING_POWER to true for the rest of the system to catch up.
            yield return new WaitForEndOfFrame();

            currentChosenPowerSO = allPowerups.Where(k => k.powerType == ownedPower.powerType).SingleOrDefault();
            currentPowerData = ownedPower;
            currentPowerUsing = ownedPower.powerType;
            currentPotionDisplay = potionHelper;

            UsePower(true);
        }
        else
        {
            UIManager.instance.DisplayBundleScreen();
        }
    }


    private bool CheckCanUsePotion()
    {
        if (localObjectToUsePowerOn == null) return false;
        IPowerUsable powerUsable;

        localObjectToUsePowerOn.TryGetComponent<IPowerUsable>(out powerUsable);
        if (powerUsable == null) return false;

        return powerUsable.CheckCanUsePower(); // if they are NOT the same - that's why it's !

        //switch (currentPowerUsing)
        //{
        //    case PowerupType.Switch:
        //        return CheckCanUseSwitch();
        //    case PowerupType.Joker:
        //        return CheckCanUseJoker();
        //    case PowerupType.Bomb:
        //        return true;
        //    case PowerupType.RefreshTiles:
        //        return true;
        //    default:
        //        break;
        //}

        //return false;
    }

    //private bool CheckCanUseSwitch()
    //{
    //    if (localObjectToUsePowerOn == null) return false;
    //    IPowerUsable powerUsable;

    //    localObjectToUsePowerOn.TryGetComponent<IPowerUsable>(out powerUsable);
    //    if (powerUsable == null) return false;

    //    return powerUsable.CheckCanUsePower(); // if they are NOT the same - that's why it's !
    //}
    //private bool CheckCanUseJoker()
    //{
    //    if (localObjectToUsePowerOn == null) return false;
    //    TileParentLogic tile;

    //    localObjectToUsePowerOn.TryGetComponent<TileParentLogic>(out tile);
    //    if (tile == null) return false;

    //    return !tile.CheckAlreadyJoker(); //if they are NOT joker - that's why it's !
    //}
    public PowerupScriptableObject publicCurrentPowerSO => currentChosenPowerSO;
    public float publicUsePotionTime => usePotionTime;
}
