using UnityEngine;
using UnityEngine.UI;

public class volumeSlider : MonoBehaviour
{
    public float localVolume;
    [SerializeField] private Slider slider;
    [SerializeField] private AudioManager audioManager;

    private void Awake()
    {
        EnsureReferences();
    }

    private void OnEnable()
    {
        EnsureReferences();
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

        if (slider != null)
        {
            if (audioManager != null)
            {
                slider.value = audioManager.GetVolume();
            }
            else
            {
                slider.value = localVolume;
            }
        }
        else
        {
            Debug.LogWarning($"{name}: Slider reference not found.", this);
        }
    }

    private void Update()
    {
        if (slider == null)
        {
            return;
        }

        if (slider.value != localVolume)
        {
            localVolume = slider.value;
        }
    }

    public void SetSliderValue(float value)
    {
        EnsureReferences();

        if (slider == null)
        {
            Debug.LogWarning($"{name}: Cannot set slider value because Slider is missing.", this);
            return;
        }

        slider.value = value;
        localVolume = value;
    }
}
