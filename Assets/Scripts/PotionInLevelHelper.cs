using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PotionInLevelHelper : MonoBehaviour
{
    [SerializeField] private CameraShake connectedShake;
    [SerializeField] private MeshRenderer connectedRenderer;
    [SerializeField] private TMP_Text connectedText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private BasicCustomUIWindow buyWidndow;
    [SerializeField] private GameObject selectedImage;
    [SerializeField] private float normalCountSize;
    [SerializeField] private float noUsesCountSize;
    [SerializeField] private GameObject plusButton;
    public Animator connectedAnim;
    public BasicCustomButton buyButton;

    public void SetPotionDisplay(string in_text, string in_Price_Text, Texture in_Tex)
    {
        if (connectedRenderer && in_Tex)
        {
            connectedRenderer.materials[0].SetTexture("_BaseMap", in_Tex);
        }

        if (connectedText)
        {
            if(in_text == "0")
            {
                in_text = "+";
                connectedText.fontSize = noUsesCountSize;
                plusButton.SetActive(true);
            }
            else
            {
                connectedText.fontSize = normalCountSize;
                plusButton.SetActive(false);

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
        if (in_text == "0")
        {
            in_text = "+";
            connectedText.fontSize = noUsesCountSize;
            plusButton.SetActive(true);
        }
        else
        {
            connectedText.fontSize = normalCountSize;
            plusButton.SetActive(false);
        }

        connectedText.text = in_text;
    }

    public void SetAsSelected(bool _IsSelected)
    {
        if (selectedImage)
        {
            selectedImage.SetActive(_IsSelected);
        }
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

    public void ShakeNow()
    {
        connectedShake.ShakeOnce();
    }

    public void PlayFlipSound()
    {
        SoundManager.instance.CallPlaySound(sounds.BoosterFlip);
    }
}
