using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;
using System.ComponentModel;
using TMPro;

[System.Serializable]
public class WorldDisplayCombo
{
    public WorldEnum world;
    public Sprite mainSprite;
    public Sprite leftMargineSprite;
    public Sprite rightMargineSprite;

}
[System.Serializable]
public class RefWorldDisplayCombo
{
    public Image mainImageRef;
    public Image leftMargineImageRef;
    public Image rightMargineImageRef;

}
struct ButtonActionIndexPair
{
    public int index;
    public System.Action action;
}

//public enum MainScreens
//{
//    InLevel,
//    Map,
//}
public class UIManager : MonoBehaviour
{
    public static UIManager instance; //TEMP - LEARN DEPENDENCY INJECTION

    public static bool IS_USING_UI;
    public static bool IS_DURING_TRANSITION;
    public static bool IS_DURING_FADE;
    public static bool IS_DURING_POTION_USAGE;

    [Header("General refrences")] // ask Lior if this section is ok for the long run
    [SerializeField] private Player player;
    [SerializeField] private AnimalsManager animalManager;
    [SerializeField] private PowerupManager powerupManager;
    [SerializeField] private DailyRewardsManager dailyRewardsManager;
    [SerializeField] private MapLogic mapLogic;
    [SerializeField] private LootManager lootManager;

    [Header("Active screens")]
    [SerializeField] private BasicUIElement currentlyOpenSoloElement;
    [SerializeField] private List<BasicUIElement> currentAdditiveScreens;
    [SerializeField] private List<BasicUIElement> currentPermanentScreens;

    [Header("Map Screen")]
    [SerializeField] private BasicCustomUIWindow levelScrollRect;
    [SerializeField] private PlayerWorkshopCustomWindow playerWorkshopWindow;
    [SerializeField] private BasicCustomUIWindow buyPotionWindow;
    [SerializeField] private LevelMapPopupCustomWindow levelMapPopUp;
    [SerializeField] private BasicCustomUIWindow generalSettings;
    [SerializeField] private BasicCustomUIWindow generalMapUI;
    [SerializeField] private BasicCustomUIWindow overAllMapUI;
    [SerializeField] private AnimalAlbumCustonWindow animalAlbumWindow;
    [SerializeField] private BasicCustomUIWindow animalAlbumRewardWidnow;
    [SerializeField] private DailyRewardsCustomWindow dailyRewardsWindow;
    [SerializeField] private BasicCustomButton playButton;
    [SerializeField] private TMP_Text levelNumText;

    [Header("In level Screen")]
    [SerializeField] private BasicCustomUIWindow inLevelUI;
    [SerializeField] private BasicCustomUIWindow inLevelPotionUsage;
    [SerializeField] private BasicCustomUIWindow inLevelSettingsWindow;
    [SerializeField] private BasicCustomUIWindow inLevelNonMatchTilesMessage;
    [SerializeField] private BasicCustomUIWindow inLevelLostLevelMessage;
    [SerializeField] private BasicCustomUIWindow inLevelLastDealWarning;
    [SerializeField] private BasicCustomUIWindow inLevelExitToMapQuesiton;
    [SerializeField] private BasicCustomUIWindow inLevelRestartLevelQuesiton;
    [SerializeField] private WinLevelCustomWindow inLevelWinWindow;
    [SerializeField] private Image fillBarImageInLevel;

    [Header("Fade object settings")] // might move to SO
    [SerializeField] private BasicCustomUIWindow fadeWindow;
    //[SerializeField] private float fadeIntoLevelTime;
    //[SerializeField] private float fadeOutLevelTime;
    [SerializeField] private float waitBeforeFadeTime;
    [SerializeField] private float fadeIntoMapTime;
    [SerializeField] private LeanTweenType tweenType;
    //[SerializeField] private float fadeOutMapTime;

    [Header("Map setup")] //might move to a different script
    [SerializeField] private WorldDisplayCombo[] orderOfWorlds;
    [SerializeField] private RefWorldDisplayCombo[] worldImageReferenceCombo;

    [Header("Monetization Screens")]
    [SerializeField] private BasicCustomUIWindow bundleWindow;

    [Header("Fill Bar Temp")]
    [SerializeField] private Image fillBarImage;
    [SerializeField] public float[] fillAmounts;
    [SerializeField] public int fillIndex;


    [Header("Coin counter")]
    [SerializeField] private GameObject coinParent;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private int CountFPS = 30;
    [SerializeField] private float Duration = 1f;
    [SerializeField] private string NumberFormat = "N0";

    [Header("general screens")]
    [SerializeField] private BasicCustomUIWindow loadingScreen;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        DisplayLodingScreen();
        //StartCoroutine(InitGameUI());


        //DisplayDailyRewardsWindow(); Enable if want to show Daily Rewards
    }

    private IEnumerator InitGameUI()
    {
        DisplayOverallMapUI(); // - Temp??

        yield return StartCoroutine(DisplayLevelCluster(false));
    }

    private void OnValidate()
    {
        if(orderOfWorlds.Length != worldImageReferenceCombo.Length)
        {
            Debug.LogError("World counts do not match!", gameObject);
        }
    }

    #region main ui manager actions
    public void CloseElement(BasicUIElement UIElement)
    {
        if (UIElement.isSolo)
        {
            currentlyOpenSoloElement = null;
            StartCoroutine(ResetUsingUI());
        }

        if (currentAdditiveScreens.Contains(UIElement))
        {
            currentAdditiveScreens.Remove(UIElement);
        }

        if (currentPermanentScreens.Contains(UIElement))
        {
            currentPermanentScreens.Remove(UIElement);
        }

        UIElement.gameObject.SetActive(false);

        CheckResetUsingUI();
    }

    private void OpenSolo(BasicUIElement UIElement)
    {
        if (currentlyOpenSoloElement == null)
        {
            // show solo screen - CLOSE ALL ADDITIVE SCREENS

            if (currentAdditiveScreens.Count > 0)
            {
                // reverse for
                for (int i = currentAdditiveScreens.Count - 1; i >= 0; i--)
                {
                    if (!currentAdditiveScreens[i].isPermanent)
                    {
                        CloseElement(currentAdditiveScreens[i]);
                    }
                }
            }

            UIElement.gameObject.SetActive(true);
            currentlyOpenSoloElement = UIElement;
        }
        else
        {
            if (UIElement.isOverrideSolo)
            {
                currentlyOpenSoloElement.gameObject.SetActive(false);

                UIElement.gameObject.SetActive(true);
                currentlyOpenSoloElement = UIElement;
            }
            else
            {
                Debug.Log("Tried to open a solo screen on top of another solo screen, but is not overriding.");
                // do nothing
            }
        }

        IS_USING_UI = true;
    }

    private void AddAdditiveElement(BasicUIElement UIElement)
    {
        if (!currentAdditiveScreens.Contains(UIElement) && !currentPermanentScreens.Contains(UIElement))
        {
            if (UIElement.isPermanent)
            {
                currentPermanentScreens.Add(UIElement);
            }
            else
            {
                currentAdditiveScreens.Add(UIElement);
            }

            UIElement.gameObject.SetActive(true);
        }
    }

    public void AddUIElement(BasicUIElement UIElement)
    {
        if (UIElement.isSolo)
        {
            OpenSolo(UIElement);
        }
        else
        {
            AddAdditiveElement(UIElement);
        }
    }

    private void CheckResetUsingUI()
    {
        if (currentlyOpenSoloElement != null)
            return;

        if (currentAdditiveScreens.Count > 0)
            return;

        if (currentPermanentScreens.Count > 0)
            return;

        ResetUsingUI();
    }

    private IEnumerator ResetUsingUI()
    {
        yield return new WaitForEndOfFrame();
        IS_USING_UI = false;
    }

    private void CloseAllCurrentScreens()
    {
        // this function restarts ALL currently activated windows
        // including any "permanent" windows.

        if (currentlyOpenSoloElement)
        {
            CloseElement(currentlyOpenSoloElement);
        }

        if (currentAdditiveScreens.Count > 0)
        {
            // reverse for
            for (int i = currentAdditiveScreens.Count - 1; i >= 0; i--)
            {
                CloseElement(currentAdditiveScreens[i]);
            }
        }

        if (currentPermanentScreens.Count > 0)
        {
            // reverse for
            for (int i = currentPermanentScreens.Count - 1; i >= 0; i--)
            {
                CloseElement(currentPermanentScreens[i]);
            }
        }

        ResetUsingUI();
    }

    private void DeactiavteAllCustomButtons()
    {
        if (currentlyOpenSoloElement)
        {
            for (int i = 0; i < currentlyOpenSoloElement.ButtonRefrences.Length; i++)
            {
                currentlyOpenSoloElement.ButtonRefrences[i].isInteractable = false;
            }
        }

        if (currentAdditiveScreens.Count > 0)
        {
            foreach (var screen in currentAdditiveScreens)
            {
                for (int i = 0; i < screen.ButtonRefrences.Length; i++)
                {
                    screen.ButtonRefrences[i].isInteractable = false;
                }
            }
        }

        if (currentPermanentScreens.Count > 0)
        {
            foreach (var screen in currentPermanentScreens)
            {
                for (int i = 0; i < screen.ButtonRefrences.Length; i++)
                {
                    screen.ButtonRefrences[i].isInteractable = false;
                }
            }
        }
    }

    private void ActivateSingleButton(BasicCustomButton button)
    {
        DeactiavteAllCustomButtons();
        button.isInteractable = true;
    }

    public void DisplayLodingScreen()
    {
        AddUIElement(loadingScreen);

        inLevelUI.OverrideSetMyElement(null, null, null);
    }

    public IEnumerator CounterText(int currentValue, int newValue, TMP_Text in_Text)
    {
        WaitForSeconds Wait = new WaitForSeconds(1f / CountFPS);
        int previousValue = currentValue;
        int stepAmount;

        if (newValue - previousValue < 0)
        {
            stepAmount = Mathf.FloorToInt((newValue - previousValue) / (CountFPS * Duration)); // newValue = -20, previousValue = 0. CountFPS = 30, and Duration = 1; (-20- 0) / (30*1) // -0.66667 (ceiltoint)-> 0
        }
        else
        {
            stepAmount = Mathf.CeilToInt((newValue - previousValue) / (CountFPS * Duration)); // newValue = 20, previousValue = 0. CountFPS = 30, and Duration = 1; (20- 0) / (30*1) // 0.66667 (floortoint)-> 0
        }

        if (previousValue < newValue)
        {
            while (previousValue < newValue)
            {
                previousValue += stepAmount;
                if (previousValue > newValue)
                {
                    previousValue = newValue;
                }

                in_Text.SetText(previousValue.ToString(NumberFormat));

                yield return Wait;
            }
        }
        else
        {
            while (previousValue > newValue)
            {
                previousValue += stepAmount;
                if (previousValue < newValue)
                {
                    previousValue = newValue;
                }

                in_Text.SetText(previousValue.ToString(NumberFormat));

                yield return Wait;
            }
        }
    }
    #endregion

    #region Inside Level related actions
    public void SetInLevelUIData()
    {
        //CloseAllCurrentScreens(); // close all screens open before level launch

        fillBarImageInLevel.fillAmount = fillBarImage.fillAmount;

        AddUIElement(inLevelUI);

        System.Action[] actions = DelegateAction(
            inLevelUI,
            new ButtonActionIndexPair { index = 0, action = DisplayInLevelSettings }, //Options button
            new ButtonActionIndexPair { index = 1, action = SoundManager.instance.ToogleMusic }, //Music icon level
            new ButtonActionIndexPair { index = 2, action = SoundManager.instance.ToggleSFX }, //SFX icon level
            new ButtonActionIndexPair { index = 3, action = DisplayInLevelExitToMapQuestion }, //to level map icon
            new ButtonActionIndexPair { index = 4, action = DisplayBundleScreen }, //shop button
            new ButtonActionIndexPair { index = 5, action = GameManager.gameClip.CallDealAction}, //deal button
            new ButtonActionIndexPair { index = 6, action = UndoSystem.instance.CallUndoAction }); //restart button

        string[] texts = new string[] { ("Level: " + (GameManager.instance.currentCluster.clusterID)).ToString() };

        inLevelUI.OverrideSetMyElement(texts, null, actions);
    }

    public void DisplayInLevelRingHasNonMatchingMessage()
    {
        AddUIElement(inLevelNonMatchTilesMessage);

        System.Action[] actions = DelegateAction(
            inLevelNonMatchTilesMessage,
            new ButtonActionIndexPair { index = 0, action = GameManager.gameControls.ReturnHomeBadRingConnections },
            new ButtonActionIndexPair { index = 0, action = () => CloseElement(inLevelNonMatchTilesMessage) },
            new ButtonActionIndexPair { index = 1, action = DisplayInLevelLostMessage });

        inLevelNonMatchTilesMessage.OverrideSetMyElement(null, null, actions);
    }

    public void DisplayInLevelLastDealWarning()
    {
        AddUIElement(inLevelLastDealWarning);

        System.Action[] actions = DelegateAction(
            inLevelLastDealWarning,
            new ButtonActionIndexPair { index = 0, action = () => CloseElement(inLevelLastDealWarning) },
            new ButtonActionIndexPair { index = 0, action = () => ClipManager.canUseDeal = true },
            //new ButtonActionIndexPair { index = 1, action = () => FadeInFadeWindow(true, MainScreens.InLevel) },
            //new ButtonActionIndexPair { index = 1, action = GameManager.instance.CallRestartLevel });
            new ButtonActionIndexPair { index = 1, action = () => CloseElement(inLevelLastDealWarning) },
            new ButtonActionIndexPair { index = 1, action = DisplayInLevelLostMessage });

        inLevelLastDealWarning.OverrideSetMyElement(null, null, actions);
    }

    public void DisplayInLevelWinWindow()
    {
        //DeactiavteAllCustomButtons();

        System.Action[] actions = DelegateAction(
            inLevelWinWindow,
            new ButtonActionIndexPair { index = 0, action = GameManager.TestButtonDelegationWorks },
            new ButtonActionIndexPair { index = 1, action = () => ManualUpdateLevelNumText("Level: " + GameManager.instance.publicMaxClusterReached.ToString()) },//the new cluster is already set from the gamemanager before the win screen appears
            new ButtonActionIndexPair { index = 1, action = () => ManualResetLevelFillBar() },//the new cluster is already set from the gamemanager before the win screen appears
            new ButtonActionIndexPair { index = 1, action = () => mapLogic.CallClusterTransfer(GameManager.instance.currentCluster) },//the new cluster is already set from the gamemanager before the win screen appears
            new ButtonActionIndexPair { index = 1, action = () => lootManager.DestroyAllLootChildren() },
            new ButtonActionIndexPair { index = 1, action = () => CloseElement(inLevelWinWindow) });
            //new ButtonActionIndexPair { index = 1, action = () => StartCoroutine(GameManager.instance.OnLevelExitWin(false))});

        inLevelWinWindow.OverrideSetMyElement(null, null, actions);

        AddUIElement(inLevelWinWindow);
    }

    private void ManualUpdateLevelNumText(string _InText)
    {
        levelNumText.text = _InText;
    }

    public IEnumerator DisplayPotionUsageWindow(bool isBought)
    {
        coinParent.SetActive(isBought);

        IS_DURING_POTION_USAGE = true;

        Sprite[] sprites = new Sprite[] { powerupManager.publicCurrentPowerSO.potionSprite };
        string[] texts = new string[] { player.GetOwnedCoins.ToString() };

        inLevelPotionUsage.OverrideSetMyElement(texts, sprites, null);

        AddUIElement(inLevelPotionUsage);

        yield return new WaitForSeconds(powerupManager.publicUsePotionTime);
        CloseElement(inLevelPotionUsage);
        IS_DURING_POTION_USAGE = false;
    }

    private void DisplayInLevelSettings()
    {
        // options for this screen get thier actions from the DisplayInLevelUI
        if (inLevelSettingsWindow.gameObject.activeInHierarchy)
        {
            CloseElement(inLevelSettingsWindow);
        }
        else
        {
            AddUIElement(inLevelSettingsWindow);
        }
    }

    private void DisplayInLevelLostMessage()
    {
        AddUIElement(inLevelLostLevelMessage);

        System.Action[] actions = DelegateAction(
            inLevelLostLevelMessage,
            //new ButtonActionIndexPair { index = 0, action = () => FadeInFadeWindow(true, MainScreens.InLevel) },
            new ButtonActionIndexPair { index = 0, action = GameManager.instance.CallRestartLevel },
            new ButtonActionIndexPair { index = 0, action = () => CloseElement(inLevelLostLevelMessage) },
            //new ButtonActionIndexPair { index = 1, action = () => StartCoroutine(DisplayLevelCluster(true)) },
            new ButtonActionIndexPair { index = 1, action = () => GameManager.instance.InitiateDestrucionOfLevel() },
            new ButtonActionIndexPair { index = 1, action = () => CloseElement(inLevelLostLevelMessage) },
            new ButtonActionIndexPair { index = 1, action = () => StartCoroutine(GameManager.instance.OnLevelExitResetSystem()) });

        inLevelLostLevelMessage.OverrideSetMyElement(null, null, actions);
    }

    private void DisplayInLevelExitToMapQuestion()
    {
        AddUIElement(inLevelExitToMapQuesiton);

        System.Action[] actions = DelegateAction(
            inLevelExitToMapQuesiton,
            //new ButtonActionIndexPair { index = 0, action = () => StartCoroutine(DisplayLevelCluster(true)) },
            new ButtonActionIndexPair { index = 0, action = () => GameManager.instance.InitiateDestrucionOfLevel() },
            new ButtonActionIndexPair { index = 0, action = () => StartCoroutine(GameManager.instance.OnLevelExitResetSystem()) },
            new ButtonActionIndexPair { index = 0, action = () => CloseElement(inLevelExitToMapQuesiton) },
            new ButtonActionIndexPair { index = 1, action = () => CloseElement(inLevelExitToMapQuesiton) });

        inLevelExitToMapQuesiton.OverrideSetMyElement(null, null, actions);
    }

    private void DisplayInLevelRestartLevelQuestion()
    {
        AddUIElement(inLevelRestartLevelQuesiton);

        System.Action[] actions = DelegateAction(
            inLevelRestartLevelQuesiton,
            //new ButtonActionIndexPair { index = 0, action = () => FadeInFadeWindow(true, MainScreens.InLevel) },
            new ButtonActionIndexPair { index = 0, action = GameManager.instance.CallRestartLevel },
            new ButtonActionIndexPair { index = 0, action = () => CloseElement(inLevelRestartLevelQuesiton) },
            new ButtonActionIndexPair { index = 1, action = () => CloseElement(inLevelRestartLevelQuesiton) });

        inLevelRestartLevelQuesiton.OverrideSetMyElement(null, null, actions);
    }

    #endregion

    #region Monetization actions

    public void DisplayBundleScreen()
    {
        AddUIElement(bundleWindow);

        System.Action[] actions = DelegateAction(
            inLevelUI,
            new ButtonActionIndexPair { index = 0, action = GameManager.TestButtonDelegationWorks }, 
            new ButtonActionIndexPair { index = 1, action = GameManager.TestButtonDelegationWorks }, 
            new ButtonActionIndexPair { index = 2, action = GameManager.TestButtonDelegationWorks }, 
            new ButtonActionIndexPair { index = 3, action = GameManager.TestButtonDelegationWorks }, 
            new ButtonActionIndexPair { index = 4, action = () => CloseElement(bundleWindow) }); 

        bundleWindow.OverrideSetMyElement(null, null, actions);
    }

    #endregion
    //why is this here?
    public void ContinueAfterChest()
    {
        //inLevelWinWindow.ManuallyShowOnlyToHudButton();
    }

    #region Level map related actions
    public void DisplayLaunchLevelPopUp(LevelSO levelSO)
    {
        string[] texts = new string[] { "Level " + levelSO.levelNumInZone.ToString(), ToDescription(levelSO.worldName)};

        System.Action[] actions = DelegateAction(
            levelMapPopUp,
            //new ButtonActionIndexPair { index = 0, action = () => FadeInFadeWindow(true, MainScreens.InLevel) },
            new ButtonActionIndexPair { index = 0, action = () => StartCoroutine(GameManager.instance.AnimateLevelElements(true))},
            new ButtonActionIndexPair { index = 0, action = GameManager.instance.SetLevel},
            new ButtonActionIndexPair { index = 0, action = () => CloseElement(levelMapPopUp)});

        AddUIElement(levelMapPopUp);

        levelMapPopUp.OverrideSetMyElement(texts, null, actions);
    }

    public void RefreshRubyAndTearsTexts(int tearsAmount, int rubiesAmount)
    {
        generalMapUI.TextRefrences[0].text = tearsAmount.ToString(); // dew drops text
        generalMapUI.TextRefrences[1].text = rubiesAmount.ToString(); // rubies text
    }

    public void DisplayBuyPotionWindow(int neededRubies)
    {
        //how to color text red/white if have enough rubies???

        System.Action[] actions = DelegateAction(
            buyPotionWindow,
            new ButtonActionIndexPair { index = 0, action = () => powerupManager.BuyPotion() },
            new ButtonActionIndexPair { index = 0, action = () => CloseElement(buyPotionWindow) },
            new ButtonActionIndexPair { index = 1, action = () => CloseElement(buyPotionWindow) });

        AddUIElement(buyPotionWindow);

        bool hasEnoughRubies = player.GetOwnedCoins >= neededRubies;
        string[] texsts = new string[] { neededRubies.ToString() };

        buyPotionWindow.OverrideSetMyElement(texsts, null, actions);
    }

    public void DisplayAnimalAlbumReward(int amountOfReward)
    {
        System.Action[] actions = DelegateAction(
            animalAlbumRewardWidnow,
            new ButtonActionIndexPair { index = 0, action = () => CloseElement(animalAlbumRewardWidnow) });

        AddUIElement(animalAlbumRewardWidnow);

        string[] texts = new string[] { amountOfReward.ToString() };

        animalAlbumRewardWidnow.OverrideSetMyElement(texts, null, actions);
    }

    public void DisplayDailyRewardsWindow()
    {
        System.Action[] actions = DelegateAction(
            dailyRewardsWindow,
            new ButtonActionIndexPair { index = 0, action = () => StartCoroutine(dailyRewardsManager.RecieveReward()) });

        AddUIElement(dailyRewardsWindow);

        dailyRewardsWindow.OverrideSetMyElement(null, null, actions);
    }

    public IEnumerator DisplayLevelCluster(bool isAnimate)
    {
        System.Action[] actions = DelegateAction(
            generalMapUI,
            new ButtonActionIndexPair { index = 0, action = DisplayMapSettings },
            new ButtonActionIndexPair { index = 1, action = DisplayBundleScreen });

        string[] texts = new string[] { ("Level: " + (GameManager.instance.publicMaxClusterReached)).ToString() };

        generalMapUI.OverrideSetMyElement(texts, null, actions);

        yield return StartCoroutine(OnGoToLevelMapLogic(isAnimate));

        //CloseAllCurrentScreens(); // close all screens open before going to map


        AddUIElement(generalMapUI);


        //if(isAnimate)
        //{
        //    yield return new WaitForSeconds(2); // this is the time it takes to move to next level on map - for now it's hardcoded.
        //}
        //generalMapUI.OverrideSetMyElement(texts, null, actions);
    }

    public void ManualUpdateLevelFillBar(float amount) //TEMP
    {
        LeanTween.value(fillBarImage.gameObject, fillBarImage.fillAmount, amount, 1).setEase(LeanTweenType.linear).setOnUpdate((float val) =>
        {
            fillBarImage.fillAmount = val;

        });
    }
    public void ManualResetLevelFillBar() //TEMP
    {
        fillIndex = 0;
        ManualUpdateLevelFillBar(fillIndex);
    }

    public void DisplayOverallMapUI()
    {
        System.Action[] actions = DelegateAction(
            overAllMapUI,
            new ButtonActionIndexPair { index = 0, action = () => StartCoroutine(GameManager.instance.InitStartLevel(false)) },
            new ButtonActionIndexPair { index = 0, action = () => CloseElement(overAllMapUI) });


        AddUIElement(overAllMapUI);

        overAllMapUI.OverrideSetMyElement(null, null, actions);
    }
    private IEnumerator OnGoToLevelMapLogic(bool isAnimate)
    {
        //everytime we go to map, no matter what - clear the undo system
        UndoSystem.instance.ClearUndoSystem();
        powerupManager.DestroyPotions();

        if (isAnimate)
        {
            yield return StartCoroutine(GameManager.instance.AnimateLevelElements(false));
        }

    }

    private void DisplayAnimalAlbum()
    {
        string tearsText = player.GetOwnedTears.ToString();
        string rubiesText = player.GetOwnedCoins.ToString();
        string[] texts = new string[] { tearsText, rubiesText };

        System.Action[] actions = DelegateAction(
            animalAlbumWindow,
            new ButtonActionIndexPair { index = 0, action = () => animalAlbumWindow.SwitchAnimalCategory(0) }, // Fox type
            new ButtonActionIndexPair { index = 1, action = () => animalAlbumWindow.SwitchAnimalCategory(1) }, // Stag type
            new ButtonActionIndexPair { index = 2, action = () => animalAlbumWindow.SwitchAnimalCategory(2) }, // Owl type
            new ButtonActionIndexPair { index = 3, action = () => animalAlbumWindow.SwitchAnimalCategory(3) }, // Boar type
            new ButtonActionIndexPair { index = 4, action = () => animalAlbumWindow.GivePlayerRewardsFromAnimalAlbum() }); // show animal reward window and give reward

        AddUIElement(animalAlbumWindow);

        animalAlbumWindow.OverrideSetMyElement(null, null, actions);
        animalAlbumWindow.InitAnimalAlbum(animalManager, player);
    }

    private IEnumerator OpenPotionsCategory()
    {
        //if succeds it opens the potions screen
        if (!playerWorkshopWindow.TrySwitchCategory(1))
        {
            yield break;
        }

        if (powerupManager.unlockedPowerups.Count > 0)
        {
            //summon all potion buttons
            foreach (PowerupType powerType in powerupManager.unlockedPowerups)
            {
                powerupManager.InstantiatePowerButton(powerType);
            }

            yield return new WaitForEndOfFrame();

            foreach (PotionCustomButton customButton in powerupManager.customPotionButtons)
            {
                customButton.SetOriginalPos();
            }
            //set selected potion
            powerupManager.SetSelectedPotion(powerupManager.unlockedPowerups[0]);
        }
        else
        {
            Debug.Log("No owned potions, can't open potion screen");
            //show error message
        }
    }

    private void DisplayMapSettings()
    {
        // options for this screen get thier actions from the DisplayInLevelUI
        if (generalSettings.gameObject.activeInHierarchy)
        {
            CloseElement(generalSettings);
            return;
        }
        else
        {
            AddUIElement(generalSettings);
        }

        //called from button

        System.Action[] actions = DelegateAction(
            generalSettings,
            new ButtonActionIndexPair { index = 0, action = SoundManager.instance.ToogleMusic },
            new ButtonActionIndexPair { index = 1, action = SoundManager.instance.ToggleSFX });

        //string[] texts = new string[] { "Name of player: Avishy" };
        generalSettings.OverrideSetMyElement(null, null, actions);
    }

    private void DisplayPlayerWorkshop()
    {
        System.Action[] actions = DelegateAction(
            playerWorkshopWindow,
            new ButtonActionIndexPair { index = 0, action = () => playerWorkshopWindow.TrySwitchCategory(0) }, // inventory catagory
            new ButtonActionIndexPair { index = 0, action = () => playerWorkshopWindow.SortWorkshop(0) }, // inventory catagory
            new ButtonActionIndexPair { index = 0, action = () => powerupManager.ClearPowerupScreenDataComplete() },// inventory catagory
            new ButtonActionIndexPair { index = 1, action = () => StartCoroutine(OpenPotionsCategory()) }, // potion catagory
            new ButtonActionIndexPair { index = 2, action = () => playerWorkshopWindow.SortWorkshop(0) }, // inventory build sort
            new ButtonActionIndexPair { index = 3, action = () => playerWorkshopWindow.SortWorkshop(1) }, // inventory gem sort
            new ButtonActionIndexPair { index = 4, action = () => playerWorkshopWindow.SortWorkshop(2) }, // inventory herb sort
            new ButtonActionIndexPair { index = 5, action = () => playerWorkshopWindow.SortWorkshop(3) }, // inventory witchcraft sort
            new ButtonActionIndexPair { index = 6, action = () => powerupManager.TryBrewPotion() }); // potion brew button


        AddUIElement(playerWorkshopWindow);

        playerWorkshopWindow.OverrideSetMyElement(null, null, actions);

        playerWorkshopWindow.InitPlayerWorkshop();
    }
    #endregion

    #region  general
    private System.Action[] DelegateAction(BasicUIElement widnow, params ButtonActionIndexPair[] buttonActionIndexPair)
    {
        System.Action[] actions = new System.Action[widnow.ButtonRefrences.Length];

        for (int i = 0; i < buttonActionIndexPair.Length; i++)
        {
            actions[buttonActionIndexPair[i].index] += buttonActionIndexPair[i].action;
        }

        // you can add any other functionality to all buttons here

        return actions;
    }

    [ContextMenu("Re-order Map")]
    private void ReOrderMapDisplay()
    {
        int id = 0;
        foreach (RefWorldDisplayCombo combo in worldImageReferenceCombo)
        {
            combo.mainImageRef.sprite = orderOfWorlds[id].mainSprite;
            combo.leftMargineImageRef.sprite = orderOfWorlds[id].leftMargineSprite;
            combo.rightMargineImageRef.sprite = orderOfWorlds[id].rightMargineSprite;
            id++;
        }
    }

    public void FadeInFadeWindow()
    {
        IS_DURING_FADE = true;

        CanvasGroup group = fadeWindow.group;



        float from = 0, to = 0;

        group.alpha = 1;
        from = 1;
        to = 0;
        //from = fadeIn == true ? 0 : 1;
        //to = fadeIn == true ? 1 : 0;
        //System.Action action = fadeIn == true ? () => StartCoroutine(ReverseFade(fadeIn, mainScreen, fadeInSpeed)) : OnEndFade;
        System.Action action = OnEndFade;

        fadeWindow.gameObject.SetActive(true);

        fadeWindow.GeneralFloatValueTo(
            group,
            from,
            to,
            fadeIntoMapTime,
            tweenType,
            action);

    }

    //private IEnumerator ReverseFade(bool fadeIn, MainScreens mainScreen, float fadeTime)
    //{
    //    IS_DURING_TRANSITION = false;
    //    // is this ok?
    //    // This is here for actions that want to happen on the transition between
    //    // fade in and out - so we for 0.5f seconds, allow actions to operate in "fade time"

    //    yield return new WaitForSeconds(fadeTime);

    //    //FadeInFadeWindow(!fadeIn, mainScreen);
    //}

    private void OnEndFade()
    {
        IS_DURING_FADE = false;
        CloseElement(fadeWindow);
    }

    //private float ReturnFadeTime(bool fadeIn, MainScreens mainScreen)
    //{
    //    if (fadeIn)
    //    {
    //        switch (mainScreen)
    //        {
    //            case MainScreens.InLevel:
    //                return fadeIntoLevelTime;
    //            case MainScreens.Map:
    //                return fadeIntoMapTime;
    //            default:
    //                break;
    //        }
    //    }
    //    else
    //    {
    //        switch (mainScreen)
    //        {
    //            case MainScreens.InLevel:
    //                return fadeOutLevelTime;
    //            case MainScreens.Map:
    //                return fadeOutMapTime;
    //            default:
    //                break;
    //        }
    //    }
    //    Debug.LogError("Some problem here");
    //    return -1;
    //}
    public static string ToDescription(WorldEnum value)
    {
        DescriptionAttribute[] da = (DescriptionAttribute[])(value.GetType().GetField(value.ToString())).GetCustomAttributes(typeof(DescriptionAttribute), false);
        return da.Length > 0 ? da[0].Description : value.ToString();
    }


    public void HideSpecificButton(CustomButtonParent button)
    {
        button.gameObject.SetActive(false);
    }
    public void ShowSpecificButton(CustomButtonParent button)
    {
        button.gameObject.SetActive(true);
    }
    #endregion


#if UNITY_EDITOR

    [MenuItem("Build Preperation/Prepare UI for build")]

    static void DeactivateAllWindows()
    {
        CustomWindowParent[] allGameWindows = FindObjectsOfType<CustomWindowParent>();
        foreach (BasicUIElement window in allGameWindows)
        {
            window.gameObject.SetActive(false);
        }
    }

#endif











    public void OnLoadData()
    {
        //fillIndex = SaveLoad.instance.indexReachedInCluster - 1; // we set the bar to the current fill amount reached

        StartCoroutine(InitGameUI());

        CloseElement(loadingScreen);
    }
















    public BasicCustomButton publicPlayButton => playButton;
    public TMP_Text publicCoinText => coinText;
}
