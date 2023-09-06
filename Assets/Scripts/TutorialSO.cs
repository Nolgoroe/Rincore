using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationTypes
{
    PressHold,
    PressRelease,
    Release
}
public enum TutorialType
{
    MoveClipToCell,
    MoveCellToCell,
    UseDeal,
    UsePotions,
    TapObject
}

[System.Serializable]
public class TutorialData
{
    public TutorialType tutorialType;

    [Header("General")]
    public int[] slotIndexes;
    public int[] cellIndexes;
    public int[] limiterIndexes;
    public int[] lockIndexes;

    public int RequiredCellIndex = -1;
    public int RequiredSliceIndex = -1;


    [Header("Tapping")]
    public bool isTapSlot;
    public bool isTapCell;
    public bool isTapLimiter;

    [Header("potions")]
    public int potionIndex;
    public PowerupType powerType;

    [Header("General")]
    public Vector3 textPosition;
    public string tutorialText;

    [Header("Clip")]
    public bool isCustomClipAmount;
    public int amountInClip;
}
[CreateAssetMenu(fileName = "Tutorial", menuName = "ScriptableObjects/Create Tutorial")]
public class TutorialSO : ScriptableObject
{
    public TutorialData[] tutorialSteps;
}