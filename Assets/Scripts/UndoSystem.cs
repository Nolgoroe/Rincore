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

        cellBase.GrabTileFrom();
        yield return new WaitForEndOfFrame();

        cellBase.ResetLockData();


        if (undoEntries[lastIndex].originalCell.originalCellParent) // meaning we went from cell to cell
        {
            // we call directly to the dispatch since we don't need the "extra" actions here in the data
            // we also have no reason to check if the drop was successful since if we reach here that means that it 100% is since we just moved a tile - so the previous cell must be empty.

            undoEntries[lastIndex].originalCell.originalCellParent.DroopedOnDispatch(undoEntries[lastIndex].movedTile, GameManager.gameRing); 

            undoEntries[lastIndex].movedTile.transform.SetParent(undoEntries[lastIndex].originalCell.originalCellParent.transform);

            undoEntries[lastIndex].movedTile.transform.localPosition = Vector3.zero;
            undoEntries[lastIndex].movedTile.transform.localRotation = Quaternion.identity;

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

                undoEntries[lastIndex].movedTile.transform.localPosition = Vector3.zero;
                undoEntries[lastIndex].movedTile.transform.localRotation = Quaternion.identity;

                undoEntries[lastIndex].movedTile.partOfBoard = false;

            }
        }

        undoEntries.RemoveAt(lastIndex);
    }

    public void RemoveEntriesOnDeal(TileHolder holder)
    {
        for (int i = undoEntries.Count - 1; i >= 0; i--)
        {
            if (undoEntries[i].originalClipParent && undoEntries[i].originalClipParent == holder)
            {
                undoEntries.RemoveAt(i);
            }
        }
    }

    public void RemoveSpecificEntryTile(TileParentLogic tile)
    {
        UndoEntry entry = undoEntries.Where(i => i.movedTile == tile).SingleOrDefault();

        undoEntries.Remove(entry);
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
