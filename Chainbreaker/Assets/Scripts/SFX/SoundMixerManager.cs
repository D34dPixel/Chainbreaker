using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager: MonoBehaviour
{
    //Creates a singleton sound mixer manager, would be used for sound settings if added

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
