using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class MusicHandler : MonoBehaviour
{


    [SerializeField] private FloatReference currentTime;
    [SerializeField] private List<MusicData> musicDatas;

    [Range(0f, 23f)]
    [SerializeField] private float dayToNightChangeTime = 6;
    [Range(0f, 23f)]
    [SerializeField] private float nightToDayChangeTime = 18;

    [Range(0.2f, 1f)]
    [SerializeField] private float defaultMusicVolume = 1f;
    [SerializeField] private float fadeOutTimer = 1f;

    private AudioSource audioSource;
    private MusicData currentMusic;


    private bool fading;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = defaultMusicVolume;

    }

    private void Start()
    {
        CheckTime();
        audioSource.clip = currentMusic.audioClip;
        audioSource.Play();
    }


    private void Update()
    {
        if (fading == true) return;


        CheckTime();
        if (audioSource.clip != currentMusic.audioClip)
        {
            PlayMusic();
        }

        CheckLoopEnd();
    }

    private void PlayMusic()
    {
        fading = true;
        audioSource.DOFade(endValue: 0f, fadeOutTimer).OnComplete(SwitchMusic2).SetUpdate(true);
    }
    private void SwitchMusic2()
    {
        audioSource.volume = defaultMusicVolume;
        audioSource.clip = currentMusic.audioClip;
        audioSource.timeSamples = currentMusic.loopStart;
        audioSource.Play();
        fading = false;

    }

    private void CheckLoopEnd()
    {
        if (audioSource.timeSamples >= currentMusic.loopEnd)
        {
            audioSource.timeSamples = currentMusic.loopStart;
        }
    }
    private void CheckTime()
    {
        if (currentTime.Value > dayToNightChangeTime && currentTime.Value < nightToDayChangeTime)
        {
            currentMusic = musicDatas[0];//Day
        }
        else
        {
            currentMusic = musicDatas[1];//Night
        }
    }

    private void SwitchMusic()
    {
        foreach (MusicData data in musicDatas)
        {
            if (currentMusic.audioClip != data.audioClip)
            {
                currentMusic = data;
                break;
            }
        }
    }
}
