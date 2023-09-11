using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class Player : MonoBehaviour
{
    [SerializeField] private int ownedCoins;
      

    public void OnLoadData()
    {
        if(SavedData.instance.savedCoins > -1)
        {
            ownedCoins = SavedData.instance.savedCoins;
        }
    }

    public void AddCoins(int amount)
    {
        ownedCoins += amount;

        Debug.Log("Added: " + amount + " " + "To Rubies!");
    }

    public void RemoveCoins(int amount)
    {
        ownedCoins -= amount;

        Debug.Log("Removed: " + amount + " " + "To Rubies!");
    }

    /**/
    // GETTERS!
    /**/
    public int GetOwnedCoins => ownedCoins;
}
