using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : Singleton<SoundManager>
{
    public AudioSource bgmAudioSource;
    public AudioSource vfxAudioSource;
    public AudioClip bgmClip;
    public AudioClip[] vfxClips;

    private Dictionary<string, int> _vfxDic = new Dictionary<string, int>();

    private void Start()
    {
        SetBGMSound();
        for (int i = 0; i < vfxClips.Length; i++)
        {
            _vfxDic.Add(vfxClips[i].name, i);
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

    public void PlayVFX(string vfxName)
    {
        int idx = _vfxDic[vfxName];
        vfxAudioSource.PlayOneShot(vfxClips[idx]);
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {

    }
}
