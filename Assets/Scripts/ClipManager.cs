using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ClipManager : MonoBehaviour
{
    [SerializeField] private int activeClipSlotsCount;

    [Header("Slots Zone")]
    [SerializeField] private ClipSlot[] slots;

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

        StartCoroutine(DeactivateClip(activeClipSlotsCount - 1)); //darken the slot

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

    private IEnumerator DeactivateClip(int index)
    {
        yield return new WaitForSeconds(delayDarkenClip);

        //Color fromColor = slots[index].GetComponent<SpriteRenderer>().color;
        //Color toColor = darkTintedColor;

        //LeanTween.value(slots[index].gameObject, fromColor, toColor, timeToDarkenClip).setEase(LeanTweenType.linear).setOnUpdate((float val) =>
        //{
        //    SpriteRenderer sr = slots[index].gameObject.GetComponent<SpriteRenderer>();
        //    Color newColor = sr.color;
        //    newColor = Color.Lerp(fromColor, toColor, val);
        //    sr.color = newColor;
        //});
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

        foreach (ClipSlot slot in slots)
        {
            slot.transform.localPosition = slot.originalSlotPos;
        }

        canUseDeal = true;
    }

    public void RenewClip()
    {
        DestroyClipData();

        for (int i = 0; i < activeClipSlotsCount; i++)
        {
            SpawnRandomTileInSlot(slots[i]);

            slots[i].originalSlotPos = slots[i].transform.localPosition;
        }

        canUseDeal = true;
    }
}
