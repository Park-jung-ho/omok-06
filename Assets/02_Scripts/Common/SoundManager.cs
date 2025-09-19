using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoundManager : Singleton<SoundManager>
{
    public AudioSource bgmAudioSource;
    public AudioSource sfxAudioSource;
    public AudioClip bgmClip;
    public AudioClip[] sfxClips;

    private Dictionary<string, int> _sfxDic = new Dictionary<string, int>();

    private void Start()
    {
        SetBGMSound();
        for (int i = 0; i < sfxClips.Length; i++)
        {
            _sfxDic.Add(sfxClips[i].name, i);
        }


    }

    public void SetBGMSound()
    {
        bgmAudioSource.clip = bgmClip;
        bgmAudioSource.playOnAwake = true;
        bgmAudioSource.loop = true;
        bgmAudioSource.volume = 0.1f;

        bgmAudioSource.Play();
    }

    public void PlaySFX(string sfxName)
    {
        int idx = _sfxDic[sfxName];
        sfxAudioSource.PlayOneShot(sfxClips[idx]);
    }

    private void SetButtonSound()
    {
        var buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var button in buttons)
        {
            button.onClick.AddListener(() => PlaySFX("click"));
        }
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {

    }
}
