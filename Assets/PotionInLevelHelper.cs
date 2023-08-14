using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PotionInLevelHelper : MonoBehaviour
{
    [SerializeField] private MeshRenderer connectedRenderer;
    [SerializeField] private TMP_Text connectedText;

    public void SetPotionDisplay(string in_text, Texture in_Tex)
    {
        if (connectedRenderer)
        {
            connectedRenderer.materials[0].SetTexture("_BaseMap", in_Tex);
        }

        if (connectedText)
        {
            connectedText.text = in_text;
        }
    }

    public void SetTextCustom(string in_text)
    {
        connectedText.text = in_text;
    }
}
