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

public class UIManager : MonoBehaviour
{
    public static UIManager instance; //TEMP - LEARN DEPENDENCY INJECTION

    public static bool IS_USING_UI;
    public static bool IS_DURING_TRANSITION;
    public static bool IS_DURING_CURTAINS;
    public static bool IS_DURING_POTION_USAGE;

    [Header("General refrences")]
    [SerializeField] private Player player;
    [SerializeField] private PowerupManager powerupManager;
    [SerializeField] private MapLogic mapLogic;
    [SerializeField] private LootManager lootManager;

    [Header("Active screens")]
    [SerializeField] private BasicUIElement currentlyOpenSoloElement;
    [SerializeField] private List<BasicUIElement> currentAdditiveScreens;
    [SerializeField] private List<BasicUIElement> currentPermanentScreens;

    [Header("Map Screen")]
    [SerializeField] private BasicCustomUIWindow generalSettings;
    [SerializeField] private BasicCustomUIWindow generalMapUI;
    [SerializeField] private BasicCustomUIWindow overAllMapUI;
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
    [SerializeField] private WinLevelCustomWindow inLevelWinWindow;
    [SerializeField] private Image fillBarImageInLevel;
    [SerializeField] private TMP_Text inLevelCoinCount;

    [Header("Curtains object settings")] // might move to SO
    [DisplayWithoutEdit()]
    [SerializeField] private float currentCurtainsLevelDelay;
    [SerializeField] private float curtainsDelayOnStart;
    [SerializeField] private float waitBeforeCurtainsOutTime;
    [SerializeField] private float waitBeforeCurtainsInTime;
    [SerializeField] private float curtainsIntoMapTime;
    [SerializeField] private float curtainsOutMapTime;
    [SerializeField] private float moveToX;
    [SerializeField] private GameObject leftParent;
    [SerializeField] private GameObject rightParent;

    [SerializeField] private LeanTweenType tweenTypeIn;
    [SerializeField] private LeanTweenType tweenTypeOut;
    [SerializeField] private AnimationCurve tweenTypeInCurve;
    [SerializeField] private AnimationCurve tweenTypeOutCurve;

    [Header("Map setup")] //might move to a different script
    [SerializeField] private WorldDisplayCombo[] orderOfWorlds;
    [SerializeField] private RefWorldDisplayCombo[] worldImageReferenceCombo;

    [Header("Monetization Screens")]
    [SerializeField] private BasicCustomUIWindow bundleWindow;

    [Header("Fill Bar Temp")]
    [SerializeField] private Image fillBarImage;
    [SerializeField] private Animator fillBarAnimator;
    [SerializeField] public int fillIndex;


    [Header("Coin counter")]
    [SerializeField] private GameObject coinParent;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private int CountFPS = 30;
    [SerializeField] private float Duration = 1f;
    [SerializeField] private string NumberFormat = "N0";

    [Header("general screens")]
    [SerializeField] private BasicCustomUIWindow loadingParent;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        IS_DURING_TRANSITION = false;
        IS_USING_UI = false;
        IS_DURING_CURTAINS = false;
        IS_DURING_POTION_USAGE = false;

        DisplayLodingScreen();
    }

    private IEnumerator InitGameUI()
    {
        DisplayOverallMapUI();

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
        currentCurtainsLevelDelay = curtainsDelayOnStart;

        ManualDisplayCurtains();
        AddUIElement(loadingParent);

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


    public void ToggleLockAllCurrentScreens(bool _isLock)
    {
        if(currentlyOpenSoloElement)
        {
            foreach (var button in currentlyOpenSoloElement.ButtonRefrences)
            {
                button.isInteractable = _isLock;
            }
        }

        if(currentAdditiveScreens.Count > 0)
        {
            foreach (var screen in currentAdditiveScreens)
            {
                foreach (var button in screen.ButtonRefrences)
                {
                    button.isInteractable = _isLock;
                }
            }
        }

        if (currentPermanentScreens.Count > 0)
        {
            foreach (var screen in currentPermanentScreens)
            {
                foreach (var button in screen.ButtonRefrences)
                {
                    button.isInteractable = _isLock;
                }
            }
        }
    }
    #endregion

    #region Inside Level related actions
    public void SetInLevelUIData()
    {
        fillBarImageInLevel.fillAmount = fillBarImage.fillAmount;

        AddUIElement(inLevelUI);

        System.Action[] actions = DelegateAction(
            inLevelUI,
            new ButtonActionIndexPair { index = 0, action = DisplayInLevelSettings }, //Options button
            new ButtonActionIndexPair { index = 1, action = SoundManager.instance.ToogleMusic }, //Music icon level
            new ButtonActionIndexPair { index = 2, action = SoundManager.instance.ToggleSFX }, //SFX icon level
            new ButtonActionIndexPair { index = 3, action = DisplayInLevelExitToMapQuestion }, //to level map icon
            new ButtonActionIndexPair { index = 4, action = DisplayBundleScreen }, //shop button
            new ButtonActionIndexPair { index = 5, action = GameManager.gameClip.CallDealAction }); //deal button

        string[] texts = new string[] { ("Level " + (GameManager.instance.currentCluster.clusterID)).ToString(), player.GetOwnedCoins.ToString() };

        inLevelUI.OverrideSetMyElement(texts, null, actions);
    }

    public void ManualUpdateCoinTextInLevel()
    {
        inLevelCoinCount.text = player.GetOwnedCoins.ToString();
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
            new ButtonActionIndexPair { index = 1, action = () => CloseElement(inLevelLastDealWarning) },
            new ButtonActionIndexPair { index = 1, action = DisplayInLevelLostMessage });

        inLevelLastDealWarning.OverrideSetMyElement(null, null, actions);
    }

    public void DisplayInLevelWinWindow()
    {
        SoundManager.instance.CallPlaySound(sounds.WinScreen);
        //DeactiavteAllCustomButtons();

        System.Action[] actions = DelegateAction(
            inLevelWinWindow,
            new ButtonActionIndexPair { index = 0, action = GameManager.TestButtonDelegationWorks },
            new ButtonActionIndexPair { index = 1, action = () => ManualUpdateLevelNumText("Level " + GameManager.instance.publicMaxClusterReached.ToString()) },//the new cluster is already set from the gamemanager before the win screen appears
            new ButtonActionIndexPair { index = 1, action = () => ManualResetLevelFillBar() },//the new cluster is already set from the gamemanager before the win screen appears
            new ButtonActionIndexPair { index = 1, action = () => mapLogic.CallClusterTransfer(GameManager.instance.currentCluster) },//the new cluster is already set from the gamemanager before the win screen appears
            new ButtonActionIndexPair { index = 1, action = () => lootManager.DestroyAllLootChildren() },
            new ButtonActionIndexPair { index = 1, action = () => CloseElement(inLevelWinWindow) });

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
        SoundManager.instance.CallPlaySound(sounds.LoseScreen);

        AddUIElement(inLevelLostLevelMessage);

        System.Action[] actions = DelegateAction(
            inLevelLostLevelMessage,
            new ButtonActionIndexPair { index = 0, action = GameManager.instance.CallRestartLevel },
            new ButtonActionIndexPair { index = 0, action = () => CloseElement(inLevelLostLevelMessage) },
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
            new ButtonActionIndexPair { index = 0, action = () => GameManager.instance.InitiateDestrucionOfLevel() },
            new ButtonActionIndexPair { index = 0, action = () => StartCoroutine(GameManager.instance.OnLevelExitResetSystem()) },
            new ButtonActionIndexPair { index = 0, action = () => CloseElement(inLevelExitToMapQuesiton) },
            new ButtonActionIndexPair { index = 1, action = () => CloseElement(inLevelExitToMapQuesiton) });

        inLevelExitToMapQuesiton.OverrideSetMyElement(null, null, actions);
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

    #region Level map related actions

    public IEnumerator DisplayLevelCluster(bool isAnimate)
    {
        System.Action[] actions = DelegateAction(
            generalMapUI,
            new ButtonActionIndexPair { index = 0, action = DisplayMapSettings },
            new ButtonActionIndexPair { index = 1, action = DisplayBundleScreen });

        string[] texts = new string[] { ("Level " + (GameManager.instance.publicMaxClusterReached)).ToString() };

        generalMapUI.OverrideSetMyElement(texts, null, actions);

        yield return StartCoroutine(OnGoToLevelMapLogic(isAnimate));

        AddUIElement(generalMapUI);
    }

    public void LevelFillBarAnimate(float index)
    {
        SoundManager.instance.CallPlaySound(sounds.LevelBarFillOnWin);

        fillBarAnimator.SetTrigger("Fill" + index);
    }

    private void ManualUpdateLevelFillBar(float amount)
    {
        fillBarImageInLevel.fillAmount = amount;
        fillBarImage.fillAmount = amount;
        //LeanTween.value(fillBarImage.gameObject, fillBarImage.fillAmount, amount, 1).setEase(LeanTweenType.linear).setOnUpdate((float val) =>
        //{
        //    fillBarImage.fillAmount = val;
        //});
    }
    public void ManualResetLevelFillBar()
    {
        if (fillIndex == 1)
        {
            SoundManager.instance.CallPlaySound(sounds.LevelBarDepleteOnLose0);
        }
        else if (fillIndex == 2)
        {
            SoundManager.instance.CallPlaySound(sounds.LevelBarDepleteOnLose1);
        }
        else if (fillIndex == 3)
        {
            SoundManager.instance.CallPlaySound(sounds.LevelBarDepleteOnLose2);
        }


        fillBarAnimator.SetTrigger("ResetFill");

        fillIndex = 0;
        ManualUpdateLevelFillBar(fillIndex);
    }

    public void DisplayOverallMapUI()
    {
        System.Action[] actions = DelegateAction(
            overAllMapUI,
            new ButtonActionIndexPair { index = 0, action = () => StartCoroutine(FadeInCurtainswindow(false, false)) },
            new ButtonActionIndexPair { index = 0, action = () => StartCoroutine(GameManager.instance.InitStartLevel(false)) },
            new ButtonActionIndexPair { index = 0, action = () => CloseElement(overAllMapUI) });


        AddUIElement(overAllMapUI);

        overAllMapUI.OverrideSetMyElement(null, null, actions);
    }
    private IEnumerator OnGoToLevelMapLogic(bool isAnimate)
    {

        if (isAnimate)
        {
            yield return StartCoroutine(GameManager.instance.AnimateLevelElements(false));

            //everytime we go to map, no matter what - clear the undo system and powers
            UndoSystem.instance.ClearUndoSystem();
            powerupManager.DestroyPotions();
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

        generalSettings.OverrideSetMyElement(null, null, actions);
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

    public IEnumerator FadeInCurtainswindow(bool _In, bool doReverse)
    {
        IS_DURING_CURTAINS = true;

        System.Action actionOnEnd = doReverse ? () => StartCoroutine(ReverseFade(_In, waitBeforeCurtainsOutTime)) : () => StartCoroutine(OnEndFade());

        if (_In)
        {
            yield return new WaitForSeconds(waitBeforeCurtainsInTime);
            SoundManager.instance.CallPlaySound(sounds.CurtainsIn);
            LeanTween.moveLocalX(leftParent, moveToX, curtainsIntoMapTime).setEase(tweenTypeInCurve);
            LeanTween.moveLocalX(rightParent, -moveToX, curtainsIntoMapTime).setEase(tweenTypeInCurve).setOnComplete(actionOnEnd);
        }
        else
        {
            SoundManager.instance.CallPlaySound(sounds.CurtainsOut);
            LeanTween.moveLocalX(leftParent, 0, curtainsOutMapTime).setEase(tweenTypeOutCurve);
            LeanTween.moveLocalX(rightParent, 0, curtainsOutMapTime).setEase(tweenTypeOutCurve).setOnComplete(actionOnEnd);
        }
    }

    public void ManualDisplayCurtains()
    {
        leftParent.transform.localPosition = new Vector3(moveToX, leftParent.transform.localPosition.y, leftParent.transform.localPosition.z);
        rightParent.transform.localPosition = new Vector3(-moveToX, rightParent.transform.localPosition.y, rightParent.transform.localPosition.z);
    }
    private IEnumerator ReverseFade(bool _In, float _Time)
    {
        IS_DURING_CURTAINS = false;

        yield return new WaitForSeconds(_Time);

        StartCoroutine(FadeInCurtainswindow(!_In, false));
    }

    private IEnumerator OnEndFade()
    {

        yield return new WaitForSeconds(currentCurtainsLevelDelay);
        IS_DURING_CURTAINS = false;

        currentCurtainsLevelDelay = 0; // this variable changes in code to affect how long we wait on the end fade.
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

        CloseElement(loadingParent);

        StartCoroutine(InitGameUI());

    }


    public BasicCustomButton publicPlayButton => playButton;
    public TMP_Text publicCoinText => coinText;
}
