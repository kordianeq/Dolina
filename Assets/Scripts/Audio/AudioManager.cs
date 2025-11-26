using UnityEngine;

public class AudioManager : MonoBehaviour
{
    AudioSource audioSource;
    [SerializeField] Volume volume;
   
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Update volume in editor only to allow real-time tweaking
#if UNITY_EDITOR
        audioSource.volume = volume.currentVolume;
#endif



    }


    /// <summary>
    /// Plays the specified audio clip using the audio source.
    /// </summary>
    /// <param name="clip">The audio clip to play. Cannot be null.</param>
    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
        //Debug.Log("Playing");
    }


    /// <summary>
    /// Plays a random sound from the provided array of audio clips.
    /// </summary>
    /// <param name="clips">An array of <see cref="AudioClip"/> objects to choose from. Must contain at least one element.</param>
    public void PlaySound(AudioClip[] clips)
    {
        audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }


    /// <summary>
    /// Plays a random sound from the provided array of audio clips and destroys the audio source after a specified time. If DESTROY TIMER 0 it will destroy after clip played
    /// </summary>
    /// <param name="clips"></param>
    /// <param name="destroyTimer"></param>
    public void PlaySound(AudioClip[] clips, float destroyTimer)
    {
        var clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
        if (destroyTimer > 0)
        {
            Invoke("DestroyAudio", destroyTimer);
        }
        else
        {
            Invoke("DestroyAudio", clip.length);
        }
    }

    void DestroyAudio()
    {
        Destroy(gameObject);
    }
    public void SetVolume(float newVolume)
    {
        audioSource.volume = newVolume;
    }
}
