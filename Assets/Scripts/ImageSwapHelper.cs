using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSwapHelper : MonoBehaviour
{
    public GameObject deActivatedChild, activatedChild;

    public SpriteRenderer connectedRenderer;
    public Image connectedImage;

    public Sprite deActivatedChildSprite, activatedChildSprite;

    private void OnValidate()
    {
        TryGetComponent<SpriteRenderer>(out connectedRenderer);
        TryGetComponent<Image>(out connectedImage);
    }

    public void SetActivatedChild()
    {
        if(connectedRenderer)
        {
            connectedRenderer.sprite = activatedChildSprite;
            return;
        }

        if(connectedImage)
        {
            connectedImage.sprite = activatedChildSprite;

            return;
        }

        activatedChild.SetActive(true);
        deActivatedChild.SetActive(false);
    }

    public void SetDeActivatedChild()
    {
        if (connectedRenderer)
        {
            connectedRenderer.sprite = deActivatedChildSprite;

            return;
        }

        if (connectedImage)
        {
            connectedImage.sprite = deActivatedChildSprite;

            return;
        }

        activatedChild.SetActive(false);
        deActivatedChild.SetActive(true);
    }

}
