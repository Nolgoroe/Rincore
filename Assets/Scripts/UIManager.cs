using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Serialization;
using System.Linq;
using System.ComponentModel;

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

public enum MainScreens
{
    InLevel,
    Map,
}
public class UIManager : MonoBehaviour
{
    public static UIManager instance; //TEMP - LEARN DEPENDENCY INJECTION

    public static bool IS_USING_UI;
    public static bool IS_DURING_TRANSITION;

    [Header("General refrences")] // ask Lior if this section is ok for the long run
    [SerializeField] private Player player;
    [SerializeField] private AnimalsManager animalManager;
    [SerializeField] private PowerupManager powerupManager;
    [SerializeField] private DailyRewardsManager dailyRewardsManager;
    //[SerializeField] private MapLogic mapLogic;

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
    [SerializeField] private AnimalAlbumCustonWindow animalAlbumWindow;
    [SerializeField] private BasicCustomUIWindow animalAlbumRewardWidnow;
    [SerializeField] private DailyRewardsCustomWindow dailyRewardsWindow;

    [Header("In level Screen")]
    [SerializeField] private BasicCustomUIWindow inLevelUI;
    [SerializeField] private BasicCustomUIWindow inLevelSettingsWindow;
    [SerializeField] private BasicCustomUIWindow inLevelNonMatchTilesMessage;
    [SerializeField] private BasicCustomUIWindow inLevelLostLevelMessage;
    [SerializeField] private BasicCustomUIWindow inLevelLastDealWarning;
    [SerializeField] private BasicCustomUIWindow inLevelExitToMapQuesiton;
    [SerializeField] private BasicCustomUIWindow inLevelRestartLevelQuesiton;
    [SerializeField] private WinLevelCustomWindow inLevelWinWindow;

    [Header("Fade object settings")] // might move to SO
    [SerializeField] private BasicCustomUIWindow fadeWindow;
    [SerializeField] private float fadeIntoLevelTime;
    [SerializeField] private float fadeOutLevelTime;
    [SerializeField] private float fadeIntoMapTime;
    [SerializeField] private float fadeOutMapTime;

    [Header("Map setup")] //might move to a different script
    [SerializeField] private WorldDisplayCombo[] orderOfWorlds;
    [SerializeField] private RefWorldDisplayCombo[] worldImageReferenceCombo;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        StartCoroutine(DisplayLevelCluster(false));

        //DisplayDailyRewardsWindow(); Enable if want to show Daily Rewards
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

    private void AddUIElement(BasicUIElement UIElement)
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
    #endregion

    #region Inside Level related actions
    public void SetInLevelUIData()
    {
        //CloseAllCurrentScreens(); // close all screens open before level launch

        AddUIElement(inLevelUI);

        System.Action[] actions = DelegateAction(
            inLevelUI,
            new ButtonActionIndexPair { index = 0, action = DisplayInLevelSettings }, //Options button
            new ButtonActionIndexPair { index = 1, action = SoundManager.instance.ToogleMusic }, //Music icon level
            new ButtonActionIndexPair { index = 2, action = SoundManager.instance.ToggleSFX }, //SFX icon level
            new ButtonActionIndexPair { index = 3, action = DisplayInLevelExitToMapQuestion }, //to level map icon
            new ButtonActionIndexPair { index = 4, action = GameManager.TestButtonDelegationWorks }, //shop button
            new ButtonActionIndexPair { index = 5, action = UndoSystem.instance.CallUndoAction }); //restart button

        string[] texts = new string[] { ("Level: " + (GameManager.instance.ReturnCurrentIndexInCluster() + 1)).ToString() };

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
            new ButtonActionIndexPair { index = 1, action = DisplayInLevelLostMessage });

        inLevelLastDealWarning.OverrideSetMyElement(null, null, actions);
    }

    public void DisplayInLevelWinWindow()
    {
        DeactiavteAllCustomButtons();

        System.Action[] actions = DelegateAction(
            inLevelWinWindow,
            //new ButtonActionIndexPair { index = 0, action = () => FadeInFadeWindow(true, MainScreens.InLevel) },
            //new ButtonActionIndexPair { index = 0, action = GameManager.instance.CallNextLevel },
            new ButtonActionIndexPair { index = 1, action = () => StartCoroutine(DisplayLevelCluster(true)) },
            new ButtonActionIndexPair { index = 1, action = () => CloseElement(inLevelWinWindow) },
            new ButtonActionIndexPair { index = 1, action = () => StartCoroutine(GameManager.instance.OnLevelExitWin())});

        inLevelWinWindow.OverrideSetMyElement(GameManager.instance.ReturnStatueName(), null, actions);

        AddUIElement(inLevelWinWindow);
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
            //new ButtonActionIndexPair { index = 0, action = GameManager.instance.CallRestartLevel },
            new ButtonActionIndexPair { index = 1, action = () => StartCoroutine(DisplayLevelCluster(true)) },
            new ButtonActionIndexPair { index = 1, action = () => StartCoroutine(GameManager.instance.InitiateDestrucionOfLevel()) });

        inLevelLostLevelMessage.OverrideSetMyElement(null, null, actions);
    }

    private void DisplayInLevelExitToMapQuestion()
    {
        AddUIElement(inLevelExitToMapQuesiton);

        System.Action[] actions = DelegateAction(
            inLevelExitToMapQuesiton,
            new ButtonActionIndexPair { index = 0, action = () => StartCoroutine(DisplayLevelCluster(true)) },
            new ButtonActionIndexPair { index = 0, action = () => StartCoroutine(GameManager.instance.InitiateDestrucionOfLevel()) },
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

    //why is this here?
    public void ContinueAfterChest()
    {
        inLevelWinWindow.ManuallyShowOnlyToHudButton();
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

        bool hasEnoughRubies = player.GetOwnedRubies >= neededRubies;
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

    private IEnumerator DisplayLevelCluster(bool isAnimate)
    {
        yield return StartCoroutine(OnGoToLevelMapLogic(isAnimate));

        CloseAllCurrentScreens(); // close all screens open before going to map

        System.Action[] actions = DelegateAction(
            generalMapUI,
            new ButtonActionIndexPair { index = 0, action = DisplayMapSettings },
            new ButtonActionIndexPair { index = 1, action = GameManager.TestButtonDelegationWorks });

        string[] texts = new string[] { ("Level: " + (GameManager.instance.ReturnCurrentIndexInCluster() + 1)).ToString() };

        AddUIElement(generalMapUI);

        generalMapUI.OverrideSetMyElement(texts, null, actions);
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
        string rubiesText = player.GetOwnedRubies.ToString();
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
        //called from button

        AddUIElement(generalSettings);

        System.Action[] actions = DelegateAction(
            generalSettings,
            new ButtonActionIndexPair { index = 0, action = SoundManager.instance.ToogleMusic },
            new ButtonActionIndexPair { index = 1, action = SoundManager.instance.ToggleSFX });

        string[] texts = new string[] { "Name of player: Avishy" };
        generalSettings.OverrideSetMyElement(texts, null, actions);
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

    //private void FadeInFadeWindow(bool fadeIn, MainScreens mainScreen)
    //{
    //    IS_DURING_TRANSITION = true;

    //    float fadeInSpeed = ReturnFadeTime(fadeIn, mainScreen);

    //    CanvasGroup group = fadeWindow.group;

    //    float from = 0, to = 0;


    //    group.alpha = fadeIn == true ? 0 : 1;
    //    from = fadeIn == true ? 0 : 1;
    //    to = fadeIn == true ? 1 : 0;
    //    System.Action action = fadeIn == true ? () => StartCoroutine(ReverseFade(fadeIn, mainScreen, fadeInSpeed)) : OnEndFade;

    //    fadeWindow.gameObject.SetActive(true);

    //    fadeWindow.GeneralFloatValueTo(
    //        group,
    //        from,
    //        to,
    //        fadeInSpeed,
    //        LeanTweenType.linear,
    //        action);
    //}

    private IEnumerator ReverseFade(bool fadeIn, MainScreens mainScreen, float fadeTime)
    {
        IS_DURING_TRANSITION = false;
        // is this ok?
        // This is here for actions that want to happen on the transition between
        // fade in and out - so we for 0.5f seconds, allow actions to operate in "fade time"

        yield return new WaitForSeconds(fadeTime);

        //FadeInFadeWindow(!fadeIn, mainScreen);
    }

    private void OnEndFade()
    {
        IS_DURING_TRANSITION = false;
        CloseElement(fadeWindow);
    }
    private float ReturnFadeTime(bool fadeIn, MainScreens mainScreen)
    {
        if (fadeIn)
        {
            switch (mainScreen)
            {
                case MainScreens.InLevel:
                    return fadeIntoLevelTime;
                case MainScreens.Map:
                    return fadeIntoMapTime;
                default:
                    break;
            }
        }
        else
        {
            switch (mainScreen)
            {
                case MainScreens.InLevel:
                    return fadeOutLevelTime;
                case MainScreens.Map:
                    return fadeOutMapTime;
                default:
                    break;
            }
        }
        Debug.LogError("Some problem here");
        return -1;
    }
    public static string ToDescription(WorldEnum value)
    {
        DescriptionAttribute[] da = (DescriptionAttribute[])(value.GetType().GetField(value.ToString())).GetCustomAttributes(typeof(DescriptionAttribute), false);
        return da.Length > 0 ? da[0].Description : value.ToString();
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
}
