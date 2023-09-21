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
    [SerializeField] private int limitTileWingRepeat;
    //[SerializeField] private int currentSlotsSpawned;

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
                SpawnSpecificTileInSlotByLevelSO(slots[i]);

                //currentSlotsSpawned++;

                slots[i].originalSlotPos = slots[i].transform.localPosition;
            }
            else
            {
                SpawnRandomTileInSlot(slots[i]);

                //currentSlotsSpawned++;
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
        int customPieces = GameManager.currentLevel.arrayOfSpecificTilesInClip.Length;

        if (customPieces > 0 && currentIndexInSpecificArray < customPieces)
        {
            SpawnSpecificTileInSlotByLevelSO(slot);
        }
        else
        {
            Tiletype tileType = ReturnCurrentTileType();
            Tile tile = tileCreatorPreset.CreateTile(tileType, GameManager.currentLevel.levelAvailablesymbols, GameManager.currentLevel.levelAvailableColors);

            slot.RecieveTileDisplayer(tile);

            if (GameManager.currentLevel.useTileCreationAlgos)
            {
                CheckRepeatInClip(slot);
            }
        }
    }

    private void CheckRepeatInClip(ClipSlot slot)
    {
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (slot != slots[i] && slot.heldTile && slots[i].heldTile)
            {
                if(CheckTilesDifferent(slot, slots[i]))
                {
                    tileCreatorPreset.ReRollTile(slot.heldTile, ReturnCurrentTileType(), GameManager.currentLevel.levelAvailablesymbols, GameManager.currentLevel.levelAvailableColors);

                    CheckRepeatInClip(slot);

                    return;
                }

                if(CheckTilesWingsRepeat(slot))
                {
                    tileCreatorPreset.ReRollTile(slot.heldTile, ReturnCurrentTileType(), GameManager.currentLevel.levelAvailablesymbols, GameManager.currentLevel.levelAvailableColors);

                    CheckRepeatInClip(slot);

                    return;
                }
            }
        }
    }

    private bool CheckTilesDifferent(ClipSlot A, ClipSlot B)
    {
        TileParentLogic tileA = A.heldTile;
        TileParentLogic tileB = B.heldTile;

        bool sameColorLeft = tileA.subTileLeft.subTileColor == tileB.subTileLeft.subTileColor;
        bool sameColorRight = tileA.subTileRight.subTileColor == tileB.subTileRight.subTileColor;

        bool sameSymbolLeft = tileA.subTileLeft.subTileSymbol == tileB.subTileLeft.subTileSymbol;
        bool sameSymbolRight = tileA.subTileRight.subTileSymbol == tileB.subTileRight.subTileSymbol;


        return sameColorLeft && sameColorRight && sameSymbolLeft && sameSymbolRight;
    }

    private bool CheckTilesWingsRepeat(ClipSlot slot)
    {
        int countSameColorAndSymbolRight = 0;
        int countSameColorAndSymbolLeft = 0;
        //int countSameSymbolRight = 0;
        //int countSameSymbolLeft = 0;

        SubTileColor rightColor = slot.heldTile.subTileRight.subTileColor;
        SubTileColor lefttColor = slot.heldTile.subTileLeft.subTileColor;

        SubTileSymbol rightSymbol = slot.heldTile.subTileRight.subTileSymbol;
        SubTileSymbol lefttSymbol = slot.heldTile.subTileLeft.subTileSymbol;

        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (slot != slots[i] && slot.heldTile && slots[i].heldTile)
            {
                bool sameColor = rightColor == slots[i].heldTile.subTileRight.subTileColor;
                bool sameSymbol = rightSymbol == slots[i].heldTile.subTileRight.subTileSymbol;
                if (sameColor && sameSymbol)
                {
                    countSameColorAndSymbolRight++;
                }

                sameColor = lefttColor == slots[i].heldTile.subTileLeft.subTileColor;
                sameSymbol = lefttSymbol == slots[i].heldTile.subTileLeft.subTileSymbol;
                if (lefttColor == slots[i].heldTile.subTileLeft.subTileColor)
                {
                    countSameColorAndSymbolLeft++;
                }
            }
        }

        return countSameColorAndSymbolRight >= limitTileWingRepeat || 
               countSameColorAndSymbolLeft >= limitTileWingRepeat;
    }

    private void SpawnSpecificTileInSlotByLevelSO(ClipSlot slot)
    {
        LevelSO currentLevel = GameManager.currentLevel;

        Tiletype tileType = ReturnCurrentTileType();

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
    public void SpawnSpecificTileInSlot(ClipSlot slot, SubTileColor rightColor, SubTileColor leftColor,SubTileSymbol rightSymbol, SubTileSymbol leftSymbol)
    {
        Tiletype tileType = ReturnCurrentTileType();

        SubTileColor newRight = rightColor;
        SubTileColor newLeft = leftColor;

        if (rightColor == SubTileColor.Stone)
        {
            int randomColorIndex = Random.Range(0, GameManager.currentLevel.levelAvailableColors.Length);
            newRight = GameManager.currentLevel.levelAvailableColors[randomColorIndex];

        }
        else if (leftColor == SubTileColor.Stone)
        {
            int randomColorIndex = Random.Range(0, GameManager.currentLevel.levelAvailableColors.Length);
            newLeft = GameManager.currentLevel.levelAvailableColors[randomColorIndex];
        }
        Tile tile = tileCreatorPreset.CreateTile(tileType, leftSymbol, rightSymbol, newLeft, newRight);
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
            SoundManager.instance.PlaySoundDeal();

            GameObject toMove = slots[i].tileGFXParent.gameObject;

            LeanTween.move(toMove, piecesDealPositionsOut, timeToAnimateMove).setEase(LeanTweenType.easeInOutQuad).setMoveLocal(); // animate

            yield return new WaitForSeconds(delayClipMove);
        }

        yield return new WaitForSeconds(waitTimeBeforeIn);

        DestroySlotTiles(); //destroy tile

        activeClipSlotsCount--;

        yield return new WaitForEndOfFrame();

        if (activeClipSlotsCount - 1 == 0 && GameManager.currentLevel.doLastTileAlgo)
        {
            LastTileAlgo();
        }
        else
        {
            for (int i = 0; i < activeClipSlotsCount; i++)
            {
                SpawnRandomTileInSlot(slots[i]);
            }

        }


        // move the tile GFX parent back into screen
        for (int i = activeClipSlotsCount - 1; i > -1; i--)
        {
            SoundManager.instance.PlaySoundDeal();

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
        DestroySlotTiles();
        if(activeClipSlotsCount < slots.Length)
        {
            activeClipSlotsCount = activeClipSlotsCount + 1;
        }

        BrightenAllClipSlotsImmediate();
        foreach (ClipSlot slot in slots)
        {
            slot.transform.localPosition = slot.originalSlotPos;
        }


        RespawnAllClipSlots();
        canUseDeal = true;


        return true;
    }

    private void RespawnAllClipSlots()
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



    private void LastTileAlgo()
    {
        int emptyCells = 0;

        CellBase cell = null;
        for (int i = 0; i < GameManager.gameRing.ringCells.Length; i++)
        {
            if (!GameManager.gameRing.ringCells[i].heldTile)
            {
                emptyCells++;
                cell = GameManager.gameRing.ringCells[i];
            }
        }

        if (emptyCells == 1 && cell != null)
        {
            SubTileColor requiredRightColor = SubTileColor.NoColor;
            SubTileSymbol requiredRightSymbol = SubTileSymbol.NoShape;

            SubTileColor requiredLeftColor = SubTileColor.NoColor;
            SubTileSymbol requiredLeftSymbol = SubTileSymbol.NoShape;


            CellBase rightCell = cell.rightCell;
            CellBase leftCell = cell.leftCell;

            requiredRightColor = rightCell.heldTile.subTileLeft.subTileColor;
            requiredRightSymbol = rightCell.heldTile.subTileLeft.subTileSymbol;

            requiredLeftColor = leftCell.heldTile.subTileRight.subTileColor;
            requiredLeftSymbol = leftCell.heldTile.subTileRight.subTileSymbol;

            ClipSlot slot = ReturnSlot(0);// temp?

            SpawnSpecificTileInSlot(slot, requiredRightColor, requiredLeftColor, requiredRightSymbol, requiredLeftSymbol);
        }
        else
        {
            for (int i = 0; i < activeClipSlotsCount; i++)
            {
                SpawnRandomTileInSlot(slots[i]);
            }
        }
    }


    private Tiletype ReturnCurrentTileType()
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

        return tileType;
    }
}
