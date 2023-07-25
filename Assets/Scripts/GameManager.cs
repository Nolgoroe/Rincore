using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GameAnalyticsSDK;
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

    //[SerializeField] private Transform levelCameraParent = null;
    [SerializeField] private Transform levelDecksParent = null;
    [SerializeField] private Animator cameraAnimatorController;
    [SerializeField] private Animator clipAnimatorController;
    [SerializeField] private Animator potionDeckAnimatorController;


    [Header("Level Animation Data")]
    [SerializeField] private float delayAfterLevelExit = 1.2f;
    [SerializeField] private float delayClipAppear = 0.4f;
    [SerializeField] private float timeClipEnter = 0.4f;
    [SerializeField] private float delayClipHide = 0.4f;

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

    [SerializeField] private GameObject[] gameRingsPrefabs;
    [SerializeField] private GameObject[] gameRingsSlicePrefabs;
    [SerializeField] private GameObject[] gameRingsClipPrefabs;
    [SerializeField] private GameObject[] gameRingsUserControlsPrefabs;

    [Header("Inspector actions and Data")]
    public ClusterSO[] allClusters;

    public int testFirebase;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        // TO DO
        // if we use a scene transfer system then  make sure the Instance is deleted if we transfer a scene
        // consider changing Sigleton access to something else.

        //currentLevel = tempcurrentlevel;

        //SetLevel(currentLevel);

        GameAnalytics.Initialize();

        gameClip = clipManager;
        LeanTween.init(5000);

        mapLogic.InitMapLogic(currentClusterSO);


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
        GameAnalytics.NewProgressionEvent (GAProgressionStatus.Start, "World1", "Level: " + currentLevel.levelNumInZone.ToString());

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

       
        AddToEndlevelActions(() => StartCoroutine(OnLevelExit()));
        AddToEndlevelActions(gameRing.ClearActions);

        
        // actions after gameplay, on winning the level
        //WinLevelActions += AdvanceLevelStatue;
        WinLevelActions += UIManager.instance.DisplayInLevelWinWindow;

        SymbolAndColorCollector.instance.DoTotalCheck(); // we do this in case there are preplaced tiles that need to be counted.

        //if(currentClusterSO.isChestCluster)
        //{
        //    //chestBarLogic.gameObject.SetActive(true);
        //    //WinLevelActions += chestBarLogic.AddToChestBar;
        //}
        //else
        //{
        //    //chestBarLogic.gameObject.SetActive(false);
        //}
        // actions after gameplay, on losing the level
        //LoseLevelActions += UIManager.instance.DisplayInLevelRingHasNonMatchingMessage;
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
            gameRing.levelStartCollider.enabled = false;

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
            UIManager.IS_DURING_TRANSITION = true;

            mapLogic.FixCamPosStartLevel(currentIndexInCluster);
            cameraAnimatorController.SetTrigger("Camera In Level");

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
            UIManager.IS_DURING_TRANSITION = true;

            mapLogic.ToggleRings(gameRing, inLevel);

            clipAnimatorController.SetTrigger("Clip Out Level");
            potionDeckAnimatorController.SetTrigger("Potion Out Level");

            yield return new WaitForSeconds(delayClipHide);

            StartCoroutine(mapLogic.FixCamPosEndLevel());
            cameraAnimatorController.SetTrigger("Camera Out Level");
        }


        if (!inLevel)
        {
            levelDecksParent.gameObject.SetActive(false);
        }
    }
    private void ClearLevelActions()// this must be added last to "endLevelActions"
    {
        BeforeRingActions = null;
        RingActions = null;
        AfterRingActions = null;
        WinLevelActions = null;
        //LoseLevelActions = null;
        endLevelActions = null;
    }

    public IEnumerator InitiateDestrucionOfLevel()
    {
        yield return new WaitUntil(() => !UIManager.IS_DURING_TRANSITION);
        Debug.Log("Initiating destruction");
        endLevelActions?.Invoke();
    }

    // This function makes sure that we have "ClearLevelActions" set as the last action to be made
    private void AddToEndlevelActions(System.Action actionToAdd)
    {
        endLevelActions -= ClearLevelActions;// this has to be the last added func

        endLevelActions += actionToAdd;

        endLevelActions += ClearLevelActions;// this has to be the last added func

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
           yield return StartCoroutine(OnLevelExit());

           SetLevel();
        }
    }

    public void ClickOnLevelIconMapSetData(LevelSO levelSO, ClusterSO clusterSO, Ring ring, int inedxInCluster)
    {
        currentLevel = levelSO;
        
        gameRing = ring;

        currentClusterSO = clusterSO;
        currentIndexInCluster = inedxInCluster;
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

    public void CallNextLevel()
    {
        StartCoroutine(MoveToNextLevel());
    }

    private IEnumerator MoveToNextLevel()
    {
        //yield return new WaitUntil(() => !UIManager.IS_DURING_TRANSITION);

        StartCoroutine(OnLevelExit());

        currentLevel = nextLevel;
        currentIndexInCluster++;
        LevelSetupData();
        yield return new WaitForEndOfFrame();

        SetLevel();
    }

    public void LevelSetupData()
    {
        //called from level actions events
        if (ReturnIsLastLevelInCluster())
        {
            nextLevel = null;
        }
        else
        {
            nextLevel = currentClusterSO.clusterLevels[currentIndexInCluster + 1];
        }
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

    public void BroadcastWinLevelActions()
    {
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
        lootManager.ManageLootReward(currentClusterSO); //go over this with Lior
    }
    public void AdvanceLootChestAnimation() //go over this with Lior
    {
        //sequencer?
        //StartCoroutine(summonedChest.AfterGiveLoot());
    }

    public IEnumerator OnLevelExitWin()
    {
        gameRing.ClearActions();
        ClearLevelActions();

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < inLevelParent.childCount; i++)
        {
            Destroy(inLevelParent.GetChild(i).gameObject);
        }

        gameClip.DestroyClipData();

        StartCoroutine(mapLogic.CameraTransitionNextLevel(currentIndexInCluster)); // move to next level automatically

        IS_IN_LEVEL = false;
    }

    public IEnumerator OnLevelExit()
    {
        ClearLevelActions();
        gameRing.ClearActions();

        yield return new WaitForEndOfFrame();
        Destroy(gameRing.gameObject);

        yield return new WaitForEndOfFrame();
        yield return StartCoroutine(mapLogic.SpawnSpecificRingInCluster(currentIndexInCluster));
        //lootManager.DestroyAllLootChildren();

        for (int i = 0; i < inLevelParent.childCount; i++)
        {
            Destroy(inLevelParent.GetChild(i).gameObject);
        }

        gameClip.DestroyClipData();

        IS_IN_LEVEL = false;

        //yield return new WaitForSeconds(0.1f);
        //for (int i = 0; i < gameRing.ringCells.Length; i++)
        //{
        //    if (gameRing.ringCells[i].heldTile)
        //    {
        //        Destroy(gameRing.ringCells[i].heldTile.gameObject);
        //    }
        //}

        //for (int i = 0; i < gameRing.ringCells.Length; i++)
        //{
        //        gameRing.ringCells[i].ResetToDefault();
        //}
    }

    /**/
    // GETTERS!
    /**/
    public List<IngredientPlusMainTypeCombo> GetPlayerCombos => player.returnOwnedIngredientsByType;
    public Dictionary<Ingredients, LootEntry> GetIngredientDict => player.returnownedIngredients;
    public List<OwnedAnimalDataSet> GetUnlockedAnimals => animalsManager.GetUnlockedAnimals();
    public ClusterSO currentCluster => currentClusterSO;
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


