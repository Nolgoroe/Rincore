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
    [SerializeField] private Sprite[] slotsSprites;
    [SerializeField] private Sprite[] slotsHighlightSprites;
    [SerializeField] private float ring12SlotOffsetX;
    private bool is12Ver;

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

    [Header("custom creation")]
    [SerializeField] private int currentIndexInSpecificArray;

    public void InitClipManager()
    {
        currentIndexInSpecificArray = 0;
        int customPieces = GameManager.currentLevel.arrayOfSpecificTilesInClip.Length;

        activeClipSlotsCount = slots.Length;

        for (int i = 0; i < activeClipSlotsCount; i++)
        {

            slotDisplays[i].sprite = slotsSprites[(int)GameManager.currentLevel.ringType];
            slots[i].vfxHelper.SetHighlightSprite(slotsHighlightSprites[(int)GameManager.currentLevel.ringType]);


            if (customPieces > 0 && currentIndexInSpecificArray < customPieces)
            {
                {
                    SpawnSpecificTileInSlot(slots[i]);

                    slots[i].originalSlotPos = slots[i].transform.localPosition;
                }
            }
            else
            {
                SpawnRandomTileInSlot(slots[i]);

                slots[i].originalSlotPos = slots[i].transform.localPosition;
            }
        }

        MoveSlotsData();

        canUseDeal = true;

        CheckCustomClipAmount();
    }

    private void MoveSlotsData()
    {
        switch (GameManager.currentLevel.ringType)
        {
            case Ringtype.ring8:

                if(is12Ver)
                {
                    MoveSlotsAction(true);
                }
                break;
            case Ringtype.ring12:

                if (!is12Ver)
                {
                    MoveSlotsAction(false);
                }
                break;
            default:
                break;
        }
    }

    private void MoveSlotsAction(bool reverstMove)
    {
        for (int i = 0; i < activeClipSlotsCount; i++)
        {
            RectTransform rect = null;
            slotDisplays[i].TryGetComponent<RectTransform>(out rect);

            if (rect)
            {
                if (reverstMove)
                {
                    is12Ver = false;
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x - ring12SlotOffsetX, rect.anchoredPosition.y);

                }
                else
                {
                    is12Ver = true;
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x + ring12SlotOffsetX, rect.anchoredPosition.y);
                }

            }
        }
    }

    public void CheckCustomClipAmount()
    {
        if (GameManager.currentLevel.levelTutorial != null && TutorialManager.instance.ReturnIsCustomClip(GameManager.currentLevel.levelTutorial))
        {
            activeClipSlotsCount = TutorialManager.instance.ReturnAmountCustomClip(GameManager.currentLevel.levelTutorial);

            for (int i = activeClipSlotsCount; i < slots.Length; i++)
            {
                if (slots[i].heldTile)
                {
                    ImmediateDeactivateClipSlot(i); //darken the slot

                    Destroy(slots[i].heldTile.gameObject);
                }

            }
        }
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
        LevelSO currentLevel = GameManager.currentLevel;
        Tiletype tileType = Tiletype.NoType;

        switch (currentLevel.ringType)
        {
            case Ringtype.ring8:
                tileType = Tiletype.Normal;
                break;
            case Ringtype.ring12:
                tileType = Tiletype.Normal12;
                break;
            case Ringtype.NoType:
                break;
            default:
                break;
        }

        Tile tile = tileCreatorPreset.CreateTile(tileType, GameManager.currentLevel.levelAvailablesymbols, GameManager.currentLevel.levelAvailableColors);
        slot.RecieveTileDisplayer(tile);
    }
    private void SpawnSpecificTileInSlot(ClipSlot slot)
    {
        LevelSO currentLevel = GameManager.currentLevel;
        Tiletype tileType = Tiletype.NoType;

        switch (currentLevel.ringType)
        {
            case Ringtype.ring8:
                tileType = Tiletype.Normal;
                break;
            case Ringtype.ring12:
                tileType = Tiletype.Normal12;
                break;
            case Ringtype.NoType:
                break;
            default:
                break;
        }
        SubTileSymbol leftSymbol = SubTileSymbol.NoShape;
        SubTileSymbol rightSymbol = SubTileSymbol.NoShape;

        if(!GameManager.currentLevel.isLevelColorOnly)
        {
            leftSymbol = currentLevel.arrayOfSpecificTilesInClip[currentIndexInSpecificArray].leftTileSymbol;
            rightSymbol = currentLevel.arrayOfSpecificTilesInClip[currentIndexInSpecificArray].rightTileSymbol;
        }

        SubTileColor leftColor = currentLevel.arrayOfSpecificTilesInClip[currentIndexInSpecificArray].leftTileColor;
        SubTileColor rightColor = currentLevel.arrayOfSpecificTilesInClip[currentIndexInSpecificArray].rightTileColor;

        Tile tile = tileCreatorPreset.CreateTile(tileType, leftSymbol, rightSymbol, leftColor, rightColor);
        slot.RecieveTileDisplayer(tile);

        currentIndexInSpecificArray++;
    }

    // called from event
    public void CallDealAction()
    {
        StartCoroutine(DealAction());
    }
    private IEnumerator DealAction()
    {
        if (!canUseDeal) yield break;


        if (TutorialManager.IS_DURING_TUTORIAL)
        {
            StartCoroutine(TutorialManager.instance.AdvanceTutorialStep());
        }


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
    private void ImmediateDeactivateClipSlot(int index)
    {
        Image sr = slotDisplays[index].gameObject.GetComponent<Image>();

        if(sr)
        {
            sr.color = darkTintedColor;
        }
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
        //if (activeClipSlotsCount == slots.Length) return false;

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

    public ClipSlot ReturnSlot(int index)
    {
        return slots[index];
    }

    public void LockAllSlots(bool _isLock)
    {
        foreach (var slot in slots)
        {
            slot.isLocked = _isLock;
        }
    }

    public void EnableSlotsBoosterHighlights(PowerupType type, bool _isOn)
    {
        foreach (ClipSlot slot in slots)
        {
            if(!_isOn)
            {
                slot.vfxHelper.EnableBoosterHighlight(_isOn, PowerupManager.instance.onUseSwitchColor);
            }
            else
            {
                switch (type)
                {
                    case PowerupType.Switch:
                        if (slot.heldTile && slot.heldTile.CheckSidesDifferent())
                        {
                            slot.vfxHelper.EnableBoosterHighlight(_isOn, PowerupManager.instance.onUseSwitchColor);
                        }
                        break;
                    case PowerupType.Bomb:
                        if (slot.heldTile)
                        {
                            slot.vfxHelper.EnableBoosterHighlight(_isOn, PowerupManager.instance.onUseBombColor);
                        }
                        break;
                    case PowerupType.Joker:
                        if (slot.heldTile && slot.heldTile.CheckIsNotJoker())
                        {
                            slot.vfxHelper.EnableBoosterHighlight(_isOn, PowerupManager.instance.onUseJokerColor);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
