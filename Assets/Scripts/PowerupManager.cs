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
    public float delayPotionEffectOnObject;

    public Color onUseSwitchColor;
    public Color onUseBombColor;
    public Color onUseJokerColor;


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
    [SerializeField] private GameObject undoButton; 

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

    public void CallClearPowerupScreenDataCoroutine()
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
        for (int i = 0; i < potionsMaterialZones.Length; i++)
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
            //ask if want buy potion
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

    public void BuyPotion()
    {
        if (player.GetOwnedCoins < currentNeededRubies)
        {
            Debug.LogError("Not enough rubies!");
            return;
        }

        player.RemoveCoins(currentNeededRubies);


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

        AddPotion(currentPotionSelected.powerType, 1);
    }

    public void AddPotion(PowerupType powerType, int amount)
    {
        OwnedPowersAndAmounts owned = ownedPowerups.Where(i => i.powerType == powerType).SingleOrDefault();
        PowerupScriptableObject powerSO = allPowerups.Where(i => i.powerType == powerType).SingleOrDefault();

        if(powerSO == null)
        {
            Debug.LogError("Problem here!");
            return;
        }

        if (owned == null )
        {
            OwnedPowersAndAmounts newPotion = new OwnedPowersAndAmounts(powerType, amount, powerSO.price);
            ownedPowerups.Add(newPotion);
        }
        else
        {
            if(owned.amount == 0)
            {
                if (currentPotionDisplay.connectedAnim)
                {
                    currentPotionDisplay.connectedAnim.SetBool("IsOFF", false);
                    currentPotionDisplay.connectedAnim.SetBool("IsON", true);
                }
            }

            owned.amount += amount;
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
        DisableAllRelaventSelectedHighlights();
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
    private void UndoAcion()
    {
        if(UndoSystem.instance.undoEntries.Count > 0)
        {
            UndoSystem.instance.CallUndoAction();
            StartCoroutine(PowerSucceededUsing());
        }
        else
        {
            ResetPowerUpData();

            CameraShake shake;
            undoButton.TryGetComponent<CameraShake>(out shake);
            if (shake == null) return;
            shake.ShakeOnce();
        }
    }

    public void SpawnPotions()
    {
        for (int i = 0; i < ownedPowerups.Count; i++)
        {

            if (ownedPowerups[i].powerType == PowerupType.Undo) continue;

            int tempIndex = i; // we do this since action subsccribing remembers the value in a memory unity.
                               // meaning in this case it would have remembered the last value of the iterator (i)


            PowerupScriptableObject chosenPower = allPowerups.Where(k => k.powerType == ownedPowerups[i].powerType).SingleOrDefault();
            if (!chosenPower)
            {
                Debug.LogError("No power SO!");
                return;
            }


            GameObject go = null;
            PotionInLevelHelper potionData = null;


            go = Instantiate(potionDisplayPrefab, potionPositions[i]);

            go.TryGetComponent<PotionInLevelHelper>(out potionData);

            if(potionData)
            {
                potionData.buyButton.buttonEvents += () => StartCoroutine(CheckUseCoinsToUsePower(chosenPower, ownedPowerups[tempIndex], potionData));

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

        ClearUndoData();

        InitUndoSystem();
    }

    public IEnumerator CheckNoPotions()
    {
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < ownedPowerups.Count; i++)
        {
            if(ownedPowerups[i].amount == 0)
            {
                if(spawnedHelpers[i].connectedAnim)
                {
                    spawnedHelpers[i].connectedAnim.SetBool("IsOFF", true);
                    spawnedHelpers[i].connectedAnim.SetBool("IsON", false);
                }
            }
        }
    }
    private void ClearUndoData()
    {
        GameObject go = null;
        PotionInLevelHelper potionData = null;
        PowerupScriptableObject chosenPower = allPowerups.Where(k => k.powerType == PowerupType.Undo).SingleOrDefault();
        OwnedPowersAndAmounts owned = ownedPowerups.Where(k => k.powerType == PowerupType.Undo).SingleOrDefault();

        if (owned == null) return;

        go = undoButton;

        go.TryGetComponent<PotionInLevelHelper>(out potionData);

        if (potionData)
        {
            potionData.buyButton.buttonEvents = null;
        }

        BasicCustomButton potionButton = null;

        go.TryGetComponent<BasicCustomButton>(out potionButton);

        if (potionButton)
        {
            potionButton.buttonEvents = null;
        }

    }

    private void InitUndoSystem()
    {

        GameObject go = null;
        PotionInLevelHelper potionData = null;
        PowerupScriptableObject chosenPower = allPowerups.Where(k => k.powerType == PowerupType.Undo).SingleOrDefault();
        OwnedPowersAndAmounts owned = ownedPowerups.Where(k => k.powerType == PowerupType.Undo).SingleOrDefault();

        if (owned == null) return;

        go = undoButton;

        go.TryGetComponent<PotionInLevelHelper>(out potionData);

        if (potionData)
        {
            potionData.buyButton.buttonEvents += () => StartCoroutine(CheckUseCoinsToUsePower(chosenPower, owned, potionData));

            potionData.SetPotionDisplay(owned.amount.ToString(), chosenPower.price.ToString(), null);

            spawnedHelpers.Add(potionData);
        }

        BasicCustomButton potionButton = null;

        go.TryGetComponent<BasicCustomButton>(out potionButton);

        if (potionButton)
        {
            potionButton.buttonEvents += () => StartCoroutine(SetUsingPotion(owned, potionData));
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

        EnableAllRelaventSelectedHighlights();

        if (TutorialManager.IS_DURING_TUTORIAL)
        {
            StartCoroutine(TutorialManager.instance.AdvanceTutorialStep());
        }

        yield return new WaitForEndOfFrame();
        UsePower(false);
    }

    private void EnableAllRelaventSelectedHighlights()
    {
        if (currentPotionDisplay)
        {
            currentPotionDisplay.SetAsSelected(true);
        }

        switch (currentPowerUsing)
        {
            case PowerupType.Switch:
                GameManager.gameRing.EnableCellsBoosterHighlights(currentPowerUsing, true);
                GameManager.gameClip.EnableSlotsBoosterHighlights(currentPowerUsing, true);
                break;
            case PowerupType.Bomb:
                GameManager.gameRing.EnableCellsBoosterHighlights(currentPowerUsing, true);
                GameManager.gameRing.EnableSlicesBoosterHighlights(true);
                break;
            case PowerupType.Joker:
                GameManager.gameRing.EnableCellsBoosterHighlights(currentPowerUsing, true);
                GameManager.gameClip.EnableSlotsBoosterHighlights(currentPowerUsing, true);
                break;
            default:
                break;
        }


    }

    private void DisableAllRelaventSelectedHighlights()
    {
        if (currentPotionDisplay)
        {
            currentPotionDisplay.SetAsSelected(false);
        }

        GameManager.gameRing.EnableCellsBoosterHighlights(currentPowerUsing, false);
        GameManager.gameClip.EnableSlotsBoosterHighlights(currentPowerUsing, false);
        GameManager.gameRing.EnableSlicesBoosterHighlights(false);
    }

    private void UsePower(bool is_Paid)
    {
        USING_POWER = true;

        if (currentPowerUsing == PowerupType.RefreshTiles || currentPowerUsing == PowerupType.Undo)
        {
            StartCoroutine(ChoosePowerToUse(is_Paid));
        }
    }

    public IEnumerator PowerSucceededUsing()
    {
        if (currentPowerData != null)
        {
            currentPowerData.amount--;

            if (currentPowerData.amount == 0)
            {
                if (currentPotionDisplay.connectedAnim)
                {
                    currentPotionDisplay.connectedAnim.SetBool("IsOFF", true);
                    currentPotionDisplay.connectedAnim.SetBool("IsON", false);
                }
            }

            if (currentPowerData.amount < 0)
            {
                currentPowerData.amount = 0;
            }

            currentPotionDisplay.SetTextCustom(currentPowerData.amount.ToString());
        }

        ResetPowerUpData();

        yield return null;
    }

    public void ManualUpdatePotionText(int index, PowerupType type)
    {
        OwnedPowersAndAmounts owned = ownedPowerups.Where(i => i.powerType == type).SingleOrDefault();

        if(owned != null)
        {
            spawnedHelpers[index].SetTextCustom(owned.amount.ToString());
        }
    }
    private IEnumerator ChoosePowerToUse(bool is_Paid)
    {
        if (currentPotionDisplay.connectedAnim)
        {
            switch (currentPowerUsing)
            {
                case PowerupType.Switch:
                    currentPotionDisplay.connectedAnim.SetTrigger("Switch");
                    break;
                case PowerupType.Bomb:
                    currentPotionDisplay.connectedAnim.SetTrigger("Bomb");
                    break;
                case PowerupType.RefreshTiles:
                    currentPotionDisplay.connectedAnim.SetTrigger("Refresh");
                    break;
                case PowerupType.Joker:
                    currentPotionDisplay.connectedAnim.SetTrigger("Joker");
                    break;
                case PowerupType.Undo:
                    break;
                default:
                    break;
            }

        }

        if (currentPowerUsing != PowerupType.Undo)
        {
            StartCoroutine(UIManager.instance.DisplayPotionUsageWindow(currentPowerData.amount == 0));

            if (is_Paid)
            {
                yield return new WaitForSeconds(0.3f); //small delay for visual catchup
                OnUseCoins();
            }

            yield return new WaitUntil(() => !UIManager.IS_DURING_POTION_USAGE);
        }
        else
        {
            if (is_Paid)
            {
                OnUseCoins();
            }
        }


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
            case PowerupType.Undo:
                UndoAcion();
                break;
            default:
                break;
        }

        if (TutorialManager.IS_DURING_TUTORIAL)
        {
            StartCoroutine(TutorialManager.instance.AdvanceTutorialStep());
        }
    }

    private void OnUseCoins()
    {
        //Manage Coin Display and data
        int currentCoins = player.GetOwnedCoins;
        int newCoins = player.GetOwnedCoins - currentChosenPowerSO.price;
        StartCoroutine(UIManager.instance.CounterText(currentCoins, newCoins, UIManager.instance.publicCoinText));
        player.RemoveCoins(currentChosenPowerSO.price);

        UIManager.instance.ManualUpdateCoinTextInLevel();
    }
    private IEnumerator CheckUseCoinsToUsePower(PowerupScriptableObject currentSO, OwnedPowersAndAmounts ownedPower, PotionInLevelHelper potionHelper)
    {
        if(player.GetOwnedCoins >= currentSO.price)
        {
            yield return new WaitForEndOfFrame();

            currentChosenPowerSO = allPowerups.Where(k => k.powerType == ownedPower.powerType).SingleOrDefault();
            currentPowerData = ownedPower;
            currentPowerUsing = ownedPower.powerType;
            currentPotionDisplay = potionHelper;

            EnableAllRelaventSelectedHighlights();

            UsePower(true);
        }
        else
        {
            UIManager.instance.DisplayBundleScreen();
        }
    }

    public void ToggleLockAllPotions(bool _isLock)
    {
        BasicCustomButton tempButton = null;

        if(spawnedHelpers.Count > 0)
        {
            foreach (var helper in spawnedHelpers)
            {
                helper.TryGetComponent<BasicCustomButton>(out tempButton);

                if(tempButton)
                {
                    tempButton.isInteractable = _isLock;
                }
            }
        }
    }

    public void ToggleLockSpecificPotion(int index, bool _isLock)
    {
        BasicCustomButton tempButton = null;

        spawnedHelpers[index].TryGetComponent<BasicCustomButton>(out tempButton);

        if (tempButton)
        {
            tempButton.isInteractable = _isLock;
        }
    }
    private bool CheckCanUsePotion()
    {


        if (localObjectToUsePowerOn == null) return false;
        IPowerUsable powerUsable;

        localObjectToUsePowerOn.TryGetComponent<IPowerUsable>(out powerUsable);
        if (powerUsable == null) return false;

        return powerUsable.CheckCanUsePower(); // if they are NOT the same - that's why it's !
    }

    public Transform ReturnPotionPosition(int index)
    {
        return potionPositions[index];
    }

    public PowerupScriptableObject publicCurrentPowerSO => currentChosenPowerSO;
    public float publicUsePotionTime => usePotionTime;
}
