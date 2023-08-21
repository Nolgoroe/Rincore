using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLogic : MonoBehaviour
{
    public static LevelSO currentLevel;
    public static Ring currentRing;

    private ClusterSO currentCluster;

    [Header("Instantiate Data")]
    [SerializeField] private GameObject[] mapRingPrefabs;
    [SerializeField] private GameObject[] lastPiecePrefabs;
    [SerializeField] private Transform ringsParent;
    [SerializeField] private float distanceBetweenRings;
    [SerializeField] private float startRingOffset;
    [SerializeField] private float nextClusterSummonOffset;
    [SerializeField] private float currentClusterSummonOffset;
    [SerializeField] private List<Ring> instantiatedRings;
    [SerializeField] private List<GameObject> summonedLastPieces;
    [SerializeField] private List<LevelPresetData> instantiatedRingsData;
    [SerializeField] private List<GameObject> allTreeVariants;


    private List<GameObject> currentTreeVariantsSummoned;


    [Header("Enter Level logic")]
    [SerializeField] private float waitBeforeUnlock;
    [SerializeField] private float cameraTrailOffset;
    [SerializeField] private float cameralevelOffset;
    [SerializeField] private float endRingsVisibleLimit = 3;

    [Header("Scroll logic")]
    [SerializeField] private bool allowScroll;
    [SerializeField] private float dragSpeed = 100;
    [SerializeField] private float dragSpeedClampSpeed = 120;
    [SerializeField] private float elacitySpeed = 100;
    [SerializeField] private GameObject levelCameraParent;

    [Header("Enter Level Animation")]
    [SerializeField] private float waitMoveNextLevelTime = 2;
    [SerializeField] private float waitMoveStartCluster = 2;

    [SerializeField] private float angleIntoLevelTime = 1.5f;
    [SerializeField] private float moveNextLevelTime = 2;
 
    [SerializeField] private float moveCameraInLevelTime = 1;
    [SerializeField] private float moveCameraOutLevelTime = 1;

    [SerializeField] private float originalClusterCameraParentPos;
    [SerializeField] private float distanceCameraFirstRing = 29;
    [SerializeField] private float currentLevelCameraParentZPos;


    [Header("Transition cluster Animation")]
    [SerializeField] private float delayBetweenRings;
    [SerializeField] private float delayAfterLastPiece;
    [SerializeField] private float moveZAmountIn;
    [SerializeField] private float moveZAmountOut;
    [SerializeField] private float ringMoveTime;
    [SerializeField] private float lastPiecesMoveTime;
    [SerializeField] private float waitTimeBeforePlayButton;

    private Vector3 translation = Vector3.zero;
    private Vector2 deltaPos = Vector2.zero;
    private Vector3 endPos = Vector3.zero;

    public IEnumerator InitMapLogic(ClusterSO cluster, bool doFade)
    {
        currentTreeVariantsSummoned = new List<GameObject>();
        currentTreeVariantsSummoned.AddRange(allTreeVariants);

        currentCluster = cluster;

        Vector3 pos = Vector3.zero;

        for (int i = 0; i < currentCluster.clusterLevels.Length; i++)
        {
            currentLevel = currentCluster.clusterLevels[i];

            GameObject go = Instantiate(mapRingPrefabs[(i% 2) + SwitchRingPrefabByType(currentLevel.ringType)], pos, Quaternion.identity, ringsParent);
            go.transform.localPosition = Vector3.zero;
            pos.z = (i * distanceBetweenRings) + startRingOffset + currentClusterSummonOffset;
            go.transform.localPosition = pos;

            go.TryGetComponent<Ring>(out currentRing);

            if (!currentRing)
            {
                Debug.LogError("No ring");
                yield break;
            }
            currentRing.InitRing();
            //currentRing.levelStartCollider.enabled = false;

            currentLevel.afterRingSpawnActions?.Invoke();

            LevelMapCustomButton customButton;
            go.TryGetComponent<LevelMapCustomButton>(out customButton);
            if (customButton == null)
            {
                Debug.Log("No level button!");
                yield break;
            }

            customButton.data.connectedLevelSO = currentLevel;
            customButton.data.connectedCluster = currentCluster;
            customButton.data.connectedRing = currentRing;
            customButton.data.indexInCluster = i;

            instantiatedRings.Add(currentRing);

            SummonTreeVariant(go.transform);
        }

        int tempnum = currentCluster.clusterLevels.Length;

        for (int i = 0; i < 3; i++)
        {
            GameObject go = Instantiate(lastPiecePrefabs[(i % 2)], pos, Quaternion.identity, ringsParent);

            go.transform.localPosition = Vector3.zero;
            pos.z = ((tempnum + i) * distanceBetweenRings) + startRingOffset + currentClusterSummonOffset;
            go.transform.localPosition = pos;

            summonedLastPieces.Add(go);
        }

        // initializes the main camera's position compared to first ring created
        // we subtract currentClusterSummonOffset since we want the camera to always be reset to it's original distance - which is where the rings will be after animations.
        originalClusterCameraParentPos = instantiatedRings[0].transform.localPosition.z + distanceCameraFirstRing - currentClusterSummonOffset; 

        //ToggleSpecificRingCollider(0, true); //TEMP


        if (doFade)
        {
            UIManager.instance.FadeInFadeWindow();

            yield return new WaitUntil(() => !UIManager.IS_DURING_FADE);
        }
    }

    private int SwitchRingPrefabByType(Ringtype ringType)
    {

        int ringIndexInPrefabs = 0;

        switch (ringType)
        {
            case Ringtype.ring8:
                ringIndexInPrefabs = 0;
                break;
            case Ringtype.ring12:
                ringIndexInPrefabs = 2;
                break;
            case Ringtype.NoType:
                break;
            default:
                break;
        }

        return ringIndexInPrefabs;
    }

    public IEnumerator SpawnSpecificRingInCluster(int levelIndex)
    {
        instantiatedRings.RemoveAt(levelIndex);
        yield return new WaitForEndOfFrame();

        Vector3 pos = Vector3.zero;

        GameObject go = Instantiate(mapRingPrefabs[levelIndex % mapRingPrefabs.Length], pos, Quaternion.identity, ringsParent);
        go.transform.localPosition = Vector3.zero;
        pos.z = (levelIndex * distanceBetweenRings) + startRingOffset;
        go.transform.localPosition = pos;

        go.TryGetComponent<Ring>(out currentRing);

        if (!currentRing)
        {
            Debug.LogError("No ring");
            yield break;
        }
        //currentRing.levelStartCollider.enabled = false; // so we can't immediatly click it as it spawns..

        currentRing.InitRing();

        GameManager.instance.SetRingmanually(currentRing);

        currentLevel = currentCluster.clusterLevels[levelIndex];
        currentLevel.afterRingSpawnActions?.Invoke();

        LevelMapCustomButton customButton;
        go.TryGetComponent<LevelMapCustomButton>(out customButton);
        if (customButton == null)
        {
            Debug.Log("No level button!");
            yield break;
        }

        customButton.data.connectedLevelSO = currentLevel;
        customButton.data.connectedCluster = currentCluster;
        customButton.data.connectedRing = currentRing;
        customButton.data.indexInCluster = levelIndex;

        instantiatedRings.Insert(levelIndex, currentRing);

        go.transform.SetSiblingIndex(levelIndex);
    }

    //public void ToggleSpecificRingCollider(int index, bool active)
    //{
    //    instantiatedRings[index].levelStartCollider.enabled = active;
    //}

    private void Update()
    {
        if (UIManager.IS_USING_UI || GameManager.IS_IN_LEVEL || !allowScroll) return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if(touch.phase == TouchPhase.Began)
            {
                endPos = Vector3.zero;
            }

            if (touch.phase == TouchPhase.Moved)
            {
                translation = Vector3.zero;

                deltaPos = touch.deltaPosition;

                deltaPos.y = Mathf.Clamp(deltaPos.y, -dragSpeedClampSpeed, dragSpeedClampSpeed);

                translation -= new Vector3(0, 0, deltaPos.y * dragSpeed * Time.deltaTime);

                levelCameraParent.transform.position = Vector3.Lerp(levelCameraParent.transform.position, levelCameraParent.transform.position + translation, dragSpeed);
            }

            if(touch.phase == TouchPhase.Stationary)
            {
                translation = Vector3.zero;
            }

            if (touch.phase == TouchPhase.Ended)
            {
                endPos = levelCameraParent.transform.position + translation;
            }
        }
        else
        {
            if (endPos != Vector3.zero)
            {
                Debug.Log("IN HERE");
                levelCameraParent.transform.position = Vector3.Lerp(levelCameraParent.transform.position, endPos, elacitySpeed * Time.deltaTime);
            }
        }

        float clampZ = Mathf.Clamp(levelCameraParent.transform.position.z, distanceBetweenRings * 2, distanceBetweenRings * currentCluster.clusterLevels.Length);
        // we do *2 in the minimum, since that is the position where we see the first three rings.

        levelCameraParent.transform.position = new Vector3(levelCameraParent.transform.position.x, levelCameraParent.transform.position.y, clampZ);
    }

    public void FixCamPosStartLevel(int currentIndexInCluster)
    {
        //this function sets the "IN LEVEL" camera's position so that we can clearly see the whole ring as we play
        currentLevelCameraParentZPos = levelCameraParent.transform.localPosition.z;

        float ZPos = cameraTrailOffset * (currentIndexInCluster + 1) + distanceBetweenRings + cameralevelOffset; // we do +2 here since we see 3 rings in the starts, so it's index 0, 1, 2.
                                                                               // from this point on, every time we advance in the cluster, we will advance by ring offset (9.5 for now)
        LeanTween.move(levelCameraParent.gameObject, new Vector3(levelCameraParent.transform.position.x, levelCameraParent.transform.position.y, ZPos), moveCameraInLevelTime);
    }
    public IEnumerator FixCamPosEndLevel()
    {
        //this function sets the map camera's position to it's original pos (for this level) so that we can continue animations from there

        LeanTween.move(levelCameraParent.gameObject, new Vector3(levelCameraParent.transform.position.x, levelCameraParent.transform.position.y, currentLevelCameraParentZPos), moveCameraOutLevelTime);

        yield return new WaitForSeconds(moveCameraOutLevelTime + 0.5f);


        UIManager.IS_DURING_TRANSITION = false;
    }

    public void ToggleRings(Ring currentRing, bool inLevel)
    {
        if(inLevel)
        {
            foreach (Ring ring in instantiatedRings)
            {
                if(ring != currentRing)
                {
                    ring.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            foreach (Ring ring in instantiatedRings)
            {
                ring.gameObject.SetActive(true);
            }
        }
    }

    public IEnumerator CameraTransitionNextLevel(int currentIndexInCluster)
    {

        //this function moves the camera to a "start point" from which we can activate the enter level animation


        endPos = Vector3.zero; // we reset the map move data before manually moving.

        yield return new WaitForSeconds(waitMoveNextLevelTime);
        if(currentIndexInCluster < currentCluster.clusterLevels.Length - endRingsVisibleLimit) //we always want to make sure we don't move if we need the current + next ring. this keeps us in bounds
        {
            float ZPos = currentLevelCameraParentZPos + distanceBetweenRings;

            LeanTween.move(levelCameraParent.gameObject, new Vector3(levelCameraParent.transform.position.x, levelCameraParent.transform.position.y, ZPos), moveNextLevelTime).setEase(LeanTweenType.easeInOutSine);
        }

        GameManager.IS_IN_LEVEL = false;

        UIManager.instance.ManualUpdateLevelFillBar(UIManager.instance.fillAmounts[UIManager.instance.fillIndex]); // TEMP!
        UIManager.instance.fillIndex++;

        yield return new WaitForSeconds(angleIntoLevelTime);
        //ToggleSpecificRingCollider(currentIndexInCluster, false);

    }

    public IEnumerator CameraTransitionClusterStart(bool isAtStart, bool hasLost)
    {
        //this function moves the camera to the cluster's "start point" from which we can activate the enter first level animation

        endPos = Vector3.zero; // we reset the map move data before manually moving.

        if(!isAtStart)
        {
            if(hasLost)
            {
                UIManager.instance.ManualResetLevelFillBar(); // TEMP!
                UIManager.instance.fillIndex = 0;
            }
            else
            {
                //last part of the fill bar happends when we go back to start of cluster
                UIManager.instance.ManualUpdateLevelFillBar(UIManager.instance.fillAmounts[UIManager.instance.fillIndex]); // TEMP!
                UIManager.instance.fillIndex++;
            }

            yield return new WaitForSeconds(waitMoveStartCluster);
        }

        float ZPos = originalClusterCameraParentPos;

        LeanTween.move(levelCameraParent.gameObject, new Vector3(levelCameraParent.transform.position.x, levelCameraParent.transform.position.y, ZPos), moveNextLevelTime).setEase(LeanTweenType.easeInOutSine);

        GameManager.IS_IN_LEVEL = false;

        yield return new WaitForSeconds(angleIntoLevelTime);
    }



    public void ResetMapData()
    {
        instantiatedRings.Clear();
        summonedLastPieces.Clear();
    }

    public void CallClusterTransfer(ClusterSO newCluster)
    {
        StartCoroutine(ClusterTransfer(newCluster));
    }

    private IEnumerator ClusterTransfer(ClusterSO newCluster)
    {
        yield return StartCoroutine(MoveParts(true)); // move current rings

        currentClusterSummonOffset = nextClusterSummonOffset;

        ResetMapData();

        yield return new WaitForEndOfFrame();

        StartCoroutine(InitMapLogic(newCluster, false)); // summon new rings with offset

        yield return new WaitForEndOfFrame();


        currentClusterSummonOffset = 0; // reset offset for next time

        yield return StartCoroutine(MoveParts(false)); // move new rings


        //ShowRingDarkOverlay();

        yield return new WaitForSeconds(waitTimeBeforePlayButton);
        StartCoroutine(GameManager.instance.InitStartLevel(false));

        //UIManager.instance.DisplayOverallMapUI();
        //UIManager.instance.ShowSpecificButton(UIManager.instance.publicPlayButton); // temp here!
    }

    private IEnumerator MoveParts(bool destroyAtEnd)
    {
        for (int i = 0; i < instantiatedRings.Count; i++)
        {
            GameObject go = instantiatedRings[i].gameObject;
            MoveAction(go, destroyAtEnd, ringMoveTime);

            yield return new WaitForSeconds(delayBetweenRings);
        }

        for (int i = 0; i < summonedLastPieces.Count; i++)
        {
            MoveAction(summonedLastPieces[i], destroyAtEnd, lastPiecesMoveTime);
        }

        yield return new WaitForSeconds(delayAfterLastPiece);
    }
    
    private void MoveAction(GameObject go, bool destroyAtEnd, float time)
    {
        if(destroyAtEnd)
        {
            LeanTween.move(go,
                new Vector3(go.transform.position.x,
                go.transform.position.y,
                go.transform.position.z + moveZAmountOut),
                time)
                .setEase(LeanTweenType.easeInCubic)
                .setOnComplete(() => AtEndMove(go, destroyAtEnd));

        }
        else
        {
            LeanTween.move(go,
                new Vector3(go.transform.position.x,
                go.transform.position.y,
                go.transform.position.z + moveZAmountIn),
                time)
                .setEase(LeanTweenType.easeInCubic)
                .setOnComplete(() => AtEndMove(go, destroyAtEnd));
        }
    }

    private void AtEndMove(GameObject ring, bool destroyAtEnd)
    {
        if (destroyAtEnd)
        {
            Destroy(ring.gameObject);
        }
    }

    private void SummonTreeVariant(Transform parent)
    {
        int index = Random.Range(0, currentTreeVariantsSummoned.Count);

        GameObject go = Instantiate(currentTreeVariantsSummoned[index], parent);

        currentTreeVariantsSummoned.RemoveAt(index);
    }

    //public IEnumerator HideRingDarkOverlay(int currentIndexInCluster)
    //{
    //    yield return new WaitForEndOfFrame();

    //    SpriteRenderer lockedRenderer = instantiatedRings[currentIndexInCluster].lockedDarkOverlay.GetComponent<SpriteRenderer>();

    //    if (currentIndexInCluster == 0)
    //    {
    //        lockedRenderer.color = new Color(lockedRenderer.color.r, lockedRenderer.color.g, lockedRenderer.color.b, 0);
    //        yield break;

    //    }


    //    yield return new WaitForSeconds(waitBeforeUnlock);

    //    LeanTween.value(lockedRenderer.gameObject, 0.55f, 0, 2).setEase(LeanTweenType.linear).setOnUpdate((float val) =>
    //    {
    //        Color newColor = lockedRenderer.color;
    //        newColor.a = val;
    //        lockedRenderer.color = newColor;
    //    });
    //}

    //public void ShowRingDarkOverlay()
    //{
    //    for (int i = 0; i < instantiatedRings.Count; i++)
    //    {
    //        if(i == 0 )
    //        {
    //            StartCoroutine(HideRingDarkOverlay(0));
    //            continue;
    //        }

    //        SpriteRenderer renderer = instantiatedRings[i].lockedDarkOverlay.GetComponent<SpriteRenderer>();

    //        renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0.55f);


    //        //LeanTween.value(renderer.gameObject, 0, 0.55f, 2).setEase(LeanTweenType.linear).setOnUpdate((float val) =>
    //        //{
    //        //    Color newColor = renderer.color;
    //        //    newColor.a = val;
    //        //    renderer.color = newColor;
    //        //});
    //    }

    //}










    public void OnLoadData()
    {
        FixCamPosStartLevel(0);
    }














    /**/
    // GETTERS!
    /**/

    public List<Ring> publicInstantiatedRings => instantiatedRings;
}
