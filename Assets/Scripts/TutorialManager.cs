using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class TutorialManager : MonoBehaviour
{
    [Header("general refs - temp?")]
    [SerializeField] private GameObject dealObject;
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

    [Header("Live data")]
    [SerializeField] private GameObject originObject;
    [SerializeField] private GameObject targetObject;
    [SerializeField] private GameObject currentMoveObject;
    [SerializeField] private TutorialSO currentTutorial;
    [SerializeField] private int currentTutorialStepIndex;

    private void StartTutorial()
    {
        Vector3 targetPos = new Vector3(originObject.transform.position.x, originObject.transform.position.y + heightOffset, originObject.transform.position.z);
        currentMoveObject = Instantiate(prefabToSpawn, targetPos, prefabToSpawn.transform.rotation);

        MiddleManCoroutine(false);
    }

    public void SetCurrenTutorialData(TutorialSO tutorialSO, int currentIndex)
    {
        if (!tutorialSO) return;

        currentTutorial = tutorialSO;
        currentTutorialStepIndex = currentIndex;

        switch (tutorialSO.tutorialSteps[currentIndex].tutorialType)
        {
            case TutorialType.MoveClipToCell:
                SetMoveFromClipToCellData();
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

    private void SetMoveFromClipToCellData()
    {
        int clipSlotIndex = currentTutorial.tutorialSteps[currentTutorialStepIndex].originalIndex;
        int ringCellIndex = currentTutorial.tutorialSteps[currentTutorialStepIndex].targetCellIndex;
        originObject = GameManager.gameClip.ReturnSlot(clipSlotIndex).gameObject;
        targetObject = GameManager.gameRing.ringCells[ringCellIndex].gameObject;
    }

    private void SetMoveFromCellToCellData()
    {
        int originRingCellIndex = currentTutorial.tutorialSteps[currentTutorialStepIndex].originalIndex;
        int targetRingCellIndex = currentTutorial.tutorialSteps[currentTutorialStepIndex].targetCellIndex;
        originObject = GameManager.gameRing.ringCells[originRingCellIndex].gameObject;
        targetObject = GameManager.gameRing.ringCells[targetRingCellIndex].gameObject;
    }

    private void SetDealTutorialData()
    {
        originObject = dealObject;
    }

    private void SetPotionTutorialData()
    {
        int index = currentTutorial.tutorialSteps[currentTutorialStepIndex].potionIndex;

        originObject = PowerupManager.instance.ReturnPotionPosition(index).gameObject;
    }

    private void MiddleManCoroutine(bool isBack)
    {
        switch (currentTutorial.tutorialSteps[currentTutorialStepIndex].tutorialType)
        {
            case TutorialType.MoveClipToCell:
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

        StartCoroutine(MoveFromAtoB(isBack));
    }

    private IEnumerator MoveFromAtoB(bool isBack)
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



    private IEnumerator SelectReleventHeighlights()
    {
        //activate highlights


        yield return new WaitForEndOfFrame();
        toTexture();
    }

    private void Start()
    {
        InvokeRepeating("toTexture", 2, 2);
    }

    [ContextMenu("Render Now")]
    private void toTexture()
    {
        if (secondCam.targetTexture.width != Display.main.systemWidth || secondCam.targetTexture.height != Display.main.systemHeight)
        {
            RecreateRenderTexture(false);
        }
        else
        {
            Texture2D texture = new Texture2D(Display.main.systemWidth, Display.main.systemHeight, TextureFormat.ARGB32, false);
            Graphics.CopyTexture(secondCam.targetTexture, texture);

            Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);


            byte[] bytes = texture.EncodeToPNG();
            var dirPath = "C:/Users/Tiltan/Desktop/Ringers APK";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllBytes(dirPath + "Image" + ".png", bytes);
            maskImage.sprite = sprite;
        }
    }

    public void RecreateRenderTexture(bool isDen)
    {
        secondCam.targetTexture = new RenderTexture(Display.main.systemWidth, Display.main.systemHeight, camDepth);
        secondCam.Render();
        toTexture();
    }

}
