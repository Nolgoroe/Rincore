using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private bool isMusicMutedPrivate; // temp here - just to see in inspector
    [SerializeField] private bool isSFXMutedPrivate; // temp here - just to see in inspector
    public bool isMusicMuted
    {
        get { return isMusicMutedPrivate; }
        private set { isMusicMutedPrivate = value; } 
    }

    public bool isSFXMuted
    {
        get { return isSFXMutedPrivate; }
        private set { isSFXMutedPrivate = value; }
    }

    private void Awake()
    {
        instance = this;
    }

    public void ToggleSFX()
    {
        //called from button acttion delegation

        if (isSFXMuted)
        {
            // un-mute music
            Debug.Log("Un-Muted SFX");
        }
        else
        {
            // mute music
            Debug.Log("Muted SFX");
        }

        isSFXMuted = !isSFXMuted;
    }
    public void ToogleMusic()
    {
        //called from button delegation

        if (isMusicMuted)
        {

            // un-mute music
            Debug.Log("Un-Muted Music");
        }
        else
        {
            // mute music
            Debug.Log("Muted Music");
        }

        isMusicMuted = !isMusicMuted;
    }
}
