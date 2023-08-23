using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public GameObject originObject;
    public GameObject targetObject;
    public float heightOffset;
    public float moveTime;
    public float waitBeforeReset;
    public GameObject currentMoveObject;

    [ContextMenu("Test")]
    private void TestNow()
    {
        Vector3 targetPos = new Vector3(originObject.transform.position.x, originObject.transform.position.y + heightOffset, originObject.transform.position.z);
        currentMoveObject = Instantiate(prefabToSpawn, targetPos, prefabToSpawn.transform.rotation);

        test3(false);
    }


    void test3(bool isBack)
    {
        StartCoroutine(TestNow2(isBack));

    }
    private IEnumerator TestNow2(bool isBack)
    {
        if (isBack)
        {
            Vector3 targetPos = new Vector3(originObject.transform.position.x, originObject.transform.position.y + heightOffset, originObject.transform.position.z);

            yield return new WaitForSeconds(waitBeforeReset);

            currentMoveObject.transform.position = targetPos;
        }
        else
        {
            Vector3 targetPos = new Vector3(targetObject.transform.position.x, targetObject.transform.position.y + heightOffset, targetObject.transform.position.z);

            LeanTween.move(currentMoveObject, targetPos, moveTime);

            yield return new WaitForSeconds(moveTime);
        }


        test3(!isBack);
    }
}
