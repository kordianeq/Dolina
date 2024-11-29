using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddSwietosc : MonoBehaviour, IInteracted
{
    //Tymczasowe rozwi�zanie, potem trzeba zrobi� skrypt na staty i tam da� swietos�
    LookController swietoscObj;

    public int swietoscAdded;
    // Start is called before the first frame update
    void Start()
    {
        swietoscObj = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<LookController>();
    }


    public void NewInteraction()
    {
        Debug.Log("InteractionSuccesful");
        if(swietoscObj.swietosc + swietoscAdded <= 100)
        {
            swietoscObj.swietosc = swietoscObj.swietosc + swietoscAdded;
        }
        else
        {
            swietoscObj.swietosc = swietoscObj.swietosc + (100 - swietoscObj.swietosc);
            Debug.Log("Max swietosc");
        }
        
    }
}
