using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class Player : MonoBehaviour
{
    [SerializeField] private int ownedCoins;
    [SerializeField] private int ownedTears;
    [SerializeField] private int maxOwnedTears;

    //we do this to sort the materials by their main types - build, herb, witch and gem
      

    public void OnLoadData()
    {
        if(SavedData.instance.savedCoins > -1)
        {
            ownedCoins = SavedData.instance.savedCoins;
        }
    }


    //public void AddIngredient(/*LootToRecieve ingredientToAdd*/)
    //{
    //    Ingredients toAdd = ingredientToAdd.ingredient;
    //    if (ownedIngredients.ContainsKey(toAdd))
    //    {
    //        ownedIngredients[toAdd].hasChanged = true;
    //        ownedIngredients[toAdd].amount += ingredientToAdd.amount;
    //    }
    //    else
    //    {
    //        LootEntry newLootEntry = new LootEntry(toAdd, toAdd.ingredientType);
    //        ownedIngredients.Add(toAdd, newLootEntry);
    //        ownedIngredients[toAdd].hasChanged = true;
    //        ownedIngredients[toAdd].amount = ingredientToAdd.amount;

    //        AddToIngredientsComboByType(toAdd);
    //    }

    //    Debug.Log("Added: " + ingredientToAdd.amount + " " + "To: " + toAdd.ToString());
    //}

    public void AddCoins(int amount)
    {
        ownedCoins += amount;

        //UIManager.instance.RefreshRubyAndTearsTexts(ownedTears, ownedRubies);
        Debug.Log("Added: " + amount + " " + "To Rubies!");
    }

    public void RemoveCoins(int amount)
    {
        ownedCoins -= amount;

        //UIManager.instance.RefreshRubyAndTearsTexts(ownedTears, ownedRubies);
        Debug.Log("Removed: " + amount + " " + "To Rubies!");
    }
    public void AddTears(int amount)
    {
        if (CheckHasMaxTears())
        {
            Debug.Log("Has max tears");
            return;
        }

        //af adding more than the max at once then we'll get here
        ownedTears += amount;

        if(ownedTears > maxOwnedTears)
        {
            ownedTears = maxOwnedTears;
        }

        //UIManager.instance.RefreshRubyAndTearsTexts(ownedTears, ownedRubies);
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
