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
    UsePotions
}

[System.Serializable]
public class TutorialData
{
    public TutorialType tutorialType;
    public int potionIndex;
    public int originalIndex;
    public int targetCellIndex;


    [Header("General")]
    public Vector3 textPosition;
    public string tutorialText;
}
[CreateAssetMenu(fileName = "Tutorial", menuName = "ScriptableObjects/Create Tutorial")]
public class TutorialSO : ScriptableObject
{
    public TutorialData[] tutorialSteps;
}
