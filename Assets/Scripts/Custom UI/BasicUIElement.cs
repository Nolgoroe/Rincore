using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System;

[RequireComponent(typeof(CanvasGroup))]
public abstract class BasicUIElement : MonoBehaviour
{
    public CustomButtonParent[] ButtonRefrences => buttonRefrences;
    public TMP_Text[] TextRefrences => textRefrences;


    // these are public since they are used outside of their family
    public bool isSolo;
    public bool isOverrideSolo;
    public bool isPermanent;
    public bool isInteractable = true;
    public CanvasGroup group;

    // Used only by children (in family)
    [SerializeField] protected TMP_Text[] textRefrences;
    [SerializeField] protected Image[] imageRefrences;
    [SerializeField] protected SpriteRenderer[] spriterRendererRefrences;
    [SerializeField] protected CustomButtonParent[] buttonRefrences;

    [Tooltip("move by curve explanation for leantween:\n\n" +
        "In a curve we cannot go past 1 to the right as the curve moves from 0 to 1.\n\n" +
        "If we move past 1 to the right, it's like asking to continue reading from an animation that does not exist.\n\n" +
        "We can go up as high or as low as we want, 1 symbolizes the target position we want to achieve.\n\n" +
        "If we want to move to new Vector3(0, 0, 50), then 1 will be 0, 0, 50 ")]
    [SerializeField] protected AnimationCurve HoverOverMeToGetInfo = AnimationCurve.Linear(0, 0, 1, 1);


    private void OnValidate()
    {
        if (group == null)
        {
            group = GetComponent<CanvasGroup>();
        }

        if (HoverOverMeToGetInfo == null || HoverOverMeToGetInfo.keys.Length < 1) return;

        for (int i = 0; i < HoverOverMeToGetInfo.keys.Length; i++)
        {
            if(HoverOverMeToGetInfo.keys[i].time > 1)
            {
                Debug.LogError("Cannot go past 1 to the right of animation curve since used in leanTween. " +
                              "read 'hover over me to get info'");
            }
        }
    }

    /**/
    // Cancel zone
    /**/
    public void CancelAllTweens()
    {
        LeanTween.cancel(gameObject);
    }

    /**/
    // Move zone
    /**/
    public void MoveToTarget(Vector3 targetPos, float time, LeanTweenType easeType)
    {
        LeanTween.move(gameObject, targetPos, time).setEase(easeType);
    }
    public void MoveToTarget(Vector2 targetPos, float time, LeanTweenType easeType)
    {
        LeanTween.move(gameObject, targetPos, time).setEase(easeType);
    }
    public void MoveToTarget(AnimationCurve moveCurve, Vector3 targetPos, float time)
    {
        LeanTween.move(gameObject, targetPos, time).setEase(moveCurve);
    }

    /**/
    // Scale zone
    /**/
    public void ScaleToTarget(Vector3 targetScale, float time, LeanTweenType easeType)
    {
        LeanTween.scale(gameObject, targetScale, time).setEase(easeType);
    }
    public void ScaleToTarget(Vector2 targetScale, float time, LeanTweenType easeType)
    {
        LeanTween.scale(gameObject, targetScale, time).setEase(easeType);
    }
    public void ScaleToTarget(AnimationCurve scaleCurve, Vector3 targetScale, float time)
    {
        LeanTween.scale(gameObject, targetScale, time).setEase(scaleCurve);
    }

    /**/
    // Rotate zone
    /**/
    public void RotateToTarget(Quaternion targetRotation, float time, LeanTweenType easeType)
    {
        LeanTween.rotate(gameObject, targetRotation.eulerAngles, time).setEase(easeType);
    }
    public void RotateToTarget(AnimationCurve rotationCurve, Quaternion targetRotation, float time)
    {
        LeanTween.rotate(gameObject, targetRotation.eulerAngles, time).setEase(rotationCurve);
    }


    /**/
    // General zone
    /**/
    /// <summary>
    /// change color of image over time.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="time"></param>
    /// <param name="easeType"></param>
    /// <param name="image"></param>
    public void GeneralFloatValueTo(Image image, float from, float to, float time, LeanTweenType easeType)
    {
        LeanTween.value(gameObject, from, to, time).setEase(easeType).setOnUpdate((float val) =>
        {
            Color newColor = image.color;
            newColor.a = val;
            image.color = newColor;
        });
    }

    /// <summary>
    /// change material variable over time.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="time"></param>
    /// <param name="easeType"></param>
    /// <param name="image"></param>
    public void GeneralFloatValueTo(Material mat, float from, float to, float time, LeanTweenType easeType, string keyName, System.Action action = null)
    {
        LeanTween.value(gameObject, from, to, time).setEase(easeType).setOnComplete(action).setOnUpdate((float val) =>
        {
            mat.SetFloat(keyName, val);
        });
    }

    /// <summary>
    /// change color of text over time.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="time"></param>
    /// <param name="easeType"></param>
    /// <param name="textObject"></param>
    public void GeneralFloatValueTo(TMP_Text textObject, float from, float to, float time, LeanTweenType easeType)
    {
        // fill logic when needed
    }

    /// <summary>
    /// change color of Sprite over time.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="time"></param>
    /// <param name="easeType"></param>
    /// <param name="sprite"></param>
    public void GeneralFloatValueTo(Sprite sprite, float from, float to, float time, LeanTweenType easeType)
    {
        // fill logic when needed
    }

    /// <summary>
    /// change alpha of CanvasGroup over time.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="time"></param>
    /// <param name="easeType"></param>
    /// <param name="canvasGroup"></param>
    public void GeneralFloatValueTo(CanvasGroup canvasGroup, float from, float to, float time, LeanTweenType easeType, System.Action action = null)
    {
        LeanTween.value(gameObject, from, to, time).setEase(easeType).setOnComplete(action).setOnUpdate((float val) =>
        {
            canvasGroup.alpha = val;
        });
    }

    /// <summary>
    /// Change float value over time.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="time"></param>
    public void GeneralFloatValueTo(float from, float to, float time)
    {
        // fill logic when needed
    }

    /**/
    //Abstract + virtual zone (inheritence zone)
    /**/
    public virtual void SetMyElement(string[] texts, Sprite[] sprites)
    {
        if (texts != null && texts.Length > 0)
        {
            if(textRefrences.Length != texts.Length)
            {
                Debug.LogError("Text Refrences and text array do not match!", gameObject);
                return;
            }

            for (int i = 0; i < textRefrences.Length; i++)
            {
                textRefrences[i].text = texts[i];
            }
        }

        if (imageRefrences.Length > 0 && sprites != null && sprites.Length > 0)
        {
            if (imageRefrences.Length != sprites.Length)
            {
                Debug.LogError("image Refrences and sprites array do not match!", gameObject);
                return;
            }

            for (int i = 0; i < imageRefrences.Length; i++)
            {
                imageRefrences[i].sprite = sprites[i];
            }
        }

        if (spriterRendererRefrences.Length > 0 && sprites != null && sprites.Length > 0)
        {
            if (spriterRendererRefrences.Length != sprites.Length)
            {
                Debug.LogError("spriter Renderer Refrences and sprites array do not match!", gameObject);
                return;
            }

            for (int i = 0; i < spriterRendererRefrences.Length; i++)
            {
                spriterRendererRefrences[i].sprite = sprites[i];
            }
        }
    }

    public abstract void OverrideSetMyElement(string[] texts, Sprite[] sprites, System.Action[] actions);
}
