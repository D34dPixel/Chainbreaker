using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager: MonoBehaviour
{

    public static SoundMixerManager instance;

    public AudioMixer mixer;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }


}
