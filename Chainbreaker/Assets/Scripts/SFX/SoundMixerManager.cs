using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;
using System.Globalization;

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
