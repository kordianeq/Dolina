using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyHurtBox : HurtBox, IDamagable
{
    [SerializeField] public PropPart bodyPart;
    public DmgMannager dmgMannager;
    void Awake()
    {
        /*if (bodyPart == null)
        { 

        }*/
        dmgMannager = ParentRecreceiver.GetComponent<DmgMannager>();
    }
    public void Damaged(float dmg)
    {
        Debug.Log("Bullet hit: " + gameObject.name);
        dmgMannager.Receive(bodyPart,dmg,-1f,Vector3.zero);
        //dmgMannager.Damaged(dmg);
    }

    public bool Damaged(float dmg, Vector3 dir, float force)
    {
        Debug.Log("Bullet hit with koncback: " + gameObject.name);
        //dmgMannager.Damaged(dmg, dir, force);
        dmgMannager.Receive(bodyPart,dmg,force,dir);
        return true;
    }
}
