using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrunkDeathResurect : NpcBehaviorStateOvveride
{
    //public NavMeshAgent nav;
    State DoOnResurect;
    public float Deadimer;
    public float resurectTimer;
    public string resurAnim;
    private void Start()
    {

    }
    public override void Enter()
    {
        SetDebugDisplay();
        ForceStateAnim();
       
    }
    public override void Do()
    {
       
    }
    public override void FixedDo()
    {

        if (time >= Deadimer)
        {
            
            ForceStateAnim(resurAnim);
            if (time >= resurectTimer)
            {
                brain.mainCore.dmgMannager.EnemyHp += 100;
                brain.ForceResurectState();
            }
            
            //Change(DoOnResurect);
        }
        //core.eBody.AddForce(Vector3.up * 100f);
    }
    public override void Exit()
    {
        //brain.moveBrain.rotationOverrid = false;
        //isComplete = false;
        //Debug.Log("StoppedRanged");
    }

    

    void OnDrawGizmosSelected()
    {
       /* if (!UnityEditor.Selection.Contains(gameObject)) { return; }
        // Display the explosion radius when selected
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxRange);
        Gizmos.DrawWireSphere(transform.position, minRange);
        Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(transform.position, OptimalRange);
        Gizmos.DrawWireSphere(transform.position, maxRange - OptimalRange + offset);
        Gizmos.DrawWireSphere(transform.position, minRange + OptimalRange + offset);
        //Gizmos.DrawLine(transform.position, patrolPoint[patrolIndex].position);*/
    }
}
