using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ImageTextHolderHelper : MonoBehaviour
{
    public Image connectedImage;
    public SpriteRenderer connectedRender;
    public TMP_Text connectedText;

    public void SetDisplay(Sprite sptrite, string text)
    {
        if(connectedImage)
        {
            connectedImage.sprite = sptrite;
        }

        if (connectedRender)
        {
            connectedRender.sprite = sptrite;
        }

        if (connectedText)
        {
            connectedText.text = text;
        }
    }
}
