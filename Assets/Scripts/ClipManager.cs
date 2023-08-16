using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ClipManager : MonoBehaviour
{
    [SerializeField] private int activeClipSlotsCount;

    [Header("Slots Zone")]
    [SerializeField] private ClipSlot[] slots;
    [SerializeField] private Image[] slotDisplays;

    [Header("Required refrences")]
    public TileCreator tileCreatorPreset;

    [Header("Deal anim")]
    [SerializeField] private Vector3 piecesDealPositionsOut;
    [SerializeField] private float delayClipMove;
    [SerializeField] private float timeToAnimateMove;
    [SerializeField] private float waitTimeBeforeIn;
    [SerializeField] private float delayDarkenClip;
    [SerializeField] private float timeToDarkenClip;
    [SerializeField] private Color darkTintedColor;

    [SerializeField] public static bool canUseDeal;
    [SerializeField] private float dealDelay;
    public void InitClipManager()
    {
        activeClipSlotsCount = slots.Length;

        for (int i = 0; i < activeClipSlotsCount; i++)
        {
            SpawnRandomTileInSlot(slots[i]);

            slots[i].originalSlotPos = slots[i].transform.localPosition;
        }

        canUseDeal = true;
    }
    public void RePopulateFirstEmpty()
    {
        foreach (ClipSlot slot in slots)
        {
            if(slot.heldTile == null)
            {
                SpawnRandomTileInSlot(slot);
                return;
            }
        }
    }
    public void RePopulateSpecificSlot(ClipSlot slot)
    {
        if (slot.heldTile == null)
        {
            SpawnRandomTileInSlot(slot);
            return;
        }
    }

    private void SpawnRandomTileInSlot(ClipSlot slot)
    {
        Tile tile = tileCreatorPreset.CreateTile(Tiletype.Normal, GameManager.currentLevel.levelAvailablesymbols, GameManager.currentLevel.levelAvailableColors);
        slot.RecieveTileDisplayer(tile);
    }

    // called from event
    public void CallDealAction()
    {
        StartCoroutine(DealAction());
    }
    private IEnumerator DealAction()
    {
        if (!canUseDeal) yield break;

        canUseDeal = false;
        if (activeClipSlotsCount - 1 == 0)
        {
            UIManager.instance.DisplayInLevelLastDealWarning();
            yield break;
        }

        UndoSystem.instance.RemoveEntriesOnDeal(slots[activeClipSlotsCount - 1]);

        StartCoroutine(DeactivateClipSlot(activeClipSlotsCount - 1)); //darken the slot

        // move the tile GFX parent out of screen
        for (int i = 0; i < activeClipSlotsCount; i++)
        {
            GameObject toMove = slots[i].tileGFXParent.gameObject;

            LeanTween.move(toMove, piecesDealPositionsOut, timeToAnimateMove).setEase(LeanTweenType.easeInOutQuad).setMoveLocal(); // animate

            yield return new WaitForSeconds(delayClipMove);
        }

        yield return new WaitForSeconds(waitTimeBeforeIn);

        DestroySlotTiles(); //destroy tile

        activeClipSlotsCount--;

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < activeClipSlotsCount; i++)
        {
            SpawnRandomTileInSlot(slots[i]);
        }

        // move the tile GFX parent back into screen
        for (int i = activeClipSlotsCount - 1; i > -1; i--)
        {
            GameObject toMove = slots[i].tileGFXParent.gameObject;

            LeanTween.move(toMove, slots[i].originalSlotPos, timeToAnimateMove).setEase(LeanTweenType.easeInOutQuad).setMoveLocal(); // animate

            yield return new WaitForSeconds(delayClipMove);
        }

        yield return new WaitForSeconds(dealDelay);
        canUseDeal = true;
    }

    private IEnumerator DeactivateClipSlot(int index)
    {
        yield return new WaitForSeconds(delayDarkenClip);

        Color fromColor = slotDisplays[index].GetComponent<Image>().color;
        Color toColor = darkTintedColor;

        LeanTween.value(slotDisplays[index].gameObject, fromColor, toColor, timeToDarkenClip).setEase(LeanTweenType.linear).setOnUpdate((float val) =>
        {
            Image sr = slotDisplays[index].gameObject.GetComponent<Image>();
            Color newColor = sr.color;
            newColor = Color.Lerp(fromColor, toColor, val);
            sr.color = newColor;
        });
    }
    private IEnumerator ActivateClipSlot(int index)
    {
        yield return new WaitForSeconds(delayDarkenClip);

        Color fromColor = slotDisplays[index].GetComponent<Image>().color;
        Color toColor = Color.white;

        LeanTween.value(slotDisplays[index].gameObject, fromColor, toColor, timeToDarkenClip).setEase(LeanTweenType.linear).setOnUpdate((float val) =>
        {
            Image sr = slotDisplays[index].gameObject.GetComponent<Image>();
            Color newColor = sr.color;
            newColor = Color.Lerp(fromColor, toColor, val);
            sr.color = newColor;
        });
    }

    private void DestroySlotTiles()
    {
        foreach (ClipSlot slot in slots)
        {
            if (slot.heldTile)
            {
                Destroy(slot.heldTile.gameObject);
            }
        }
    }

    public void DestroyClipData()
    {
        DestroySlotTiles();
        activeClipSlotsCount = slots.Length;

        BrightenAllClipSlotsImmediate();
        foreach (ClipSlot slot in slots)
        {
            slot.transform.localPosition = slot.originalSlotPos;
        }

        canUseDeal = true;

    }

    public bool RenewClip()
    {
        if (activeClipSlotsCount == slots.Length) return false;

        DestroyClipData();

        BrightenAllClipSlots();

        canUseDeal = true;


        return true;
    }

    private void BrightenAllClipSlots()
    {
        for (int i = 0; i < activeClipSlotsCount; i++)
        {
            SpawnRandomTileInSlot(slots[i]);

            slots[i].originalSlotPos = slots[i].transform.localPosition;

            StartCoroutine(ActivateClipSlot(i));
        }

    }

    private void BrightenAllClipSlotsImmediate()
    {
        for (int i = 0; i < activeClipSlotsCount; i++)
        {
            Image sr = slotDisplays[i].gameObject.GetComponent<Image>();
            sr.color = Color.white;
        }
    }

    public bool isFullClip()
    {
        return activeClipSlotsCount == slots.Length;
    }
}
