using System.Collections;
using System.Collections.Generic;
//using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class NpcSimplePatrol : NpcBehaviorStateOvveride
{
    [Header("weypointDetection Range")]
    [SerializeField] private float maxRange;  
    [SerializeField] private float minRange;  
    [SerializeField] private Transform[] moveTarget;
    Vector3 navVect = Vector3.zero;
    public int patrolIndex = 0;
    
    [Header("same level states")]   
    public State ReachedTarget;
    public State LostTarget;
    //[Header("do when passive")]   
    //public State patrolType;
    
    private void Awake() {
        
    }
    public override void Enter()
    {
        SetDebugDisplay();
        //if (patrolType != null) { SetChild(patrolType); }
        //Debug.Log("Patroling yo");
    }
    public override void Do()
    {
        //Debug.Log("i am moving yo");
        //Change(DoAfter);
              

    }
    public override void FixedDo()
    {
        //Debug.Log("Patroling yo");
        if(moveTarget.Length>0)
        {float dist = Vector3.Distance(moveTarget[patrolIndex].position, core.transform.position);
        //Debug.Log(dist);
        if (maxRange < dist)
        {//Debug.Log("outside"); lost target

            Change(LostTarget);
        }
        else if (minRange < dist) // going towards target
        {
  
            navVect = NavCalc();
            brain.mainCore.SetMoveVector(navVect);
            //core.rBody.AddForce((navVect * speed) - core.rBody.velocity, ForceMode.Acceleration);
        }
        else // reached target
        {
            // Debug.Log("inrange");
            Change(ReachedTarget);

        }
        }else
        {
            Debug.Log("NoPatrolPoints");
            Change(LostTarget);
        }
       /* if (brain.wantJump)
        {
            //plz fix this abomination later
            if (JumpState != null)
            { brain.ForceMasterState(JumpState); }
            //MasterSet(JumpState);
        }
        

        brain.MoveCharacter(speed, drag);*/
    }
    public override void Exit()
    {
        ///Debug.Log("SUCCCESSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS");
        isComplete = false;
        if (patrolIndex++ >= moveTarget.Length - 1)
        {
            patrolIndex = 0;

        }
    }

    Vector3 NavCalc()
    {
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, moveTarget[patrolIndex].position, NavMesh.AllAreas, path);
        if (Vector3.Distance(this.transform.parent.position, this.transform.position) > 1)
        {
            // nav.Warp(this.transform.parent.position);
        }
        //ag.Warp(this.transform.parent.position);
        //nav.destination = target;
        //Debug.Log(ag.path.corners[0]);

        if (path.corners.Length > 1)
        {
            // Debug.Log(nav.path.corners[1] - transform.position);
            Debug.DrawRay(path.corners[1], Vector3.up);
            Debug.DrawRay(transform.position, (path.corners[1] - transform.position).normalized);
            return (path.corners[1] - transform.position).normalized;
        }
        return Vector3.zero;
    }

    void OnDrawGizmosSelected()
    {
        if (!UnityEditor.Selection.Contains(gameObject)) { return; }
        // Display the explosion radius when selected
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxRange);

        Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(transform.position, OptimalRange);
        Gizmos.DrawWireSphere(transform.position, minRange);

        //Gizmos.DrawLine(transform.position, patrolPoint[patrolIndex].position);
    }
}
