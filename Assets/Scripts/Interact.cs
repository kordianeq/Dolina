using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interact : MonoBehaviour
{
    UiMenager canvas;
    bool isInArea, isLookingAt;

    [Header("Interaction settings")]
    public bool overrideInteractText;
    public string customInteractText;
    //input do zmiany przydal by sie jakis gameobj na to 
    public KeyCode interact = KeyCode.E;
    void Start()
    {
        canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<UiMenager>();   
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(interact) && isInArea)
        {
            if(this.gameObject.TryGetComponent<IInteracted>(out IInteracted interacion))
            {
                interacion.NewInteraction();
            }
        }
        
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInArea = true;
            if (overrideInteractText)
            {
                canvas.interactText.text = customInteractText;
            }
            
            canvas.interactPanel.gameObject.SetActive(true);

            
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInArea = false;
            canvas.interactPanel.gameObject.SetActive(false);

        }
    }

    public void RayCastLookAt()
    {
        //canvas.interactPanel.gameObject.SetActive(true);

        if (Input.GetKeyDown(interact))
        {
            if (this.gameObject.TryGetComponent<IInteracted>(out IInteracted interacion))
            {
                interacion.NewInteraction();
            }
        }


    }
}
