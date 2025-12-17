using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

    public AudioSource SFXprefab;

    //make sure script is a singleton
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    //clip with a parent
    public void PlaySFXClip(AudioClip audioClip, Vector3 spawnPos, float volume, Transform parent)
    {
        AudioSource audioSource = Instantiate(SFXprefab, spawnPos, Quaternion.identity, parent); //creates an audio source object at a given point with a given parent

        //set audiosource variables as required
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        
        //play audio and destroy after finished
        audioSource.Play();
        Destroy(audioSource.gameObject, audioSource.clip.length);
    }

    //clip that has no parent (this one will not be commented due to redundancy)
    public void PlaySFXClip(AudioClip audioClip, Vector3 spawnPos, float volume)
    {
        AudioSource audioSource = Instantiate(SFXprefab, spawnPos, Quaternion.identity);

        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();
        Destroy(audioSource.gameObject, audioSource.clip.length);
    }
}
