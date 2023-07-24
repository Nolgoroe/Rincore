using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class SliceSpriteSetter
{
    public SliceConditionsEnums sliceEnum;
    public Mesh[] sliceMesh;
    public Texture[] slicePossibleTextures;
}
public class Ring : MonoBehaviour
{
    public CellBase[] ringCells;
    public Slice[] ringSlices;
    public BoxCollider levelStartCollider;

    [SerializeField] private int filledCellsCount;
    [SerializeField] private int unsuccessfulConnectionsCount;

    [SerializeField] private System.Action onAddTile;
    [SerializeField] private System.Action onRemoveTile;

    [SerializeField] private GameObject sliceDisplayPrefab; // move somewhere else?

    [SerializeField] private SliceSpriteSetter[] sliceDisplayArray; // move somewhere else?

    public void InitRing()
    {
        onAddTile += ChangeCellCountAndConnectionDataOnRemove;
        onAddTile += OnAddTileToRing;

        onRemoveTile += UpdateFilledAndConnectDataCount;
    }

    public void SpawnStoneTileInCell(int cellIndex, TileParentLogic tile, bool isStone)
    {
        ringCells[cellIndex].DroppedOn(tile, this);
        ringCells[cellIndex].SetAsLocked(isStone);
        ringCells[cellIndex].SetAsStone(isStone);

    }

    public void CallOnAddTileActions()
    {
        onAddTile?.Invoke();
    }

    public void CallOnRemoveTileFromRing()
    {
        onRemoveTile?.Invoke();
    }

    public void ClearActions()
    {
        onAddTile = null;
        onRemoveTile = null;
    }

    public void SetSliceDisplay(Slice sliceData, int sliceIndex)
    {
        //SpriteRenderer sliceDisplayObject = Instantiate(sliceDisplayPrefab, ringSlices[sliceIndex].transform).GetComponent<SpriteRenderer>();
        SliceDisplay3D sliceDisplayObject = Instantiate(sliceDisplayPrefab, ringSlices[sliceIndex].transform).GetComponent<SliceDisplay3D>();

        if (!sliceDisplayObject) return;

        sliceData.connectedDisplay = sliceDisplayObject;

        SliceSpriteSetter relaventSliceData = sliceDisplayArray.Where(x => x.sliceEnum == sliceData.connectionType).FirstOrDefault();

        if(relaventSliceData == null)
        {
            Debug.LogError("Can't find slice data");
            return;
        }

        switch (sliceData.connectionType)
        {
            case SliceConditionsEnums.GeneralColor:
                sliceDisplayObject.limiterRenderer.material.mainTexture = relaventSliceData.slicePossibleTextures[0];
                //sliceDisplayObject.limiterFilter.mesh = relaventSliceData.sliceMesh[0];
                break;
            case SliceConditionsEnums.GeneralSymbol:
                sliceDisplayObject.limiterRenderer.material.mainTexture = relaventSliceData.slicePossibleTextures[0];
                //sliceDisplayObject.limiterFilter.mesh = relaventSliceData.sliceMesh[0];
                break;
            case SliceConditionsEnums.SpecificColor:
                sliceDisplayObject.limiterRenderer.material.mainTexture = relaventSliceData.slicePossibleTextures[(int)sliceData.requiredColor];
                //sliceDisplayObject.limiterFilter.mesh = relaventSliceData.sliceMesh[0];
                break;
            case SliceConditionsEnums.SpecificSymbol:
                sliceDisplayObject.limiterRenderer.material.mainTexture = relaventSliceData.slicePossibleTextures[(int)sliceData.requiredSymbol];
                //sliceDisplayObject.limiterFilter.mesh = relaventSliceData.sliceMesh[(int)sliceData.requiredSymbol];
                break;
            default:
                Debug.LogError("Problem with slice generation");
                break;
        }


    }

    private void OnAddTileToRing()
    {
        filledCellsCount++;

        if (filledCellsCount == ringCells.Length && unsuccessfulConnectionsCount == 0)
        {
            GameManager.instance.BroadcastWinLevelActions();
            Debug.Log("Win Level");
        }

        if (filledCellsCount == ringCells.Length && unsuccessfulConnectionsCount > 0)
        {
            //GameManager.instance.BroadcastLoseLevelActions();

            Debug.Log("lose Level");
        }
    }
    public bool LastPieceRingProblems()
    {
        return filledCellsCount == GameManager.gameRing.ringCells.Length
            &&
            unsuccessfulConnectionsCount > 0;
    }

    private void UpdateFilledAndConnectDataCount()
    {
        filledCellsCount--;

        ChangeCellCountAndConnectionDataOnRemove();
    }

    private void ChangeCellCountAndConnectionDataOnRemove()
    {
        unsuccessfulConnectionsCount = 0;

        foreach (CellBase cell in ringCells)
        {
            unsuccessfulConnectionsCount += cell.GetUnsuccessfullConnections();
        }
    }

    private int CheckIndexIntInRange<T>(int index, T[] array) // this is a generic action - might want to move to different script
    {
        int returnNum = index;

        if (returnNum < 0)
        {
            returnNum = array.Length - 1;
        }

        if (returnNum > array.Length - 1)
        {
            returnNum = 0;
        }

        return returnNum;
    }

    [ContextMenu("Auto set cell neighbors")]
    private void AutoSetCellNeighbors()
    {
        int counter = 0;

        foreach (CellBase cell in ringCells)
        {
            cell.rightCell = ringCells[CheckIndexIntInRange(counter + 1, ringCells)];
            cell.leftCell = ringCells[CheckIndexIntInRange(counter - 1, ringCells)];

            cell.leftSlice = ringSlices[CheckIndexIntInRange(counter, ringSlices)];
            cell.rightSlice = ringSlices[CheckIndexIntInRange(counter + 1, ringSlices)];
            counter++;
        }

    }

}
