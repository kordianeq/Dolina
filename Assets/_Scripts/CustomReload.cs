using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomReload : MonoBehaviour, IReload
{
    GunSystem gun;
    [Header("Inputs")]
    public KeyCode reloadButton = KeyCode.R;
    public KeyCode stateSwitch = KeyCode.Mouse2;

    bool rPressed,canBeOpen,canBeClosed, chamberUnlocked, chamberLoaded;
    int rCount = 0;
    void Start()
    {
        gun = GetComponent<GunSystem>();
    }
    public void Reloaded()
    {
        Debug.Log("ReloadStart");
        canBeOpen = true;
        chamberUnlocked = false;
        chamberLoaded = false;
    }
    void Update()
    {
        if (Input.GetKeyUp(stateSwitch) && canBeOpen)
        {
            canBeOpen = false;
            chamberUnlocked = true;
            Debug.Log("chamberUnlocked");
        }
        if (chamberUnlocked && chamberLoaded == false)
        {
            canBeOpen = false;
            
            LoadingChamber();
        }
        if (Input.GetKeyUp(stateSwitch) && canBeClosed && chamberLoaded)
        {
            
            chamberUnlocked = false;
            
            Debug.Log("chamberClosed");
            gun.ReloadFinished();
        }
    }

    void LoadingChamber()
    {
        if (Input.GetKeyUp(reloadButton))
        {
           rCount++;
            if(rCount == 2)
            {
                canBeClosed = true;
                chamberLoaded = true;
                Debug.Log("Chamber Loaded");
                rCount = 0;
            }
        }
        //else
        //{
        //    Invoke(nameof(DoubleClickReset), 1);
        //}
    }

    void DoubleClickReset()
    {
        rPressed = false;
    }
}
