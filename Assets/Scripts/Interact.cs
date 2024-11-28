using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interact : MonoBehaviour
{
    UiMenager canvas;

    //input do zmiany przydal by sie jakis gameobj na to 
    public KeyCode interact = KeyCode.E;
    void Start()
    {
        canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<UiMenager>();   
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(interact))
        {
            if(gameObject.TryGetComponent<IInteracted>(out IInteracted interacion))
            {
                interacion.NewInteraction();
            }
        }   
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canvas.interactPanel.gameObject.SetActive(true);
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canvas.interactPanel.gameObject.SetActive(false);

        }
    }
}
