using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PotionIngredientSegment : UIElementDisplayerSegment
{
    [SerializeField] private Color normalTextColor;
    [SerializeField] private Color missingIngredientsColor;
    [SerializeField] private TMP_Text amountText;

    public override void OverrideSetMyElement(string[] texts, Sprite[] sprites, System.Action[] actions = null)
    {
        base.SetMyElement(texts, sprites);
    }

    public void SetColorMissingIngredients(bool missing)
    {
        if(missing)
        {
            amountText.color = missingIngredientsColor;
        }
        else
        {
            amountText.color = normalTextColor;
        }
    }

    public void SetAmountsText(string text)
    {
        amountText.text = text;
    }
}
