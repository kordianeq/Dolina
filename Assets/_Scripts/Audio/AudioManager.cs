using UnityEngine;

public class AudioManager : MonoBehaviour
{
    AudioSource audioSource;
    [SerializeField] Volume volume;
    [Header("Moving")]
    [SerializeField] AudioClip footsteps;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        audioSource.volume = volume.currentVolume;
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
        //Debug.Log("Playing");
    }

    public void PlaySound(AudioClip[] clips)
    {
        audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }

    public void SetVolume(float newVolume)
    {
        audioSource.volume = newVolume;
    }
}
