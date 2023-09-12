using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum sounds
{
    ButtonClick,
    CurtainsIn,
    CurtainsOut,
    GoIntoLevel,
    GoOutOfLevel,
    SelectPowerup,
    ReleasePowerup,
    UseSwitch,
    UseBomb,
    UseJoker,
    UseRefreshTiles,
    TilepPickup,
    TilePlace,
    TileConnect,
    LevelBarFillOnWin,
    LevelBarDepleteOnLose0,
    LevelBarDepleteOnLose1,
    LevelBarDepleteOnLose2,
    ErrorSound,
    ClusterTransfer,
    WinScreen,
    LoseScreen,
    Lock,
    UnLock,
    BoosterFlip,
    CoinUse
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private List<AudioSource> allAudioSources;
    [SerializeField] private Dictionary<sounds, AudioSource> audioSources;

    [Header("Deal")]
    [SerializeField] private AudioClip[] dealSounds;
    [SerializeField] private AudioSource dealAudioSource;


    [Header("General")]
    public bool isMusicMuted;
    public bool isSFXMuted;

    private void Awake()
    {
        instance = this;
    }


    private void Start()
    {
        audioSources = new Dictionary<sounds, AudioSource>();

        for (int i = 0; i < System.Enum.GetValues(typeof(sounds)).Length; i++)
        {
            audioSources.Add((sounds)i, allAudioSources[i]);
        }
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

    public void CallPlaySound(sounds sound)
    {
        StartCoroutine(PlaySound(sound));
    }
    private IEnumerator PlaySound(sounds sound)
    {
        if (audioSources[sound].gameObject.activeInHierarchy) yield break;

        audioSources[sound].gameObject.SetActive(true);

        yield return new WaitForSeconds(audioSources[sound].clip.length);

        if (!audioSources[sound].loop)
        {
            audioSources[sound].gameObject.SetActive(false);
        }
    }

    public void PlaySoundDeal()
    {
        int randomSound = Random.Range(0, dealSounds.Length);
        dealAudioSource.clip = dealSounds[randomSound];

        dealAudioSource.Play();
    }
    public void StopSound(sounds sound)
    {
        audioSources[sound].gameObject.SetActive(false);
    }

    public void StopAllSounds()
    {
        foreach (var pair in audioSources)
        {
            pair.Value.gameObject.SetActive(false);
        }
    }

    public bool isSoundPlaying(sounds sound)
    {
        return audioSources[sound].gameObject.activeInHierarchy;
    }

    [ContextMenu("Rename Sounds")]
    private void RenameSoundsInScene()
    {
        for (int i = 0; i < System.Enum.GetValues(typeof(sounds)).Length; i++)
        {
            allAudioSources[i].name = ((sounds)i).ToString();
        }
    }
}