using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class sliderScript : MonoBehaviour
{
    // Start is called before the first frame update
    public float value;
    public float min;
    public float max;

    Slider sliderRef;

    void Start()
    {
        sliderRef = gameObject.GetComponent<Slider>();
        sliderRef.value = value;
        sliderRef.minValue = min;
        sliderRef.maxValue = max;
    }

    // Update is called once per frame
    void Update()
    {
        sliderRef.value = value;
    }
}
