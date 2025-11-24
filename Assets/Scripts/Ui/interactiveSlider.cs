using UnityEngine;
using UnityEngine.UI;

public class interactiveSlider : MonoBehaviour
{
    Slider slider;
    public float value, minValue, maxValue;
    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
        slider.value = minValue;
    }

    void Update()
    {
        value = slider.value;

    }

}
