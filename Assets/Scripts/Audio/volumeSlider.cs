using UnityEngine;
using UnityEngine.UI;

public enum AudioChannelType
{
    Master,
    Music,
    SFX,
    Ambient
}

[RequireComponent(typeof(Slider))]
public class volumeSlider : MonoBehaviour
{
    [Header("Typ kanału audio")]
    [Tooltip("Wybierz który kanał ma kontrolować ten suwak.")]
    public AudioChannelType channelType = AudioChannelType.Master;

    [Header("Bieżąca wartość")]
    [Range(0f, 1f)] public float localVolume = 1.0f;

    [SerializeField] private Slider slider;
    [SerializeField] private AudioManager audioManager;

    private void Awake()
    {
        EnsureReferences();
    }

    private void OnEnable()
    {
        EnsureReferences();
        SyncSliderFromSettings();
    }

    private void Start()
    {
        EnsureReferences();
        SyncSliderFromSettings();

        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private void OnDestroy()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }

    private void EnsureReferences()
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }

        if (audioManager == null)
        {
            audioManager = AudioManager.Instance != null ? AudioManager.Instance : FindFirstObjectByType<AudioManager>();
        }
    }

    /// <summary>
    /// Synchronizuje pozycję suwaka z aktualnymi ustawieniami w SettingsSystem.
    /// </summary>
    public void SyncSliderFromSettings()
    {
        EnsureReferences();
        if (slider == null) return;

        float val = 1.0f;
        switch (channelType)
        {
            case AudioChannelType.Master:
                val = SettingsSystem.currentSettings.masterVolume;
                break;
            case AudioChannelType.Music:
                val = SettingsSystem.currentSettings.musicVolume;
                break;
            case AudioChannelType.SFX:
                val = SettingsSystem.currentSettings.sfxVolume;
                break;
            case AudioChannelType.Ambient:
                val = SettingsSystem.currentSettings.ambientVolume;
                break;
        }

        localVolume = val;
        slider.SetValueWithoutNotify(val);
    }

    /// <summary>
    /// Wywoływane natychmiast podczas przesuwania suwaka w menu.
    /// </summary>
    public void OnSliderValueChanged(float value)
    {
        localVolume = value;
        ApplyVolumeLive(value);
    }

    /// <summary>
    /// Wprowadza zmianę głośności na żywo dla natychmiastowego odsłuchu.
    /// </summary>
    public void ApplyVolumeLive(float value)
    {
        EnsureReferences();

        switch (channelType)
        {
            case AudioChannelType.Master:
                if (audioManager != null) audioManager.SetMasterVolume(value);
                else AudioListener.volume = value;
                SettingsSystem.currentSettings.masterVolume = value;
                break;

            case AudioChannelType.Music:
                if (audioManager != null) audioManager.SetMusicVolume(value);
                SettingsSystem.currentSettings.musicVolume = value;
                break;

            case AudioChannelType.SFX:
                if (audioManager != null) audioManager.SetSfxVolume(value);
                SettingsSystem.currentSettings.sfxVolume = value;
                break;

            case AudioChannelType.Ambient:
                if (audioManager != null) audioManager.SetAmbientVolume(value);
                SettingsSystem.currentSettings.ambientVolume = value;
                break;
        }
    }

    /// <summary>
    /// Ustawia wartość suwaka zewnętrznie (np. przy wczytywaniu ustawień).
    /// </summary>
    public void SetSliderValue(float value)
    {
        EnsureReferences();
        localVolume = value;
        if (slider != null)
        {
            slider.SetValueWithoutNotify(value);
        }
    }
}
