using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    private AudioSource audioSource;

    public static AudioPlayer instance = null;

    private void Awake()
    {
        if (instance == null) instance = this;
        if (instance != this) Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    public void PlayMusic(AudioClip music)
    {
        audioSource.clip = music;
        audioSource.Play();
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
    
    public void DisableSound()
    {
        audioSource.mute = true;
    }

    public void EnableSound()
    {
        audioSource.mute = false;
    }

    public void NoMusic()
    {
        audioSource.clip = null;
    }
}
