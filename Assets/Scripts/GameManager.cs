using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GameAnalyticsSDK;
using System;

public class GameManager : MonoBehaviour
{
    const string ANIM_SET_RIVE = "Set Rive ";
    const string ANIM_CLEAR_RIVE = "Clear Rive ";

    public static bool IS_IN_LEVEL;

    public static Vector3 GENERAL_TILE_SIZE = new Vector3(1, 1, 1); 
    public static GameManager instance; //TEMP - LEARN DEPENDENCY INJECTION

    [Header("Level setup Data")]
    [SerializeField] private ClusterSO currentClusterSO;
    [SerializeField] private int currentIndexInCluster;
    //[SerializeField] private int currentMaxLevelReached;
    [SerializeField] private int currentMaxClusterReached;

    //[SerializeField] private Transform levelCameraParent = null;
    [SerializeField] private Transform levelDecksParent = null;
    [SerializeField] private Animator cameraAnimatorController;
    [SerializeField] private Animator clipAnimatorController;
    [SerializeField] private Animator potionDeckAnimatorController;


    [Header("Level Animation Data")]
    //[SerializeField] private float delayAfterLevelExit = 1.2f;
    [SerializeField] private float delayClipAppear = 0.4f;
    [SerializeField] private float timeClipEnter = 0.4f;
    [SerializeField] private float delayClipHide = 0.4f;
    [SerializeField] private float delayRestartCluster =1f;

    [Header("In game Data")]
    public LevelSO nextLevel;
    public static Ring gameRing;
    public static ClipManager gameClip;
    public static LevelSO currentLevel;
    public static InLevelUserControls gameControls;
    //public ChestLogic summonedChest; //temp?
    //public ChestBarLogic chestBarLogic; //temp?

    [SerializeField] private AnimalStatueData currentLevelAnimalStatue; //temp?
    [SerializeField] private Animator currentLevelGeneralStatueAnimator;//temp?
    [SerializeField] private bool isAnimalLevel;

    private System.Action BeforeRingActions;
    private System.Action RingActions;
    private System.Action AfterRingActions;
    private System.Action WinLevelActions;
    //private System.Action LoseLevelActions;
    /// <summary>
    /// Never add to this directly - always use the function "AddToEndlevelCleanup(action to add) with the action we want to add".
    /// We do this since we REQUIRE that the last action will be a specific one.
    /// </summary>
    private System.Action endLevelActions;

    [Header("General refrences")]
    [SerializeField] private Transform inLevelParent;
    [SerializeField] private ZoneManager zoneManager;
    [SerializeField] private LootManager lootManager;
    [SerializeField] private AnimalsManager animalsManager;
    [SerializeField] private Player player;
    [SerializeField] private MapLogic mapLogic;
    [SerializeField] private ClipManager clipManager;
    [SerializeField] private PowerupManager powerupManager;
    [SerializeField] private TutorialManager tutorialManager;




    [SerializeField] private GameObject[] gameRingsPrefabs;
    [SerializeField] private GameObject[] gameRingsSlicePrefabs;
    [SerializeField] private GameObject[] gameRingsClipPrefabs;
    [SerializeField] private GameObject[] gameRingsUserControlsPrefabs;

    [Header("Inspector actions and Data")]
    public ClusterSO[] allClusters;


    [Header("Level Animation Data")]
    [SerializeField] private float delayCheckDoTutorial;


    [Header("TEMP")]
    public int testFirebase;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        Application.targetFrameRate = 30;


        // TO DO
        // if we use a scene transfer system then  make sure the Instance is deleted if we transfer a scene
        // consider changing Sigleton access to something else.


        //SetLevel(currentLevel);

        GameAnalytics.Initialize();

        gameClip = clipManager;
        LeanTween.init(5000);

        //mapLogic.InitMapLogic(currentClusterSO);
        //UIManager.instance.ShowSpecificButton(UIManager.instance.publicPlayButton); // temp here!

        //StartCoroutine(mapLogic.HideRingDarkOverlay(0));

        //currentMaxLevelReached = currentClusterSO.clusterLevels[0].levelNumInZone; //TEMP

        testFirebase = (int)Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("Key_1").DoubleValue;
    }


    private void Update()
    {
        //if(gameClip)
        //{
        //    Debug.Log("game clip is summoned");
        //}

        if(Input.GetKeyDown(KeyCode.X))
        {
            BroadcastWinLevelActions();
        }
    }

    //called from button click
    public void SetLevel()
    {
        GameAnalytics.NewDesignEvent("Testing GA 1", 0);

        //first clean all subscribes if there are any.
        endLevelActions?.Invoke();

        //currentLevel = level; //choose level here

        //this is the only place in code where we add delegates to the actions of before, during and after ring.
        // this will not actually invoke the unity event functions - it will add it's invoked functions to the action in the order they are created.
        BeforeRingActions += () => currentLevel.beforeRingSpawnActions.Invoke();
        //BeforeRingActions += SpawnLevelBG;

        RingActions += BuildLevel;
        RingActions += () => currentLevel.ringSpawnActions.Invoke();

        //AfterRingActions += () => currentLevel.afterRingSpawnActions.Invoke();

        SymbolAndColorCollector.instance.ResetData();


        StartCoroutine(StartLevel());
    }

    private IEnumerator StartLevel()
    {
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "World1", "Level " + currentLevel.levelNumInZone.ToString());

        IS_IN_LEVEL = true;

        isAnimalLevel = false; //maybe have a reset function

        //yield return new WaitUntil(() => !UIManager.IS_DURING_TRANSITION);
        yield return new WaitForEndOfFrame();

        //Before Ring
        BeforeRingActions?.Invoke();

        //Ring
        RingActions?.Invoke();

        //After Ring
        AfterRingActions?.Invoke();


        //AddToEndlevelActions(() => StartCoroutine(OnLevelExitLose()));
        AddToEndlevelActions(ClearLevelData);


        if (currentIndexInCluster + 1 == currentClusterSO.clusterLevels.Length)
        {
            WinLevelActions += () => StartCoroutine(InitClusterTransfer());
        }
        else
        {
            WinLevelActions += SetDataOnWin;
            WinLevelActions += () => StartCoroutine(OnLevelExitWin(false));
        }

        SymbolAndColorCollector.instance.DoTotalCheck(); // we do this in case there are preplaced tiles that need to be counted.


        yield return new WaitForSeconds(delayCheckDoTutorial);

        tutorialManager.SetCurrenTutorialStepData(currentLevel.levelTutorial, 0);
        //gameClip.CheckCustomClipAmount();
    }

    private void SetDataOnWin()
    {
        currentIndexInCluster++;

        //StartCoroutine(mapLogic.HideRingDarkOverlay(currentIndexInCluster)); //unlock dark overlay of next level
    }

    private void BuildLevel()
    {
        //// All of these should be part of the list "Before ring spawn actions" or "after...."???? (either? or? none?)

        // Spawn ring by type from level
        //gameRing = Instantiate(gameRingsPrefabs[(int)currentLevel.ringType], inLevelParent).GetComponent<Ring>();
        if (!gameRing)
        {
            Debug.LogError("No ring!");
        }
        else
        {
            //gameRing.levelStartCollider.enabled = false;

            //gameRing.InitRing();
        }

        // Spawn clip by type from level (or a general clip)

        //gameClip = Instantiate(gameRingsClipPrefabs[(int)currentLevel.ringType], inLevelParent).GetComponent<ClipManager>();
        if (!gameClip)
        {
            Debug.LogError("No Clip!");
        }
        else
        {
            // Init clip - spawn according to rules
            gameClip.InitClipManager();
        }


        //Spawn User Controls For Level
        gameControls = Instantiate(gameRingsUserControlsPrefabs[(int)currentLevel.ringType], inLevelParent).GetComponent<InLevelUserControls>();
        if (!gameControls)
        {
            Debug.LogError("No User Controls!");
        }

        //local Init User Controls For Level - we don't do enoguh to merit own Init function
        gameControls.InitUserControls(gameRing, gameClip);

        //Init slices that pass information to cells (run 2)


        powerupManager.SpawnPotions();

        //currentMaxLevelReached = currentLevel.levelNumInZone;
    }

    private void SpawnLevelBG()
    {
        ZoneMaterialData go = Resources.Load<ZoneMaterialData>(zoneManager.ReturnBGPathByType(currentLevel.worldName));
        ZoneMaterialData levelBG = Instantiate(go, inLevelParent);

        if(levelBG)
        {
            levelBG.ChangeZoneToBlurryZoneDisplay();
        }
    }

    public IEnumerator AnimateLevelElements(bool inLevel)
    {
        if(inLevel)
        {
            //mapLogic.FixCamPosStartLevel(currentIndexInCluster);
            cameraAnimatorController.SetTrigger("Camera In Level" + currentIndexInCluster);

            yield return new WaitForSeconds(delayClipAppear);
            levelDecksParent.gameObject.SetActive(true);


            // every level launch, no matter what, we launch the in level UI after it enters the view.
            // we do this BEFORE setting the win level and end level actions
            UIManager.instance.SetInLevelUIData();

            clipAnimatorController.SetTrigger("Clip In Level");
            potionDeckAnimatorController.SetTrigger("Potion In Level");

            yield return new WaitForSeconds(timeClipEnter);
            mapLogic.ToggleRings(gameRing, inLevel);



            UIManager.IS_DURING_TRANSITION = false;
        }
        else
        {
            //UIManager.IS_DURING_TRANSITION = true;

            mapLogic.ToggleRings(gameRing, inLevel);

            clipAnimatorController.SetTrigger("Clip Out Level");
            potionDeckAnimatorController.SetTrigger("Potion Out Level");

            yield return new WaitForSeconds(delayClipHide);

            //StartCoroutine(mapLogic.FixCamPosEndLevel());

            //int currentAnimPlay = 0;
            //currentAnimPlay =  currentIndexInCluster == 0 ? 0 : currentIndexInCluster - 1;

            
            cameraAnimatorController.SetTrigger("Camera Out Level" + currentIndexInCluster); /// we do -1 here since we already incremented the current index in the cluster
        }


        if (!inLevel)
        {
            levelDecksParent.gameObject.SetActive(false);
        }
    }
    private void ClearLevelData()// this must be added last to "endLevelActions"
    {
        BeforeRingActions = null;
        RingActions = null;
        AfterRingActions = null;
        WinLevelActions = null;
        //LoseLevelActions = null;
        endLevelActions = null;
    }

    public void InitiateDestrucionOfLevel()
    {
        //yield return new WaitUntil(() => !UIManager.IS_DURING_TRANSITION);
        Debug.Log("Initiating destruction");
        endLevelActions?.Invoke();
    }

    public IEnumerator InitiateDestrtuctionOfCluster()
    {
        yield return new WaitUntil(() => !UIManager.IS_DURING_TRANSITION);
        Debug.Log("Initiating destruction");
        //endLevelActions?.Invoke();

    }

    // This function makes sure that we have "ClearLevelActions" set as the last action to be made
    private void AddToEndlevelActions(System.Action actionToAdd)
    {
        endLevelActions -= ClearLevelData;// this has to be the last added func

        endLevelActions += actionToAdd;

        endLevelActions += ClearLevelData;// this has to be the last added func

    }
    private void RemoveFromEndlevelActions(System.Action actionToAdd)
    {
        endLevelActions -= actionToAdd;

    }


    [ContextMenu("Restart")]
    public void CallRestartLevel()
    {
        StartCoroutine(RestartLevel());
    }
    private IEnumerator RestartLevel()
    {
        //yield return new WaitUntil(() => !UIManager.IS_DURING_TRANSITION);

        for (int k = 0; k < 1; k++)
        {
           yield return StartCoroutine(OnLevelExitResetLevel());
        }
    }

    public void ClickOnLevelIconMapSetData(LevelPresetData data)
    {
        currentLevel = data.connectedLevelSO;
        
        gameRing = data.connectedRing;

        currentClusterSO = data.connectedCluster;
        currentIndexInCluster = data.indexInCluster;
    }

    public void SetRingmanually(Ring ring)
    {
        gameRing = ring;
    }

    public static void TestButtonDelegationWorks()
    {
        //THIS IS TEMP
        Debug.Log("Works!");
    }



    public void SpawnLevelStatue()
    {
        if(currentClusterSO.clusterPrefabToSummon)
        {
            currentLevelAnimalStatue = Instantiate(currentClusterSO.clusterPrefabToSummon, inLevelParent);
            currentLevelGeneralStatueAnimator = currentLevelAnimalStatue.statueAnimator;

            isAnimalLevel = currentLevelAnimalStatue != null;

            currentLevelGeneralStatueAnimator.SetTrigger(ANIM_SET_RIVE + currentIndexInCluster);

        }
        else
        {
            isAnimalLevel = false;
        }
    }
    public void AdvanceLevelStatue()
    {
        if(ReturnIsLastLevelInCluster() && isAnimalLevel)
        {
            // release animal
            animalsManager.ReleaseAnimal(currentLevelAnimalStatue, inLevelParent);
        }
        else
        {
            // advance animal statue

            if(currentLevelGeneralStatueAnimator)
            {
                currentLevelGeneralStatueAnimator.SetTrigger(ANIM_CLEAR_RIVE + currentIndexInCluster);
            }
        }
    }

    public string[] ReturnStatueName()
    {
        string[] texts = new string[1];

        if (ReturnIsLastLevelInCluster() && isAnimalLevel)
        {
            string animalname = currentLevelAnimalStatue.animal.ToString();
            texts[0] = animalname + " released!";
        }
        else
        {
            texts[0] = "Corruption cleansed!";
        }

        return texts;
    }

    public int ReturnNumOfLevelsInCluster()
    {
        return currentClusterSO.clusterLevels.Length;
    }
    public bool ReturnIsLastLevelInCluster()
    {
        return (currentIndexInCluster + 1 == currentClusterSO.clusterLevels.Length);
    }
    public int ReturnCurrentIndexInCluster()
    {
        return currentIndexInCluster;
    }
    //public int ReturnLastLevelIndexReached()
    //{
    //    return currentMaxLevelReached;
    //}

    public LevelSO ReturnCurrentLevelSO()
    {
        return currentClusterSO.clusterLevels[currentIndexInCluster];
    }
    public void BroadcastWinLevelActions()
    {
        //currentMaxLevelReached++;

        WinLevelActions?.Invoke();

        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "World1", currentLevel.levelNumInZone.ToString());

        GameAnalytics.NewErrorEvent(GAErrorSeverity.Critical, "I am testing GA");

    }
    //public void BroadcastLoseLevelActions()
    //{
    //    LoseLevelActions?.Invoke();
    //}

    public void AdvanceGiveLootFromManager()
    {
        //sequencer?
        //lootManager.ManageLootReward(currentClusterSO); //go over this with Lior
    }
    public void AdvanceLootChestAnimation() //go over this with Lior
    {
        //sequencer?
        //StartCoroutine(summonedChest.AfterGiveLoot());
    }


    public IEnumerator OnLevelExitResetSystem()
    {
        UIManager.IS_DURING_TRANSITION = true;

        //ClearLevelActions();
        gameRing.ClearActions();

        bool isAtStartOfCluster = currentIndexInCluster == 0 ? true : false;

        RestartClusterData();
        //currentMaxLevelReached = currentCluster.clusterLevels[0].levelNumInZone;

        yield return StartCoroutine(UIManager.instance.DisplayLevelCluster(true));

        yield return StartCoroutine(mapLogic.CameraTransitionClusterStart(isAtStartOfCluster, true)); // move to start of cluster


        //FADE
        StartCoroutine(UIManager.instance.FadeInCurtainswindow(true));
        yield return new WaitUntil(() => !UIManager.IS_DURING_CURTAINS);


        foreach (var ring in mapLogic.publicInstantiatedRings)
        {
            Destroy(ring.gameObject);
            yield return new WaitForEndOfFrame();
        }
        foreach (var treePiece in mapLogic.publicInstantiatedLastPieces)
        {
            Destroy(treePiece.gameObject);
            yield return new WaitForEndOfFrame();
        }

        mapLogic.ResetMapData();


        for (int i = 0; i < inLevelParent.childCount; i++)
        {
            Destroy(inLevelParent.GetChild(i).gameObject);
        }

        gameClip.DestroyClipData();

        IS_IN_LEVEL = false;




        yield return StartCoroutine(mapLogic.InitMapLogic(currentClusterSO));
        //StartCoroutine(mapLogic.HideRingDarkOverlay(0));

        yield return new WaitForSeconds(delayRestartCluster);

        StartCoroutine(InitStartLevel(false));
    }

    public IEnumerator OnLevelExitResetLevel()
    {
        ClearLevelData();
        gameRing.ClearActions();

        Destroy(mapLogic.publicInstantiatedRings[currentIndexInCluster].gameObject);
        yield return new WaitForEndOfFrame();

        yield return StartCoroutine(mapLogic.SpawnSpecificRingInCluster(currentIndexInCluster));

        for (int i = 0; i < inLevelParent.childCount; i++)
        {
            Destroy(inLevelParent.GetChild(i).gameObject);
        }

        gameClip.DestroyClipData();

        StartCoroutine(InitStartLevel(true));
    }


    public IEnumerator OnLevelExitWin(bool isClusterEnd)
    {
        UIManager.IS_DURING_TRANSITION = true;

        yield return StartCoroutine(UIManager.instance.DisplayLevelCluster(true));

        gameRing.ClearActions();
        ClearLevelData();

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < inLevelParent.childCount; i++)
        {
            Destroy(inLevelParent.GetChild(i).gameObject);
        }

        gameClip.DestroyClipData();

        IS_IN_LEVEL = false;

        if (!isClusterEnd)
        {

            //yield return StartCoroutine(mapLogic.CameraTransitionNextLevel(currentIndexInCluster)); // move to next level automatically
            mapLogic.DataOnTransitionNextLevel();
            yield return new WaitForSeconds(2);

            StartCoroutine(InitStartLevel(false));
        }
    }

    //public void CallNextLevel()
    //{
    //    StartCoroutine(MoveToNextLevel());
    //}


    public bool LevelSetupData()
    {
        LevelMapCustomButton levelButton = null;

        mapLogic.publicInstantiatedRings[currentIndexInCluster].TryGetComponent<LevelMapCustomButton>(out levelButton);

        if(!levelButton)
        {
            Debug.Log("Each level should have a level map custom button script attached to it.");
            return false;
        }

        levelButton.AutomatiTransferLevel();




        return true;

        ////called from level actions events
        //if (ReturnIsLastLevelInCluster())
        //{
        //    nextLevel = null;
        //}
        //else
        //{
        //    nextLevel = currentClusterSO.clusterLevels[currentIndexInCluster];
        //}
    }

    private void RestartClusterData()
    {
        currentIndexInCluster = 0;
    }

    public IEnumerator InitStartLevel(bool restart)
    {
        if (LevelSetupData())
        {
            if(!restart)
            {
                UIManager.IS_DURING_TRANSITION = true;

                StartCoroutine(AnimateLevelElements(true));
            }

            yield return new WaitForEndOfFrame();
            SetLevel();
        }
    }



    private IEnumerator InitClusterTransfer()
    {
        ClearLevelData();
        gameRing.ClearActions();

        bool isAtStartOfCluster = currentIndexInCluster == 0 ? true : false;

        RestartClusterData();

        yield return StartCoroutine(UIManager.instance.DisplayLevelCluster(true));

        yield return new WaitForSeconds(0.8f);

        yield return StartCoroutine(mapLogic.CameraTransitionClusterStart(isAtStartOfCluster, false)); // move to start of cluster

        for (int i = 0; i < inLevelParent.childCount; i++)
        {
            Destroy(inLevelParent.GetChild(i).gameObject);
        }

        gameClip.DestroyClipData();

        IS_IN_LEVEL = false;




        UIManager.instance.DisplayInLevelWinWindow();
        lootManager.PublicGiveLoot();


        currentClusterSO = allClusters[currentMaxClusterReached];

        currentMaxClusterReached++; ;

        if (currentMaxClusterReached > allClusters.Length)
        {
            Debug.LogError("No next cluster!");
            // thank you for playing the demo screen?
            yield break;
        }

    }


    public void OnLoadData()
    {
        currentIndexInCluster = 0;
        currentClusterSO = allClusters[SavedData.instance.currentClusterIDReached - 1]; // we do -1 since clusters start at 1, not at 0
        currentMaxClusterReached = currentClusterSO.clusterID;


        StartCoroutine(mapLogic.InitMapLogic(currentClusterSO));
    }

    /**/
    // GETTERS!
    /**/
    public List<OwnedAnimalDataSet> GetUnlockedAnimals => animalsManager.GetUnlockedAnimals();
    public ClusterSO currentCluster => currentClusterSO;
    public int publicMaxClusterReached => currentMaxClusterReached;
    //public Player publicPlayer => player;
    //public MapLogic publicMapLogic => mapLogic;
    public bool IsAnimalAlreadyInAlbum(AnimalsInGame animal) => animalsManager.CheckAnimalAlreadyInAlbum(animal);


    /**/
    // general methods area - methods that can be dropped and used in any class - mostly inspector things for now
    /**/

    //public GameObject preafabToInstantiateInspector;

    //[ContextMenu("Instantiate prefab under object")]
    //public void InstantiatePrefabUnderObject ()
    //{
    //    GameObject go = PrefabUtility.InstantiatePrefab(preafabToInstantiateInspector, transform) as GameObject;
    //    go.GetComponent<Image>().sprite = GetComponent<Image>().sprite;
    //}

    //[ContextMenu("Destroy self and move child 1 up in herarchy")]
    //public void DestroySelfAndMoveChildUpInHerarchy()
    //{
    //    transform.GetChild(0).SetParent(transform.parent);
    //    DestroyImmediate(gameObject);
    //}
}


