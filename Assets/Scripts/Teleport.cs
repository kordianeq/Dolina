using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour, IInteracted
{
    public GameObject canvas;
    UiMenager menager;
    public bool trigger;
    public int sceneId;
    

    void Start()
    {
        
        menager = canvas.GetComponent<UiMenager>();
    }

    public void NewInteraction()
    {
        if (trigger == false)
        {
            menager.OnChangeScene(sceneId);
        }
            
    }
    private void OnTriggerEnter(Collider other)
    {
        if (trigger)
        {
            menager.OnChangeScene(sceneId);
        }
        
    }
}
