using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PotionInLevelHelper : MonoBehaviour
{
    [SerializeField] private MeshRenderer connectedRenderer;
    [SerializeField] private TMP_Text connectedText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private BasicCustomUIWindow buyWidndow;
    public BasicCustomButton buyButton;

    public void SetPotionDisplay(string in_text, string in_Price_Text, Texture in_Tex)
    {
        if (connectedRenderer)
        {
            connectedRenderer.materials[0].SetTexture("_BaseMap", in_Tex);
        }

        if (connectedText)
        {
            if(in_text == "0")
            {
                in_text = "+";
            }

            connectedText.text = in_text;
        }

        if (priceText)
        {
            priceText.text = in_Price_Text;
        }
    }

    public void SetTextCustom(string in_text)
    {
        connectedText.text = in_text;
    }

    public void ToggleHoverWindow(bool isActive)
    {
        if(isActive)
        {
            UIManager.instance.AddUIElement(buyWidndow);
        }
        else
        {
            UIManager.instance.CloseElement(buyWidndow);
        }
    }
}
