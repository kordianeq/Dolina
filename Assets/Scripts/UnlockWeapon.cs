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
    public void Unlock()
    {
        if (objToUnlock == false)
        {
            objToUnlock.SetActive(true);
        }
        
    }
}
