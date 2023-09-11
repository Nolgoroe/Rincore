using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavedData : MonoBehaviour
{
    public static SavedData instance;

    [Header("Saved Data")]
    public int currentClusterIDReached;
    public int savedCoins;


    private void Awake()
    {
        instance = this;
    }
}
