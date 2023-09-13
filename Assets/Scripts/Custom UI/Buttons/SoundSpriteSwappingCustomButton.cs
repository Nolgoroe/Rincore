using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SoundSpriteSwappingCustomButton : CustomButtonParent
{
    [SerializeField] private Sprite spriteVerMuted;
    [SerializeField] private Sprite spriteVerUnMuted;
    [SerializeField] private UnityEvent buttonEventsOnEnable;

    [SerializeField] private Image connectedImage;
    [SerializeField] private SpriteRenderer connectedSpriteRenderer;
    
    private void OnEnable()
    {
        buttonEventsOnEnable?.Invoke();
    }

    public override void OnClickButton()
    {
        SoundManager.instance.CallPlaySound(sounds.ButtonClick);

        buttonEvents?.Invoke();
        buttonEventsInspector?.Invoke();
    }

    public override void OverrideSetMyElement(string[] texts, Sprite[] sprites, Action[] actions)
    {
        // nothing happends here
    }

    //called from button
    public void CheckSoundManagerMusicState()
    {
        if(SoundManager.instance.isMusicMuted)
        {
            SetDisplayAsMuted();
        }
        else
        {
            SetDisplayAsUnMuted();
        }
    }

    //called from button
    public void CheckSoundManagerSFXState()
    {
        if (SoundManager.instance.isSFXMuted)
        {
            SetDisplayAsMuted();

        }
        else
        {
            SetDisplayAsUnMuted();
        }
    }

    private void SetDisplayAsMuted()
    {
        if (connectedImage)
        {
            connectedImage.sprite = spriteVerMuted;
        }

        if (connectedSpriteRenderer)
        {
            connectedSpriteRenderer.sprite = spriteVerMuted;
        }
    }
    private void SetDisplayAsUnMuted()
    {
        if (connectedImage)
        {
            connectedImage.sprite = spriteVerUnMuted;
        }

        if (connectedSpriteRenderer)
        {
            connectedSpriteRenderer.sprite = spriteVerUnMuted;
        }
    }
}