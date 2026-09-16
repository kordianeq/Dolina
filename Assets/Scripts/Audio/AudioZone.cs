using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AudioZone : MonoBehaviour
{
    [Header("Profil wewnątrz strefy")]
    [Tooltip("Profil audio uruchamiany po wejściu gracza w ten trigger (np. wejście do Saloonu, jaskini).")]
    public AudioProfile zoneProfile;

    [Header("Profil po wyjściu")]
    [Tooltip("Opcjonalny profil przywracany po wyjściu (jeśli pusty, przywraca profil sprzed wejścia).")]
    public AudioProfile returnProfile;

    [Header("Czas przejścia")]
    public float transitionDuration = 1.2f;

    private AudioProfile previousProfile;
    private Collider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;

        if (AudioManager.Instance != null && zoneProfile != null)
        {
            previousProfile = AudioManager.Instance.CurrentProfile;
            AudioManager.Instance.PlayProfile(zoneProfile, transitionDuration);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;

        if (AudioManager.Instance != null)
        {
            AudioProfile target = returnProfile != null ? returnProfile : previousProfile;
            if (target != null)
            {
                AudioManager.Instance.PlayProfile(target, transitionDuration);
            }
        }
    }

    private bool IsPlayer(Collider other)
    {
        if (other.CompareTag("Player")) return true;
        if (other.GetComponentInParent<SourceMovement>() != null) return true;
        if (other.GetComponentInParent<PlayerStats>() != null) return true;
        return false;
    }
}

