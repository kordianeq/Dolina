using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DamagableMesh : MonoBehaviour, IKickeable, IDamagable
{

    int damageSteps = 0;
    int damageMeshesTotal;

    void Awake()
    {
        
        foreach(Transform child in transform)
        {
            damageSteps++;
        }
        damageMeshesTotal = damageSteps;

       
        if(damageSteps == 0)
        {
            Debug.LogError("No destruction meshes assgined, damaging mesh "+ gameObject.name + " will not result in swaping mesh");
            return;
        }
        MeshSwap(0);

    }
    public void HandleDamage()
    {
        if(damageSteps < 0) return;

        damageSteps--;
        MeshSwap(damageMeshesTotal - damageSteps );
        

    }
    public void MeshSwap(int idObjToShow)
    {
        int i =0;
        foreach(Transform child in transform)
        {
            if(i == idObjToShow) child.gameObject.SetActive(true);
            else child.gameObject.SetActive(false);

            i++;
        }

    }

    
    public void KickHandle()
    {
        HandleDamage();
    }

    public void Damaged(float damage)
    {
        HandleDamage();
    }
}
