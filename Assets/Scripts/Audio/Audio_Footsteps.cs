using UnityEngine;

public enum FootstepSurface
{
    Default,
    Wood,
    Metal,
    Grass,
    Dirt,
    Stone
}
public class Audio_Footsteps : MonoBehaviour
{
    [SerializeField] AudioClip[] footstepClips;
    [SerializeField] AudioClip[] fotstepLandingClips;
    AudioManager audioManager;

    [Header("Settings")]
    [SerializeField] float stepInterval = 0.5f;
    bool canPlayAudio = true;


    private void Awake()
    {
       audioManager = GetComponent<AudioManager>();
    }

    private void Update()
    {
        var player = GameManager.Instance.PlayerRef;
        if (player.rb.linearVelocity.magnitude > 0.5f && player.grounded)
        {
            if (canPlayAudio)
            {
                PlayFootsteps(FootstepSurface.Default);
                canPlayAudio = false;
                Invoke("ResetCanPlayAudio", stepInterval);
            }
        }
        
    }
    void ResetCanPlayAudio()
    {
        canPlayAudio = true;
    }

    public void PlayFootsteps()
    {
        var fotstepClip = footstepClips[Random.Range(0, footstepClips.Length)];

        audioManager.PlaySound(fotstepClip);

    }
    public void PlayFootsteps(FootstepSurface surface)
    {
        var fotstepClip = footstepClips[Random.Range(0, footstepClips.Length)];

        audioManager.PlaySound(fotstepClip);

    }

    public void PlayLanding()
    {
        var fotstepClip = fotstepLandingClips[Random.Range(0, fotstepLandingClips.Length)];

        audioManager.PlaySound(fotstepClip);
    }
    public void PlayLanding(FootstepSurface surface)
    {
        var fotstepClip = fotstepLandingClips[Random.Range(0, fotstepLandingClips.Length)];

        audioManager.PlaySound(fotstepClip);
    }
}
