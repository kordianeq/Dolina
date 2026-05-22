using UnityEngine;
using UnityEngine.UI;

public class sensitivitySlider : MonoBehaviour
{
    
    public float localSensitivity;
    Slider slider;
    [SerializeField] CameraControll cameraControll;
    // Start is called before the first frame update
    void Awake()
    {
        cameraControll = GameManager.Instance.PlayerCam.GetComponent<CameraControll>();
        slider = GetComponent<Slider>();
        slider.maxValue = 5f;

        // przypisz tutaj wartoœæ z PlayerPrefs, jeœli istnieje, lub ustaw domyœln¹ wartoœæ (np. 1f)
        slider.value = localSensitivity;
        //audioManager.SetVolume(localVolume);
    }

    // Update is called once per frame
    void Update()
    {

        if (slider.value != localSensitivity)
        {
            localSensitivity = slider.value;
            //audioManager.SetVolume(localVolume);
            cameraControll.AdjustCameraSensitivity(localSensitivity);
        }

    }

    public void SetSliderValue(float value)
    {
        slider.value = value;
        localSensitivity = value;
    }
}
