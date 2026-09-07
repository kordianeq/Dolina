using UnityEngine;
using UnityEngine.UI;

public class sensitivitySlider : MonoBehaviour
{
    public float localSensitivity;
    private Slider slider;
    [SerializeField] private CameraControll cameraControll;

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

        if (cameraControll == null && GameManager.Instance != null && GameManager.Instance.PlayerCam != null)
        {
            cameraControll = GameManager.Instance.PlayerCam.GetComponent<CameraControll>();
        }

        if (slider != null)
        {
            slider.maxValue = 5f;
            slider.value = localSensitivity;
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

        if (slider.value != localSensitivity)
        {
            localSensitivity = slider.value;

            if (cameraControll != null)
            {
                cameraControll.AdjustCameraSensitivity(localSensitivity);
            }
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
        localSensitivity = value;
    }
}
