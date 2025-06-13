using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcRangedState : NpcBehaviorStateOvveride
{
    [Header("Move stats, make plyer go brrrr")]
    [SerializeField] private GameObject bulletType;
    public Vector3 offset;
    public float firerate, reloadTime;
    public int ammo;

    [Header("Dont")]
    public float fireTimer, relTimer, ammoco;
    [SerializeField] private Transform target;


    private void Awake()
    {
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }
    public override void Enter()
    {
        brain.mainCore.animator.SetTrigger("Draw");
    }
    public override void Do()
    {


    }
    public override void FixedDo()
    {
        if (relTimer >0)
        {
            relTimer -= Time.deltaTime;
        }
        else
        {
            if (fireTimer > 0)
            {
                fireTimer -= Time.deltaTime;
                brain.mainCore.animator.SetBool("Shoot",false);
            }
            else
            {
                brain.mainCore.animator.SetBool("Shoot",true);
                fireTimer = firerate;
                Vector3 a = transform.position + offset;
                Instantiate(bulletType, a, Quaternion.LookRotation(a - target.position));
                ammoco++;
                if (ammoco >= ammo)
                {
                    ammoco = 0;
                    relTimer = reloadTime;
                }
            }
        }
    }
    public override void Exit()
    {

    }
}
