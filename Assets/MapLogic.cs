using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLogic : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField] private ClusterSO currentCluster;

    [Header("Instantiate Data")]
    [SerializeField] private GameObject mapRingPrefab;
    [SerializeField] private Transform ringsParent;
    [SerializeField] private float ringsOffset;
    public static LevelSO currentLevel;
    public static Ring currentRing;

    [Header("Scroll logic")]
    [SerializeField] private float dragSpeed = 100;
    [SerializeField] private float dragSpeedClampSpeed = 120;
    [SerializeField] private float elacitySpeed = 100;
    [SerializeField] private Camera camera = null;

    private Vector3 translation = Vector3.zero;
    private Vector2 deltaPos = Vector2.zero;
    private Vector3 endPos = Vector3.zero;

    private void Start()
    {
        Vector3 pos = Vector3.zero;
        
        for (int i = 0; i < currentCluster.clusterLevels.Length; i++)

        {
            GameObject go = Instantiate(mapRingPrefab, pos, Quaternion.identity, ringsParent);
            go.transform.localPosition = Vector3.zero;
            pos.z = i * ringsOffset;
            go.transform.localPosition = pos;

            go.TryGetComponent<Ring>(out currentRing);

            if(!currentRing)
            {
                Debug.LogError("No ring");
                return;
            }

            currentLevel = currentCluster.clusterLevels[i];
            currentLevel.afterRingSpawnActions?.Invoke();

            LevelMapCustomButton customButton;
            go.TryGetComponent<LevelMapCustomButton>(out customButton);
            if(customButton == null)
            {
                Debug.Log("No level button!");
                return;
            }

            customButton.connectedLevelSO = currentLevel;
            customButton.connectedCluster = currentCluster;
            customButton.connectedRing = currentRing;
        }
    }

    private void Update()
    {
        if (UIManager.IS_USING_UI) return;

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

                camera.transform.position = Vector3.Lerp(camera.transform.position, camera.transform.position + translation, dragSpeed);
            }

            if(touch.phase == TouchPhase.Stationary)
            {
                translation = Vector3.zero;
            }

            if (touch.phase == TouchPhase.Ended)
            {
                endPos = camera.transform.position + translation;
            }
        }
        else
        {
            if (endPos != Vector3.zero)
            {
                camera.transform.position = Vector3.Lerp(camera.transform.position, endPos, elacitySpeed * Time.deltaTime);
            }
        }

        float clampZ = Mathf.Clamp(camera.transform.position.z, 0, ringsOffset * (currentCluster.clusterLevels.Length - 3)); 
        // we do - 3 since we always see 3 rings on screen.. so if we go over it it's the amount over minus the original 3

        camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, clampZ);
    }
}
