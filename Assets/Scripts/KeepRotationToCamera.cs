using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepRotationToCamera : MonoBehaviour
{
    Camera cam;
   
    void Start()
    {
       cam = GameObject.Find("Main Camera").GetComponent<Camera>();
    }


    void Update()
    {
        transform.LookAt(transform.position + cam.transform.forward);
    }
}
