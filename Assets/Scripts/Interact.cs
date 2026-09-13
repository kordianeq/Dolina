using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interact : MonoBehaviour
{
    UiMenager uiManager;
    bool isInArea, isLookingAt;

    GameManager gameManager;
    [Header("Interaction settings")]
    public bool overrideInteractText;
    public string customInteractText;
    //input do zmiany przydal by sie jakis gameobj na to 
    public KeyCode interact = KeyCode.E;
    
    void Awake()
    {
        gameManager = GameManager.Instance;
        uiManager = GameManager.Instance.UiMenager;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager != null && (gameManager.isShopping || gameManager.State == PlayerState.Locked))
        {
            if (uiManager != null && uiManager.interactPanel != null && uiManager.interactPanel.gameObject.activeSelf)
            {
                uiManager.interactPanel.gameObject.SetActive(false);
            }
            return;
        }

        if (Input.GetButtonDown("Interact") && isInArea)
        {
            if (this.gameObject.TryGetComponent<IInteracted>(out IInteracted interacion))
            {
                if (uiManager != null && uiManager.interactPanel != null)
                {
                    uiManager.interactPanel.gameObject.SetActive(false);
                }
                interacion.NewInteraction();
            }
        }
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         isInArea = true;
    //         if (gameManager != null && (gameManager.isShopping || gameManager.State == PlayerState.Locked))
    //         {
    //             return;
    //         }

    //         if (overrideInteractText)
    //         {
    //             uiManager.interactText.text = customInteractText;
    //         }

    //         uiManager.interactPanel.gameObject.SetActive(true);
    //     }
    // }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInArea = false;
            uiManager.interactPanel.gameObject.SetActive(false);
        }
    }

    public void RayCastLookAt()
    {
        if (gameManager != null && (gameManager.isShopping || gameManager.State == PlayerState.Locked))
        {
            if (uiManager != null && uiManager.interactPanel != null && uiManager.interactPanel.gameObject.activeSelf)
            {
                uiManager.interactPanel.gameObject.SetActive(false);
            }
            return;
        }

        if (overrideInteractText)
        {
            uiManager.interactText.text = customInteractText;
        }
        uiManager.interactPanel.gameObject.SetActive(true);

        if (Input.GetKeyDown(interact))
        {
            if (this.gameObject.TryGetComponent<IInteracted>(out IInteracted interacion))
            {
                if (uiManager != null && uiManager.interactPanel != null)
                {
                    uiManager.interactPanel.gameObject.SetActive(false);
                }
                interacion.NewInteraction();
            }
        }
    }
    public void DistableUi(){
        uiManager.interactPanel.gameObject.SetActive(false);
    }
}
