using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    [Header("general refs - temp?")]
    [SerializeField] private BasicCustomButton dealObject;
    [SerializeField] private Camera secondCam;
    [SerializeField] private Image maskImage;
    [SerializeField] private int camDepth;

    [Header("Needed refs")]
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private RectTransform textParent;
    [SerializeField] private TMP_Text tutorialText;


    [Header("Animation Data")]
    [SerializeField] private float heightOffset;
    [SerializeField] private float moveTime;
    [SerializeField] private float waitBeforeReset;
    [SerializeField] private float waitBeforeResetTap;
    [SerializeField] private float waitAfterTapDone;
    [SerializeField] private float waitTimeBeforeFlip;

    [Header("Live data")]
    public static bool IS_DURING_TUTORIAL;
    [SerializeField] private Tutoriable originObject;
    [SerializeField] private Tutoriable targetObject;
    [SerializeField] private GameObject currentMoveObject;
    [SerializeField] private Animator currentMoveObjectAnim;
    [SerializeField] private TutorialSO currentTutorial;
    [SerializeField] private int currentTutorialStepIndex;
    [SerializeField] private List<Tutoriable> activeHighlights;

    Coroutine coroutin;
    private void Awake()
    {
        instance = this;
    }

    public IEnumerator AdvanceTutorialStep()
    {
        currentTutorialStepIndex++;

        if(currentTutorial.tutorialSteps.Length <= currentTutorialStepIndex)
        {
            StartCoroutine(FinishTutorial());
        }
        else
        { 
            StartCoroutine(RemoveCurrentHighlights());
            Destroy(currentMoveObject.gameObject);

            yield return new WaitForEndOfFrame();

            SetCurrenTutorialStepData(currentTutorial, currentTutorialStepIndex);
        }

    }

    public IEnumerator FinishTutorial()
    {
        if(coroutin != null)
        {
            StopCoroutine(coroutin);
        }

        StartCoroutine(RemoveCurrentHighlights());
        UnLockAll();
        ToggleAllTutorialParts(false);

        if(currentMoveObject.gameObject)
        {
            LeanTween.cancel(currentMoveObject.gameObject);
            Destroy(currentMoveObject.gameObject);
        }

        yield return new WaitForEndOfFrame();

        IS_DURING_TUTORIAL = false;

        CheckManuallyLock();

        yield return new WaitForSeconds(waitTimeBeforeFlip);

        PowerupManager.instance.CallCheckNoPotions();

    }
    private void CheckManuallyLock()
    {
        if (!targetObject) return;

        CellBase cell = null;

        targetObject.TryGetComponent<CellBase>(out cell);

        if (cell && (cell.leftSlice.isLock || cell.rightSlice.isLock ))
        {
            cell.CheckConnections();
        }
    }

    public void SetCurrenTutorialStepData(TutorialSO tutorialSO, int currentIndex)
    {
        if (!tutorialSO) return;

        IS_DURING_TUTORIAL = true;

        currentTutorial = tutorialSO;
        currentTutorialStepIndex = currentIndex;

        LockAllExceptStep();

        switch (tutorialSO.tutorialSteps[currentIndex].tutorialType)
        {
            case TutorialType.MoveClipToCell:
                SetMoveFromClipToCellData();
                break;
            case TutorialType.MoveCellToCell:
                SetMoveFromCellToCellData();
                break;
            case TutorialType.UseDeal:
                SetDealTutorialData();
                break;
            case TutorialType.UsePotions:
                SetPotionTutorialData();
                break;
            case TutorialType.TapObject:
                SetTapTutorialData();
                break;
            default:
                break;
        }

        textParent.anchoredPosition = tutorialSO.tutorialSteps[currentIndex].textPosition;

        tutorialText.text = tutorialSO.tutorialSteps[currentIndex].tutorialText;

        CustomShowAllHeighlights();// cusotomally show any and all heighlights in the arrays

        StartCoroutine(ActivateReleventHeighlights());

        ToggleAllTutorialParts(true);

        StartTutorialStep();
    }

    private void CustomShowAllHeighlights()
    {
        foreach (var index in currentTutorial.tutorialSteps[currentTutorialStepIndex].slotIndexes)
        {
            Tutoriable tempTutoriable = null;

            GameManager.gameClip.ReturnSlot(index).gameObject.TryGetComponent<Tutoriable>(out tempTutoriable);

            if(tempTutoriable)
            {
                activeHighlights.Add(tempTutoriable);
            }
        }

        foreach (var index in currentTutorial.tutorialSteps[currentTutorialStepIndex].cellIndexes)
        {
            Tutoriable tempTutoriable = null;

            GameManager.gameRing.ringCells[index].gameObject.TryGetComponent<Tutoriable>(out tempTutoriable);

            if(tempTutoriable)
            {
                activeHighlights.Add(tempTutoriable);
            }
        }

        foreach (var index in currentTutorial.tutorialSteps[currentTutorialStepIndex].limiterIndexes)
        {
            Tutoriable tempTutoriable = null;

            GameManager.gameRing.ringSlices[index].gameObject.TryGetComponent<Tutoriable>(out tempTutoriable);

            if(tempTutoriable)
            {
                activeHighlights.Add(tempTutoriable);
            }
        }

        foreach (var index in currentTutorial.tutorialSteps[currentTutorialStepIndex].lockIndexes)
        {
            Tutoriable tempTutoriable = null;

            GameManager.gameRing.ringSlices[index].lockIconAnim.gameObject.TryGetComponent<Tutoriable>(out tempTutoriable);

            if(tempTutoriable)
            {
                activeHighlights.Add(tempTutoriable);
            }
        }
    }
    private void StartTutorialStep()
    {
        Vector3 targetPos = new Vector3(originObject.transform.position.x, originObject.transform.position.y + heightOffset, originObject.transform.position.z);
        currentMoveObject = Instantiate(prefabToSpawn, targetPos, prefabToSpawn.transform.rotation);
        currentMoveObjectAnim = currentMoveObject.transform.GetChild(0).GetComponent<Animator>(); //temp hardcoded

        MiddleManCoroutine(false);
    }

    private void ToggleAllTutorialParts(bool _IsOn)
    {
        textParent.gameObject.SetActive(_IsOn);
        maskImage.gameObject.SetActive(_IsOn);
    }

    private void MiddleManCoroutine(bool isBack)
    {
        if(coroutin != null)
        {
            StopCoroutine(coroutin);
        }

        switch (currentTutorial.tutorialSteps[currentTutorialStepIndex].tutorialType)
        {
            case TutorialType.MoveClipToCell:
                coroutin = StartCoroutine(MoveFromAtoB(isBack));
                break;
            case TutorialType.MoveCellToCell:
                coroutin = StartCoroutine(MoveFromAtoB(isBack));
                break;
            case TutorialType.UseDeal:
                coroutin = StartCoroutine(TapInPlace());
                break;
            case TutorialType.UsePotions:
                coroutin = StartCoroutine(TapInPlace());
                break;
            case TutorialType.TapObject:
                coroutin = StartCoroutine(TapInPlace());
                break;
            default:
                break;
        }

    }
    private IEnumerator MoveFromAtoB(bool isBack)
    {
        if (currentMoveObjectAnim == null) yield break;

        if (isBack)
        {
            Vector3 targetPos = new Vector3(originObject.transform.position.x, originObject.transform.position.y + heightOffset, originObject.transform.position.z);

            yield return new WaitForSeconds(waitBeforeReset);

            currentMoveObject.transform.position = targetPos;
        }
        else
        {
            currentMoveObjectAnim.SetTrigger("Press&hold");

            yield return new WaitForSeconds(moveTime);

            Vector3 targetPos = new Vector3(targetObject.transform.position.x, targetObject.transform.position.y + heightOffset, targetObject.transform.position.z);

            LeanTween.move(currentMoveObject.gameObject, targetPos, moveTime);

            yield return new WaitForSeconds(moveTime);

            currentMoveObjectAnim.SetTrigger("Release");
        }


        MiddleManCoroutine(!isBack);
    }

    private IEnumerator TapInPlace()
    {
        currentMoveObjectAnim.SetTrigger("Press&hold");

        yield return new WaitForSeconds(waitBeforeResetTap);

        currentMoveObjectAnim.SetTrigger("Release");

        yield return new WaitForSeconds(waitAfterTapDone);


        MiddleManCoroutine(false);
    }
    private void SetMoveFromClipToCellData()
    {
        int clipSlotIndex = currentTutorial.tutorialSteps[currentTutorialStepIndex].slotIndexes[0];
        int ringCellIndex = currentTutorial.tutorialSteps[currentTutorialStepIndex].cellIndexes[0];

        GameManager.gameClip.ReturnSlot(clipSlotIndex).SetAsLocked(false);
        GameManager.gameRing.ringCells[ringCellIndex].SetAsLocked(false);

        GameManager.gameClip.ReturnSlot(clipSlotIndex).gameObject.TryGetComponent<Tutoriable>(out originObject);
        GameManager.gameRing.ringCells[ringCellIndex].gameObject.TryGetComponent<Tutoriable>(out targetObject);        
    }

    private void SetMoveFromCellToCellData()
    {
        int originRingCellIndex = currentTutorial.tutorialSteps[currentTutorialStepIndex].cellIndexes[0];
        int targetRingCellIndex = currentTutorial.tutorialSteps[currentTutorialStepIndex].cellIndexes[1];

        GameManager.gameRing.ringCells[originRingCellIndex].SetAsLocked(false);
        GameManager.gameRing.ringCells[targetRingCellIndex].SetAsLocked(false);

        GameManager.gameRing.ringCells[originRingCellIndex].gameObject.TryGetComponent<Tutoriable>(out originObject);
        GameManager.gameRing.ringCells[targetRingCellIndex].gameObject.TryGetComponent<Tutoriable>(out targetObject);
    }

    private void SetDealTutorialData()
    {
        dealObject.TryGetComponent<Tutoriable>(out originObject);

        if (originObject == null)
        {
            Debug.LogError("Part of tutorial is not tutoriable!");
        }
        else
        {
            dealObject.isInteractable = true;
            activeHighlights.Add(originObject);
        }
    }

    private void SetTapTutorialData()
    {
        if(currentTutorial.tutorialSteps[currentTutorialStepIndex].isTapSlot)
        {
            int clipSlotIndex = currentTutorial.tutorialSteps[currentTutorialStepIndex].slotIndexes[0];
            GameManager.gameClip.ReturnSlot(clipSlotIndex).gameObject.TryGetComponent<Tutoriable>(out originObject);
        }

        if (currentTutorial.tutorialSteps[currentTutorialStepIndex].isTapCell)
        {
            int ringCellIndex = currentTutorial.tutorialSteps[currentTutorialStepIndex].cellIndexes[0];
            GameManager.gameRing.ringCells[ringCellIndex].gameObject.TryGetComponent<Tutoriable>(out originObject);

        }

        if (currentTutorial.tutorialSteps[currentTutorialStepIndex].isTapLimiter)
        {
            int limiterIndex = currentTutorial.tutorialSteps[currentTutorialStepIndex].limiterIndexes[0];
            GameManager.gameRing.ringSlices[limiterIndex].gameObject.TryGetComponent<Tutoriable>(out originObject);
        }

        if (originObject == null)
        {
            Debug.LogError("Part of tutorial is not tutoriable!");
        }
        else
        {
            activeHighlights.Add(originObject);
        }
    }
    private void SetPotionTutorialData()
    {
        int index = currentTutorial.tutorialSteps[currentTutorialStepIndex].potionIndex;
        PowerupType typeToAdd = currentTutorial.tutorialSteps[currentTutorialStepIndex].powerType;

        PowerupManager.instance.ReturnPotionPosition(index).gameObject.TryGetComponent<Tutoriable>(out originObject);
        PowerupManager.instance.AddPotion(typeToAdd, 1);
        PowerupManager.instance.ManualUpdatePotionText(index, typeToAdd);
        PowerupManager.instance.ToggleLockSpecificPotion(index, true); // true means can use potion

        if (originObject == null)
        {
            Debug.LogError("Part of tutorial is not tutoriable!");
        }
        else
        {
            activeHighlights.Add(originObject);
        }
    }


    private IEnumerator ActivateReleventHeighlights()
    {
        //activate highlights
        foreach (var highlight in activeHighlights)
        {
            highlight.ToggleConnectedHighlight(true);
        }

        yield return new WaitForEndOfFrame();
        ToTexture();
    }

    private IEnumerator RemoveCurrentHighlights()
    {
        if (activeHighlights.Count <= 0) yield break;

        //activate highlights
        foreach (var highlight in activeHighlights)
        {
            highlight.ToggleConnectedHighlight(false);
        }

        activeHighlights.Clear();
        yield return new WaitForEndOfFrame();
        ToTexture();
    }

    private void LockAllExceptStep()
    {
        GameManager.gameClip.LockAllSlots(true);
        GameManager.gameRing.LockAllCells(true);

        UIManager.instance.ToggleLockAllCurrentScreens(false);
        PowerupManager.instance.ToggleLockAllPotions(false);
    }
    private void UnLockAll()
    {
        GameManager.gameClip.LockAllSlots(false);
        GameManager.gameRing.LockAllCells(false);

        UIManager.instance.ToggleLockAllCurrentScreens(true);
        PowerupManager.instance.ToggleLockAllPotions(true);

    }


    public bool ReturnHitCurrentNeededObject(Transform objectHit)
    {
        if(currentTutorial.tutorialSteps[currentTutorialStepIndex].RequiredCellIndex > -1)
        {
            CellBase cellBaseHit = null;
            Tile hitTile = null;

            int requiredCellIndex = currentTutorial.tutorialSteps[currentTutorialStepIndex].RequiredCellIndex;
            CellBase requiredCellBase = GameManager.gameRing.ringCells[requiredCellIndex];


            objectHit.TryGetComponent<Tile>(out hitTile);
            if(hitTile)
            {
                cellBaseHit = hitTile.cellParent;
            }
            else
            {
                objectHit.TryGetComponent<CellBase>(out cellBaseHit);
            }


            if (cellBaseHit)
            {
                if(requiredCellBase)
                {
                    if(cellBaseHit == requiredCellBase)
                    {
                        return true;
                    }
                }

            }
        }

        if(currentTutorial.tutorialSteps[currentTutorialStepIndex].RequiredSliceIndex > -1)
        {
            Slice sliceHit = objectHit.GetComponent<Slice>();
            int requiredSliceIndex = currentTutorial.tutorialSteps[currentTutorialStepIndex].RequiredSliceIndex;

            if (sliceHit)
            {
                Slice requiredSlice = GameManager.gameRing.ringSlices[requiredSliceIndex];

                if (requiredSlice)
                {
                    if (sliceHit == requiredSlice)
                    {
                        return true;
                    }
                }

            }
        }

        return false;
    }

    public bool ReturnIsCustomClip(TutorialSO tutorial)
    {
        return tutorial.tutorialSteps[0].isCustomClipAmount; //hard coded for now
    }
    public int ReturnAmountCustomClip(TutorialSO tutorial)
    {
        return tutorial.tutorialSteps[0].amountInClip; //hard coded for now
    }

    //private void Start()
    //{
    //    InvokeRepeating("toTexture", 0, 2);
    //}
    [ContextMenu("Render Now")]
    private void ToTexture()
    {
        if (secondCam.targetTexture && (secondCam.targetTexture.width != Display.main.systemWidth || secondCam.targetTexture.height != Display.main.systemHeight))
        {
            RecreateRenderTexture();
        }
        else if(secondCam.targetTexture == null)
        {
            RecreateRenderTexture();
        }
        else
        {
            Texture2D texture = new Texture2D(Display.main.systemWidth, Display.main.systemHeight, TextureFormat.RGBA32, false);
            Graphics.CopyTexture(secondCam.targetTexture, texture);

            Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);

            maskImage.sprite = sprite;

            //secondCam.targetTexture.antiAliasing = 4;
        }
    }

    public void RecreateRenderTexture()
    {
        secondCam.targetTexture = new RenderTexture(Display.main.systemWidth, Display.main.systemHeight, 32);

        secondCam.Render();


        ToTexture();
    }

}
