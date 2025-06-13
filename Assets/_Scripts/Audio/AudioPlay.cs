using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayAudio : MonoBehaviour, IPlayAudio
{
    AudioSource source;
    void Start()
    {
        source = GetComponent<AudioSource>();
    }
    public void AudioPlay(AudioClip audio)
    {
        source.PlayOneShot(audio);
    }
}
