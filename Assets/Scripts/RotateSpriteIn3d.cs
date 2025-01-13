using System;
using System.Collections.Generic;
using UnityEngine;

public class RotateSpriteIn3d : MonoBehaviour
{

    Camera cam;
    float angle;
    
    int spriteToUnhide = 0;
    [SerializeField]List<Sprite> images;
    SpriteRenderer spriteRenderer;
    void Start()
    {
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        SelectSprite(images[0]);
    }


    void Update()
    {
        transform.LookAt(transform.position + cam.transform.forward);
        angle = transform.rotation.eulerAngles.y;


        // mozna dodac wiecej segmentow ( uwaga tylko w osi y ( nie dziala z gory i z dolu))
      
        switch (angle)
        {
            case float n when (n <= 360 && n >= 270):
                SelectSprite(images[3]);
                
                //Debug.Log("Img 3");
                break;

            case float n when (n < 270 && n >= 180 ):
                SelectSprite(images[2]);
                //Debug.Log("Img 2");
                break;

            case float n when (n < 180 && n >= 90):
                SelectSprite(images[1]);
                //Debug.Log("Img 1");
                break;

            case float n when (n < 90):
                SelectSprite(images[0]);
                //Debug.Log("Img 0");
                break;
                
            default:
                //Debug.Log(angle);
                return;
        }
        

        
    }

    void SelectSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }

}
