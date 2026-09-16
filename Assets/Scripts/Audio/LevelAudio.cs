using UnityEngine;

public class LevelAudio : MonoBehaviour
{
    [Header("Profil Audio Poziomu")]
    [Tooltip("Przypisz ScriptableObject AudioProfile dla tego poziomu (np. Profile_Desert, Profile_MainMenu).")]
    public AudioProfile levelProfile;

    [Header("Opcjonalne nadpisanie przejścia")]
    [Tooltip("Wartość ujemna (np. -1) oznacza użycie domyślnego czasu z AudioProfile.")]
    public float overrideCrossfadeTime = -1f;

    private void Start()
    {
        ApplyLevelAudio();
    }

    /// <summary>
    /// Uruchamia profil audio dla bieżącego poziomu z płynnym przejściem.
    /// </summary>
    public void ApplyLevelAudio()
    {
        if (AudioManager.Instance != null && levelProfile != null)
        {
            AudioManager.Instance.PlayProfile(levelProfile, overrideCrossfadeTime);
        }
    }
}

