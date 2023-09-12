using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class OriginalCellData
{
    public CellBase originalCellParent;
    public bool isLocked;
    public bool isStone;
}
[Serializable]
public class UndoEntry
{
    [Header("Original Clip Parent Data")]
    public ClipSlot originalClipParent;

    [Header("Original Cell Parent Data")]
    public OriginalCellData originalCell;

    [Header("Current Cell Parent Data")]
    public CellBase currntCellParent;

    public TileParentLogic movedTile;
}
public class UndoSystem : MonoBehaviour
{
    public static UndoSystem instance;

    public List<UndoEntry> undoEntries;

    [SerializeField] private int YposOffsetCell;
    [SerializeField] private int YposOffsetClip;
    [SerializeField] private float timeToMove;

    private void Start()
    {
        instance = this;
    }

    public void CallUndoAction()
    {
        if (undoEntries.Count <= 0) return;

        StartCoroutine(OneStepBack());
    }

    private IEnumerator OneStepBack()
    {
        int lastIndex = undoEntries.Count - 1;

        CellBase cellBase = undoEntries[lastIndex].currntCellParent as CellBase;
        if (!cellBase) yield break;

        SoundManager.instance.CallPlaySound(sounds.TilepPickup);

        cellBase.GrabTileFrom();
        yield return new WaitForEndOfFrame();

        cellBase.ResetLockData();


        if (undoEntries[lastIndex].originalCell.originalCellParent) // meaning we went from cell to cell
        {
            // we call directly to the dispatch since we don't need the "extra" actions here in the data
            // we also have no reason to check if the drop was successful since if we reach here that means that it 100% is since we just moved a tile - so the previous cell must be empty.

            undoEntries[lastIndex].originalCell.originalCellParent.DroopedOnDispatch(undoEntries[lastIndex].movedTile, GameManager.gameRing);

            TweemMoveFromAtoB(undoEntries[lastIndex].movedTile.gameObject, Vector3.zero, timeToMove, false);
            undoEntries[lastIndex].movedTile.transform.SetParent(undoEntries[lastIndex].originalCell.originalCellParent.transform);
        }
        else
        {
            if (undoEntries[lastIndex].originalClipParent) // meaning we came from clip
            {
                if(undoEntries[lastIndex].originalClipParent.heldTile)
                {
                    Destroy(undoEntries[lastIndex].originalClipParent.heldTile.gameObject);
                }

                yield return new WaitForEndOfFrame();

                undoEntries[lastIndex].originalClipParent.heldTile = undoEntries[lastIndex].movedTile;
                undoEntries[lastIndex].movedTile.transform.SetParent(undoEntries[lastIndex].originalClipParent.transform);

                TweemMoveFromAtoB(undoEntries[lastIndex].movedTile.gameObject, Vector3.zero, timeToMove, true);

                undoEntries[lastIndex].movedTile.transform.localScale = Vector3.one;

                undoEntries[lastIndex].movedTile.partOfBoard = false;

            }
        }
        SoundManager.instance.CallPlaySound(sounds.TilePlace);


        undoEntries.RemoveAt(lastIndex);
    }

    private void TweemMoveFromAtoB(GameObject toMove, Vector3 targetPos, float moveTime, bool isClipParent)
    {
        if(isClipParent)
        {
            toMove.transform.position = new Vector3(toMove.transform.position.x, toMove.transform.position.y + YposOffsetClip, toMove.transform.position.z);

            LeanTween.moveLocal(toMove, targetPos, moveTime);
            LeanTween.rotateLocal(toMove, Vector3.zero, moveTime);
        }
        else
        {
            toMove.transform.position = new Vector3(toMove.transform.position.x, toMove.transform.position.y + YposOffsetCell, toMove.transform.position.z);
            LeanTween.moveLocal(toMove, targetPos, moveTime);
            LeanTween.rotateLocal(toMove, Vector3.zero, moveTime);
        }

    }

    public void RemoveEntriesOnDeal(TileHolder holder)
    {
        for (int i = undoEntries.Count - 1; i >= 0; i--)
        {
            if (undoEntries[i].originalClipParent)
            {
                undoEntries.RemoveAt(i);
            }
        }
    }

    public void RemoveSpecificEntryTile(TileParentLogic tile)
    {
        undoEntries.RemoveAll(i => i.movedTile == tile);
    }

    public void AddNewUndoEntry(Transform originalParent, Transform currentParent, TileParentLogic piece)
    {
        if (originalParent == null || currentParent == null || piece == null) return;

        UndoEntry newEntry = new UndoEntry();
        newEntry.originalCell = new OriginalCellData();
        newEntry.movedTile = piece;

        originalParent.TryGetComponent<ClipSlot>(out newEntry.originalClipParent);

        originalParent.TryGetComponent<CellBase>(out newEntry.originalCell.originalCellParent);

        if (newEntry.originalCell.originalCellParent)
        {
            CellBase cell = originalParent.GetComponent<CellBase>();
            if (cell == null) return;

            newEntry.originalCell.isLocked = cell.isLocked;
        }

        currentParent.TryGetComponent<CellBase>(out newEntry.currntCellParent);

        if (originalParent != currentParent)
        {
            undoEntries.Add(newEntry);
        }
    }

    public void ClearUndoSystem()
    {
        undoEntries.Clear();
    }
}
