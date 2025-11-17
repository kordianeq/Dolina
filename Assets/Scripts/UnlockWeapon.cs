using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnlockable
{
    void Unlock();
}
public class UnlockWeapon : MonoBehaviour, IUnlockable
{
    public GameObject objToUnlock;

    void Awake()
    {
       
    }
    public void Unlock()
    {
        //Debug.Log("Unlocking weapon: " + objToUnlock.name);
        if (objToUnlock.activeInHierarchy == false)
        {
            objToUnlock.SetActive(true);
        }
        
    }
}
