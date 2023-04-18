using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    const string ANIM_SET_RIVE = "Set Rive ";
    const string ANIM_CLEAR_RIVE = "Clear Rive ";

    public static GameManager instance; //TEMP - LEARN DEPENDENCY INJECTION

    [Header("Level setup Data")]
    [SerializeField] private ClusterSO currentClusterSO;
    [SerializeField] private int currentIndexInCluster;

   [Header("In game Data")]
    public static Ring gameRing;
    public static ClipManager gameClip;
    public static LevelSO currentLevel;
    public static InLevelUserControls gameControls;
    public LevelSO tempcurrentlevel; //temp!
    public LevelSO nextLevel;
    public ChestLogic summonedChest; //temp?
    public ChestBarLogic chestBarLogic; //temp?

    [SerializeField] private AnimalStatueData currentLevelAnimalStatue; //temp?
    [SerializeField] private Animator currentLevelGeneralStatueAnimator;//temp?
    [SerializeField] private bool isAnimalLevel;//temp?

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

    [SerializeField] private GameObject[] gameRingsPrefabs;
    [SerializeField] private GameObject[] gameRingsSlicePrefabs;
    [SerializeField] private GameObject[] gameRingsClipPrefabs;
    [SerializeField] private GameObject[] gameRingsUserControlsPrefabs;

    [Header("Inspector actions and Data")]
    public ClusterSO[] allClusters;


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

        LeanTween.init(5000);
    }

    private void Update()
    {
        //if(gameClip)
        //{
        //    Debug.Log("game clip is summoned");
        //}
    }

    //called from button click
    public void SetLevel()
    {
        //first clean all subscribes if there are any.
        endLevelActions?.Invoke();

        //currentLevel = level; //choose level here

        //this is the only place in code where we add delegates to the actions of before, during and after ring.
        // this will not actually invoke the unity event functions - it will add it's invoked functions to the action in the order they are created.
        BeforeRingActions += () => currentLevel.beforeRingSpawnActions.Invoke();
        BeforeRingActions += SpawnLevelBG;

        RingActions += BuildLevel;
        RingActions += () => currentLevel.ringSpawnActions.Invoke();

        AfterRingActions += () => currentLevel.afterRingSpawnActions.Invoke();

        SymbolAndColorCollector.instance.ResetData();

        StartCoroutine(StartLevel());
    }

    private IEnumerator StartLevel()
    {
        isAnimalLevel = false; //maybe have a reset function

        yield return new WaitUntil(() => !UIManager.IS_DURING_TRANSITION);

        //Before Ring
        BeforeRingActions?.Invoke();

        //Ring
        RingActions?.Invoke();

        //After Ring
        AfterRingActions?.Invoke();

       
        AddToEndlevelActions(DestroyOnLevelExit);
        AddToEndlevelActions(gameRing.ClearActions);

        // every level launch, no matter what, we launch the in level UI
        // we do this BEFORE setting the win level and end level actions
        UIManager.instance.DisplayInLevelUI();

        
        // actions after gameplay, on winning the level
        WinLevelActions += AdvanceLevelStatue;
        WinLevelActions += UIManager.instance.DisplayInLevelWinWindow;

        if(currentClusterSO.isChestCluster)
        {
            chestBarLogic.gameObject.SetActive(true);
            WinLevelActions += chestBarLogic.AddToChestBar;
        }
        else
        {
            chestBarLogic.gameObject.SetActive(false);
        }
        // actions after gameplay, on losing the level
        //LoseLevelActions += UIManager.instance.DisplayInLevelRingHasNonMatchingMessage;
    }

    private void BuildLevel()
    {
        //// All of these should be part of the list "Before ring spawn actions" or "after...."???? (either? or? none?)

        // Spawn ring by type from level
        gameRing = Instantiate(gameRingsPrefabs[(int)currentLevel.ringType], inLevelParent).GetComponent<Ring>();
        if (!gameRing)
        {
            Debug.LogError("No ring!");
        }
        gameRing.InitRing();

        // Spawn clip by type from level (or a general clip)
        gameClip = Instantiate(gameRingsClipPrefabs[(int)currentLevel.ringType], inLevelParent).GetComponent<ClipManager>();
        if (!gameClip)
        {
            Debug.LogError("No Clip!");
        }

        // Init clip - spawn according to rules
        gameClip.InitClipManager();


        //Spawn User Controls For Level
        gameControls = Instantiate(gameRingsUserControlsPrefabs[(int)currentLevel.ringType], inLevelParent).GetComponent<InLevelUserControls>();
        if (!gameControls)
        {
            Debug.LogError("No User Controls!");
        }

        //local Init User Controls For Level - we don't do enoguh to merit own Init function
        gameControls.InitUserControls(gameRing, gameClip);

        //Init slices that pass information to cells (run 2)
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


    [ContextMenu("Restart")]
    public void CallRestartLevel()
    {
        StartCoroutine(RestartLevel());
    }
    private IEnumerator RestartLevel()
    {
        yield return new WaitUntil(() => !UIManager.IS_DURING_TRANSITION);

        for (int k = 0; k < 1; k++)
        {
            DestroyOnLevelExit();

            yield return new WaitForEndOfFrame();

            SetLevel();
        }
    }

    public void ClickOnLevelIconMapSetData(LevelSO levelSO, ClusterSO clusterSO, int inedxInCluster)
    {
        currentLevel = levelSO;
        tempcurrentlevel = levelSO; // this is temp

        currentClusterSO = clusterSO;
        currentIndexInCluster = inedxInCluster;
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
        yield return new WaitUntil(() => !UIManager.IS_DURING_TRANSITION);

        DestroyOnLevelExit();

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
        StartCoroutine(summonedChest.AfterGiveLoot());
    }

    public void DestroyOnLevelExit()
    {
        lootManager.DestroyAllLootChildren();

        for (int i = 0; i < inLevelParent.childCount; i++)
        {
            Destroy(inLevelParent.GetChild(i).gameObject);
        }
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


