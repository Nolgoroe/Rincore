using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class LootEntry
{
    public Ingredients ingredient;
    public IngredientTypes ingredientType;
    public bool hasChanged;
    public int amount;

    public LootEntry(Ingredients _ingredient, IngredientTypes _ingredientType)
    {
        ingredient = _ingredient;
        ingredientType = _ingredientType;
    }
}

[System.Serializable]
public class IngredientPlusMainTypeCombo
{
    public IngredientTypes mainType;
    public List<Ingredients> typeIngredients;
}


public class Player : MonoBehaviour
{
    [SerializeField] private int ownedRubies;
    [SerializeField] private int ownedTears;
    [SerializeField] private int maxOwnedTears;

    private Dictionary<Ingredients, LootEntry> ownedIngredients;

    [Header("Ingredient combos by type")]
    //we do this to sort the materials by their main types - build, herb, witch and gem
    [SerializeField] private List<IngredientPlusMainTypeCombo> ingredientsToMainTypes;
      
    private void Start()
    {
        ownedIngredients = new Dictionary<Ingredients, LootEntry>();
    }

    [ContextMenu("Iterate in inventory")]
    private void IterateThroughInventory()
    {
        foreach (KeyValuePair<Ingredients, LootEntry> ingredient in ownedIngredients)
        {
            Debug.Log(ingredient.Key + " amount: " + ingredient.Value.amount);

            if (ingredient.Value.hasChanged)
            {
                Debug.Log("Found change!");

                ingredient.Value.hasChanged = false;

            }
        }
    }

    public void AddIngredient(LootToRecieve ingredientToAdd)
    {
        Ingredients toAdd = ingredientToAdd.ingredient;
        if (ownedIngredients.ContainsKey(toAdd))
        {
            ownedIngredients[toAdd].hasChanged = true;
            ownedIngredients[toAdd].amount += ingredientToAdd.amount;
        }
        else
        {
            LootEntry newLootEntry = new LootEntry(toAdd, toAdd.ingredientType);
            ownedIngredients.Add(toAdd, newLootEntry);
            ownedIngredients[toAdd].hasChanged = true;
            ownedIngredients[toAdd].amount = ingredientToAdd.amount;

            AddToIngredientsComboByType(toAdd);
        }

        Debug.Log("Added: " + ingredientToAdd.amount + " " + "To: " + toAdd.ToString());
    }

    public void RemoveIngredients(Ingredients ingredient, int amount)
    {
        if (ownedIngredients.ContainsKey(ingredient))
        {
            if (ownedIngredients[ingredient].amount >= amount)
            {
                ownedIngredients[ingredient].amount -= amount;
            }
            else
            {
                Debug.LogError("Tried to remove too much of this ingredient: " + ingredient.name);
            }
        }
        else
        {
            //Debug.LogError("Tried to remove non exsisting ingredient");
        }
    }

    public void AddRubies(int amount)
    {
        ownedRubies += amount;

        UIManager.instance.RefreshRubyAndTearsTexts(ownedTears, ownedRubies);
        Debug.Log("Added: " + amount + " " + "To Rubies!");
    }

    public void RemoveRubies(int amount)
    {
        ownedRubies -= amount;

        UIManager.instance.RefreshRubyAndTearsTexts(ownedTears, ownedRubies);
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

        UIManager.instance.RefreshRubyAndTearsTexts(ownedTears, ownedRubies);
        Debug.Log("Added: " + amount + " " + "To Tears!");
    }
    private void AddToIngredientsComboByType(Ingredients toAdd)
    {
        IngredientPlusMainTypeCombo Combo = ingredientsToMainTypes.Where(i => i.mainType == toAdd.ingredientType).SingleOrDefault();
        if (Combo != null)
        {
            Combo.typeIngredients.Add(toAdd);

        }
    }

    private bool CheckHasMaxTears()
    {
        return ownedTears < maxOwnedTears;
    }

    /**/
    // GETTERS!
    /**/
    public int GetOwnedRubies => ownedRubies;
    public int GetOwnedTears => ownedTears;
    public bool GetHasMaxTears => CheckHasMaxTears();
    public List<IngredientPlusMainTypeCombo> returnOwnedIngredientsByType => ingredientsToMainTypes;
    public Dictionary<Ingredients, LootEntry> returnownedIngredients => ownedIngredients;
}
