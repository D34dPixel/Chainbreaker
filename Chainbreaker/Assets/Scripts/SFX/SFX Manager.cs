using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

    public AudioSource SFXprefab;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }


    public void PlaySFXClip(AudioClip audioClip, Vector3 spawnPos, float volume)
    {
        AudioSource audioSource = Instantiate(SFXprefab, spawnPos, Quaternion.identity);

        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();
        Destroy(audioSource.gameObject, audioSource.clip.length);
    }
}
