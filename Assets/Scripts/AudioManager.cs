using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    AudioSource audioSource;

    [Header("Moving")]
    [SerializeField] AudioClip footsteps;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
        Debug.Log("Playing");
    }

    public void PlaySound(AudioClip[] clips)
    {
        audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }
}
