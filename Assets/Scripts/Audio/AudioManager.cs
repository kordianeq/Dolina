using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Kanały Audio (Dzieci AudioManager)")]
    [SerializeField] public AudioSource sfxSource;
    [SerializeField] public AudioSource musicSource;
    [SerializeField] public AudioSource ambienceSource;

    [Header("Aktualne poziomy głośności (0 - 1)")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float ambientVolume = 1f;

    [System.Serializable]
    public struct SceneAudioMapping
    {
        public string sceneName;
        public AudioProfile profile;
    }

    [Header("Automatyczne profile scen (opcjonalna centralna lista)")]
    [Tooltip("Jeśli poziom nie posiada komponentu LevelAudio, AudioManager użyje tego przypisania.")]
    public List<SceneAudioMapping> sceneProfiles = new List<SceneAudioMapping>();
    public AudioProfile defaultFallbackProfile;

    // Aktualnie odtwarzany profil audio
    public AudioProfile CurrentProfile { get; private set; }

    private float currentMusicRelativeVol = 1f;
    private float currentAmbientRelativeVol = 1f;

    private Coroutine musicFadeRoutine;
    private Coroutine ambientFadeRoutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureAudioSources();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        EnsureAudioSources();

        // Wczytaj zapisane poziomy głośności z pliku konfiguracyjnego
        SettingsSystem.Load();
        ApplyAllVolumes(
            SettingsSystem.currentSettings.masterVolume,
            SettingsSystem.currentSettings.musicVolume,
            SettingsSystem.currentSettings.sfxVolume,
            SettingsSystem.currentSettings.ambientVolume
        );
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sprawdź po załadowaniu sceny, czy ma swój LevelAudio, a jeśli nie - użyj fallbacku z centralnej bazy
        StartCoroutine(CheckSceneAudioFallback(scene.name));
    }

    private IEnumerator CheckSceneAudioFallback(string sceneName)
    {
        // Poczekaj 1 klatkę, aby komponent LevelAudio na scenie miał pierwszeństwo w swoim Start()
        yield return null;

        if (FindFirstObjectByType<LevelAudio>() != null)
        {
            yield break;
        }

        var mapping = sceneProfiles.Find(m => m.sceneName.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase));
        if (mapping.profile != null)
        {
            PlayProfile(mapping.profile);
        }
        else if (defaultFallbackProfile != null)
        {
            PlayProfile(defaultFallbackProfile);
        }
    }

    /// <summary>
    /// Odnajduje i przypisuje komponenty AudioSource na obiektach dzieciach (Sfx, Music, Ambience).
    /// </summary>
    public void EnsureAudioSources()
    {
        // 1. Sprawdź dzieci po nazwach (Sfx, Music, Ambience)
        if (sfxSource == null)
        {
            Transform t = transform.Find("Sfx") ?? transform.Find("SFX");
            if (t != null) sfxSource = t.GetComponent<AudioSource>();
        }
        if (musicSource == null)
        {
            Transform t = transform.Find("Music") ?? transform.Find("Muzyka");
            if (t != null) musicSource = t.GetComponent<AudioSource>();
        }
        if (ambienceSource == null)
        {
            Transform t = transform.Find("Ambience") ?? transform.Find("Ambient") ?? transform.Find("Otoczenie");
            if (t != null) ambienceSource = t.GetComponent<AudioSource>();
        }

        // 2. Jeśli nadal brakuje, wyszukaj we wszystkich komponentach dzieci
        if (sfxSource == null || musicSource == null || ambienceSource == null)
        {
            AudioSource[] sources = GetComponentsInChildren<AudioSource>(true);
            foreach (var src in sources)
            {
                string lower = src.name.ToLowerInvariant();
                if (sfxSource == null && lower.Contains("sfx")) sfxSource = src;
                else if (musicSource == null && lower.Contains("music")) musicSource = src;
                else if (ambienceSource == null && (lower.Contains("ambien") || lower.Contains("otoczen"))) ambienceSource = src;
            }
        }

        // 3. Fallback: jeśli któregoś kanału brak, utwórz go dynamicznie jako dziecko
        if (sfxSource == null)
        {
            GameObject obj = new GameObject("Sfx");
            obj.transform.SetParent(transform);
            sfxSource = obj.AddComponent<AudioSource>();
        }
        if (musicSource == null)
        {
            GameObject obj = new GameObject("Music");
            obj.transform.SetParent(transform);
            musicSource = obj.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        if (ambienceSource == null)
        {
            GameObject obj = new GameObject("Ambience");
            obj.transform.SetParent(transform);
            ambienceSource = obj.AddComponent<AudioSource>();
            ambienceSource.loop = true;
        }
    }

    // --- PROFIL AUDIO (AudioProfile) I PŁYNNY CROSSFADE ---

    /// <summary>
    /// Uruchamia dany profil audio z płynnym wyciszeniem starego i wejściem nowego.
    /// Jeśli ten sam profil lub klip już gra, nie przerywa go!
    /// </summary>
    public void PlayProfile(AudioProfile profile, float overrideFadeTime = -1f)
    {
        if (profile == null) return;

        EnsureAudioSources();

        float fadeTime = overrideFadeTime >= 0f ? overrideFadeTime : profile.crossfadeDuration;
        CurrentProfile = profile;

        // Muzyka
        if (profile.musicClip != null)
        {
            CrossfadeMusic(profile.musicClip, profile.musicVolume, profile.loopMusic, fadeTime);
        }
        else
        {
            StopMusic(fadeTime);
        }

        // Ambience
        if (profile.ambientClip != null)
        {
            CrossfadeAmbience(profile.ambientClip, profile.ambientVolume, profile.loopAmbience, fadeTime);
        }
        else
        {
            StopAmbience(fadeTime);
        }
    }

    public void CrossfadeMusic(AudioClip newClip, float relativeVol = 1f, bool loop = true, float duration = 1.5f)
    {
        if (musicSource == null) return;
        if (musicFadeRoutine != null) StopCoroutine(musicFadeRoutine);
        musicFadeRoutine = StartCoroutine(CrossfadeRoutine(musicSource, newClip, relativeVol, loop, duration, true));
    }

    public void CrossfadeAmbience(AudioClip newClip, float relativeVol = 1f, bool loop = true, float duration = 1.5f)
    {
        if (ambienceSource == null) return;
        if (ambientFadeRoutine != null) StopCoroutine(ambientFadeRoutine);
        ambientFadeRoutine = StartCoroutine(CrossfadeRoutine(ambienceSource, newClip, relativeVol, loop, duration, false));
    }

    private IEnumerator CrossfadeRoutine(AudioSource source, AudioClip newClip, float targetRelativeVol, bool loop, float duration, bool isMusic)
    {
        float channelMaxVol = isMusic ? musicVolume : ambientVolume;
        float targetFinalVol = channelMaxVol * targetRelativeVol;

        // Jeśli dokładnie ten sam klip już gra, nie przerywamy – jedynie płynnie korygujemy głośność
        if (source.clip == newClip && source.isPlaying && newClip != null)
        {
            if (isMusic) currentMusicRelativeVol = targetRelativeVol;
            else currentAmbientRelativeVol = targetRelativeVol;

            float startV = source.volume;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                channelMaxVol = isMusic ? musicVolume : ambientVolume;
                targetFinalVol = channelMaxVol * targetRelativeVol;
                source.volume = Mathf.Lerp(startV, targetFinalVol, duration > 0f ? t / duration : 1f);
                yield return null;
            }
            source.volume = targetFinalVol;
            yield break;
        }

        // 1. Fade out starego dźwięku
        if (source.isPlaying && source.volume > 0.001f && duration > 0.05f)
        {
            float halfDuration = duration * 0.5f;
            float startV = source.volume;
            float t = 0f;
            while (t < halfDuration)
            {
                t += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startV, 0f, t / halfDuration);
                yield return null;
            }
            source.volume = 0f;
        }

        // 2. Podmiana i start nowego klipu
        if (newClip != null)
        {
            source.clip = newClip;
            source.loop = loop;
            source.Play();

            if (isMusic) currentMusicRelativeVol = targetRelativeVol;
            else currentAmbientRelativeVol = targetRelativeVol;

            // 3. Fade in nowego dźwięku
            float halfDuration = duration * 0.5f;
            float t = 0f;
            while (t < halfDuration)
            {
                t += Time.unscaledDeltaTime;
                channelMaxVol = isMusic ? musicVolume : ambientVolume;
                targetFinalVol = channelMaxVol * targetRelativeVol;
                source.volume = Mathf.Lerp(0f, targetFinalVol, halfDuration > 0f ? t / halfDuration : 1f);
                yield return null;
            }
            channelMaxVol = isMusic ? musicVolume : ambientVolume;
            source.volume = channelMaxVol * targetRelativeVol;
        }
        else
        {
            source.Stop();
            source.clip = null;
        }
    }

    public void StopMusic(float fadeDuration = 1.0f)
    {
        if (musicSource == null) return;
        CrossfadeMusic(null, 0f, false, fadeDuration);
    }

    public void StopAmbience(float fadeDuration = 1.0f)
    {
        if (ambienceSource == null) return;
        CrossfadeAmbience(null, 0f, false, fadeDuration);
    }

    // --- KONTROLA GŁOŚNOŚCI KANAŁÓW ---

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        AudioListener.volume = masterVolume;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * currentMusicRelativeVol;
        }
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        if (ambienceSource != null)
        {
            ambienceSource.volume = ambientVolume * currentAmbientRelativeVol;
        }
    }

    public void ApplyAllVolumes(float master, float music, float sfx, float ambient)
    {
        SetMasterVolume(master);
        SetMusicVolume(music);
        SetSfxVolume(sfx);
        SetAmbientVolume(ambient);
    }

    // Kompatybilność wsteczna:
    public void SetVolume(float newVolume)
    {
        SetMasterVolume(newVolume);
    }

    public float GetVolume() => masterVolume;
    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSfxVolume() => sfxVolume;
    public float GetAmbientVolume() => ambientVolume;

    // --- BEZPOŚREDNIE ODTWARZANIE DŹWIĘKÓW I MUZYKI ---

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        CrossfadeMusic(clip, 1f, loop, 1.2f);
    }

    public void PlayAmbience(AudioClip clip)
    {
        CrossfadeAmbience(clip, 1f, true, 1.2f);
    }

    public void PlaySound(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySound(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0 || sfxSource == null) return;

        if (clips.Length == 1)
        {
            if (clips[0] != null) sfxSource.PlayOneShot(clips[0]);
        }
        else
        {
            var clip = clips[Random.Range(0, clips.Length)];
            if (clip != null) sfxSource.PlayOneShot(clip);
        }
    }

    public void PlaySound(AudioClip[] clips, float destroyTimer)
    {
        PlaySound(clips);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        EnsureAudioSources();
        if (Application.isPlaying)
        {
            ApplyAllVolumes(masterVolume, musicVolume, sfxVolume, ambientVolume);
        }
    }
#endif
}
