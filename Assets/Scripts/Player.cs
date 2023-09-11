using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class Player : MonoBehaviour
{
    [SerializeField] private int ownedCoins;
    [SerializeField] private int ownedTears;
    [SerializeField] private int maxOwnedTears;
      

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
    public void AddTears(int amount)
    {
        if (CheckHasMaxTears())
        {
            Debug.Log("Has max tears");
            return;
        }

        ownedTears += amount;

        if(ownedTears > maxOwnedTears)
        {
            ownedTears = maxOwnedTears;
        }

        Debug.Log("Added: " + amount + " " + "To Tears!");
    }

    private bool CheckHasMaxTears()
    {
        return ownedTears < maxOwnedTears;
    }

    /**/
    // GETTERS!
    /**/
    public int GetOwnedCoins => ownedCoins;
    public int GetOwnedTears => ownedTears;
    public bool GetHasMaxTears => CheckHasMaxTears();
}
