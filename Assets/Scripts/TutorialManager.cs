using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class TutorialManager : MonoBehaviour
{
    [Header("general refs - temp?")]
    public GameObject dealObject;


    [Header("Needed refs")]
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private RectTransform textParent;
    [SerializeField] private TMP_Text tutorialText;



    [Header("Animation Data")]
    [SerializeField] private float heightOffset;
    [SerializeField] private float moveTime;
    [SerializeField] private float waitBeforeReset;

    [Header("Live data")]
    [SerializeField] private GameObject originObject;
    [SerializeField] private GameObject targetObject;
    [SerializeField] private GameObject currentMoveObject;

    private void StartTutorial()
    {
        Vector3 targetPos = new Vector3(originObject.transform.position.x, originObject.transform.position.y + heightOffset, originObject.transform.position.z);
        currentMoveObject = Instantiate(prefabToSpawn, targetPos, prefabToSpawn.transform.rotation);

        MiddleManCoroutine(false);
    }

    public void SetCurrenTutorialData(TutorialSO tutorialSO, int currentIndex)
    {
        if (!tutorialSO) return;

        switch (tutorialSO.tutorialSteps[currentIndex].tutorialType)
        {
            case TutorialType.MoveClipToCell:
                SetMoveFromClipToCellData(tutorialSO, currentIndex);
                break;
            case TutorialType.MoveCellToCell:
                break;
            case TutorialType.UseDeal:
                break;
            case TutorialType.UsePotions:
                break;
            default:
                break;
        }

        textParent.anchoredPosition = tutorialSO.tutorialSteps[currentIndex].textPosition;
        tutorialText.text = tutorialSO.tutorialSteps[currentIndex].tutorialText;
    }

    private void SetMoveFromClipToCellData(TutorialSO tutorialSO, int currentIndex)
    {
        int clipSlotIndex = tutorialSO.tutorialSteps[currentIndex].originalIndex;
        int ringCellIndex = tutorialSO.tutorialSteps[currentIndex].targetCellIndex;
        originObject = GameManager.gameClip.ReturnSlot(clipSlotIndex).gameObject;
        targetObject = GameManager.gameRing.ringCells[ringCellIndex].gameObject;
    }

    private void SetMoveFromCellToCellData(TutorialSO tutorialSO, int currentIndex)
    {
        int originRingCellIndex = tutorialSO.tutorialSteps[currentIndex].originalIndex;
        int targetRingCellIndex = tutorialSO.tutorialSteps[currentIndex].targetCellIndex;
        originObject = GameManager.gameRing.ringCells[originRingCellIndex].gameObject;
        targetObject = GameManager.gameRing.ringCells[targetRingCellIndex].gameObject;
    }

    private void SetDealTutorialData(TutorialSO tutorialSO, int currentIndex)
    {
        originObject = dealObject;
    }

    private void SetPotionTutorialData(TutorialSO tutorialSO, int currentIndex)
    {
        int index = tutorialSO.tutorialSteps[currentIndex].potionIndex;

        originObject = PowerupManager.instance.ReturnPotionPosition(index).gameObject;
    }

    private void MiddleManCoroutine(bool isBack)
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


        MiddleManCoroutine(!isBack);
    }
}
