using System;
using UnityEngine;

public class RotateSpriteIn3d : MonoBehaviour
{

    Camera cam;
    float angle;
    [SerializeField] GameObject front;

    [SerializeField] SpriteRenderer[] images;

    void Start()
    {
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
    }


    void Update()
    {
        transform.LookAt(transform.position + cam.transform.forward);
        angle = transform.rotation.eulerAngles.y;
       
        // mozna dodac wiecewj segmentow ( uwaga tylko w osi y ( nie dziala z gory i z dolu))
        switch (angle)
        {
            case float n when (n <= 360 && n >= 270):
                Debug.Log("Img 3");
                break;

            case float n when (n < 270 && n >= 180 ):
                Debug.Log("Img 2");
                break;

            case float n when (n < 180 && n >= 90):

                Debug.Log("Img 1");
                break;

            case float n when (n < 90):
                Debug.Log("Img 0");
                break;

            default:
                Debug.Log(angle);
                return;
        }
    }
}
