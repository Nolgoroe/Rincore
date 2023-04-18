using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MainSystems
{
    playerInventory,
    playerAnimals
}

[System.Serializable]
public class MainSystemToRefrences
{
    public MainSystems mainSystem;
    public GameObject[] iconables; //TEMP - this will later be the Iconable interface!!!
}

public class InterestPointManager : MonoBehaviour
{
    [SerializeField] private MainSystemToRefrences[] mainSystemsToRefrences;
}
