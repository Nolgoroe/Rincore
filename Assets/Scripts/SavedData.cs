using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavedData : MonoBehaviour
{
    public static SavedData instance;

    [Header("Saved Data")]
    public int currentClusterIDReached;
    public int savedCoins;
    public int savedJokerCount;
    public int savedSwitchCount;
    public int savedBombCount;
    public int savedRefreshCount;
    public int savedUndoCount;


    private void Awake()
    {
        instance = this;
    }
}
