using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLogic : MonoBehaviour
{
    public static LevelSO currentLevel;
    public static Ring currentRing;

    //[Header("Refrences")]
    private ClusterSO currentCluster;

    [Header("Instantiate Data")]
    [SerializeField] private GameObject mapRingPrefab;
    [SerializeField] private Transform ringsParent;
    [SerializeField] private float ringsOffset;

    [SerializeField] private List<Ring> instantiatedRings;

    [Header("Scroll logic")]
    [SerializeField] private bool allowScroll;
    [SerializeField] private float dragSpeed = 100;
    [SerializeField] private float dragSpeedClampSpeed = 120;
    [SerializeField] private float elacitySpeed = 100;
    [SerializeField] private Transform levelCameraParent = null;
    [SerializeField] private float waitMoveNextLevelTime = 2;
    [SerializeField] private float moveNextLevelTime = 2;


    private Vector3 translation = Vector3.zero;
    private Vector2 deltaPos = Vector2.zero;
    private Vector3 endPos = Vector3.zero;

    public void InitMapLogic(ClusterSO cluster)
    {
        currentCluster = cluster;

        Vector3 pos = Vector3.zero;

        for (int i = 0; i < currentCluster.clusterLevels.Length; i++)
        {
            GameObject go = Instantiate(mapRingPrefab, pos, Quaternion.identity, ringsParent);
            go.transform.localPosition = Vector3.zero;
            pos.z = i * ringsOffset;
            go.transform.localPosition = pos;

            go.TryGetComponent<Ring>(out currentRing);

            if (!currentRing)
            {
                Debug.LogError("No ring");
                return;
            }
            currentRing.InitRing();
            currentRing.levelStartCollider.enabled = false;

            currentLevel = currentCluster.clusterLevels[i];
            currentLevel.afterRingSpawnActions?.Invoke();

            LevelMapCustomButton customButton;
            go.TryGetComponent<LevelMapCustomButton>(out customButton);
            if (customButton == null)
            {
                Debug.Log("No level button!");
                return;
            }

            customButton.connectedLevelSO = currentLevel;
            customButton.connectedCluster = currentCluster;
            customButton.connectedRing = currentRing;
            customButton.indexInCluster = i;

            instantiatedRings.Add(currentRing);
        }


        ActivateSpecificRingCollider(0); //TEMP
    }

    public IEnumerator SpawnSpecificRingInCluster(int levelIndex)
    {
        instantiatedRings.RemoveAt(levelIndex);
        yield return new WaitForEndOfFrame();

        Vector3 pos = Vector3.zero;

        GameObject go = Instantiate(mapRingPrefab, pos, Quaternion.identity, ringsParent);
        go.transform.localPosition = Vector3.zero;
        pos.z = levelIndex * ringsOffset;
        go.transform.localPosition = pos;

        go.TryGetComponent<Ring>(out currentRing);

        if (!currentRing)
        {
            Debug.LogError("No ring");
            yield break;
        }
        currentRing.InitRing();

        currentLevel = currentCluster.clusterLevels[levelIndex];
        currentLevel.afterRingSpawnActions?.Invoke();

        LevelMapCustomButton customButton;
        go.TryGetComponent<LevelMapCustomButton>(out customButton);
        if (customButton == null)
        {
            Debug.Log("No level button!");
            yield break;
        }

        customButton.connectedLevelSO = currentLevel;
        customButton.connectedCluster = currentCluster;
        customButton.connectedRing = currentRing;
        customButton.indexInCluster = levelIndex;

        instantiatedRings.Insert(levelIndex, currentRing);

        go.transform.SetSiblingIndex(levelIndex);
    }

    private void ActivateSpecificRingCollider(int index)
    {
        instantiatedRings[index].levelStartCollider.enabled = true;
    }

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

        float clampZ = Mathf.Clamp(levelCameraParent.transform.position.z, ringsOffset * 2, ringsOffset * currentCluster.clusterLevels.Length);
        // we do *2 in the minimum, since that is the position where we see the first three rings.

        levelCameraParent.transform.position = new Vector3(levelCameraParent.transform.position.x, levelCameraParent.transform.position.y, clampZ);
    }

    public void FixCamPosStartLevel(int currentIndexInCluster)
    {
        float ZPos = ringsOffset * (currentIndexInCluster + 2); // we do +2 here since we see 3 rings in the starts, so it's index 0, 1, 2.
                                                                               // from this point on, every time we advance in the cluster, we will advance by ring offset (9.5 for now)
        LeanTween.move(levelCameraParent.gameObject, new Vector3(levelCameraParent.position.x, levelCameraParent.position.y, ZPos), 1.3f);
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
        endPos = Vector3.zero; // we reset the map move data before manually moving.

        yield return new WaitForSeconds(waitMoveNextLevelTime);
        if(currentIndexInCluster < currentCluster.clusterLevels.Length -2 ) //we always want to make sure we don't move if we need the current + next ring. this keeps us in bounds
        {
            float ZPos = levelCameraParent.transform.position.z + ringsOffset;

            LeanTween.move(levelCameraParent.gameObject, new Vector3(levelCameraParent.position.x, levelCameraParent.position.y, ZPos), moveNextLevelTime).setEase(LeanTweenType.easeInOutSine);
        }

        GameManager.IS_IN_LEVEL = false;

        if (currentIndexInCluster + 1 != currentCluster.clusterLevels.Length) //if next level in cluster is not out of bounds
        {
            yield return new WaitForSeconds(moveNextLevelTime);
            ActivateSpecificRingCollider(currentIndexInCluster + 1);
        }
    }
}
