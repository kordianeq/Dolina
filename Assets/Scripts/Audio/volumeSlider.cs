using UnityEngine;
using UnityEngine.UI;

public class volumeSlider : MonoBehaviour
{
   
    public float localVolume;
    Slider slider;
    [SerializeField] AudioManager audioManager;
    // Start is called before the first frame update
    void Start()
    {
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        slider = GetComponent<Slider>();
        
        localVolume = slider.value;
        //audioManager.SetVolume(localVolume);
    }

    // Update is called once per frame
    void Update()
    {
        
        if(slider.value != localVolume)
        {
            localVolume = slider.value;
            //audioManager.SetVolume(localVolume);
        }
        
    }

    public void SetSliderValue(float value)
    {
        slider.value = value;
        localVolume = value;
    }
}
