using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : Singleton<SoundManager>
{
    public AudioSource audioSource;
    public AudioClip bgmClip;
    public AudioClip mouseClickClip;
    public AudioClip placeStoneClip;

    public void SetBGMSound()
    {
        audioSource.clip = bgmClip;     
        audioSource.playOnAwake = true; 
        audioSource.loop = true;        
        audioSource.volume = 0.1f;      

        audioSource.Play();
    }

    public void OnClickSound()
    {
        audioSource.PlayOneShot(mouseClickClip);
    }
    public void OnPlaceStoneSound()
    {
        audioSource.PlayOneShot(placeStoneClip);
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        throw new System.NotImplementedException();
    }
}
