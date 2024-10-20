using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.AI;

public class NavDestinationSlide : Mod_State
{
    Vector3 navVect = Vector3.zero;
    //public NavMeshAgent nav;
    public float TurnSpeed;
    public float randomMidRange;
    float range;
    public override void Enter()
    {
        goal = Goal.None;
        range = Random.Range(randomMidRange, minRange);
        core.eAnimator.SetTrigger("Walk");
        //Debug.Log("Entered Destination MovementMode");
    }
    public override void Do()
    {
        Vector3 targetLook = target - core.transform.position;
        targetLook = new Vector3(targetLook.x,target.y,targetLook.z);
        Vector3 lookDir = Vector3.RotateTowards(core.visualBody.transform.forward, targetLook, TurnSpeed * Time.deltaTime, 0.0f);
        //lookDir = new Vector3(lookDir.x,0,lookDir.y);
        core.visualBody.transform.rotation = Quaternion.LookRotation(lookDir);
        
        // core.transform.rotation = Quaternion.RotateTowards(core.transform.position, target,TurnSpeed*Time.deltaTime);
        //core.transform.rotation = Quaternion.Slerp(transform.rotation, secondTransform.rotation, rotationSpeed * Time.deltaTime);
        //rotate toward target
    }
    public override void FixedDo()
    {
        //Debug.Log("go brrrrrrrrrrrrrrrr towards");
        float dist = Vector3.Distance(target, core.transform.position);
        if (maxRange < dist)
        {//Debug.Log("outside"); lost target
            goal = Goal.Fail;
            isComplete = true;
        }
        else if (range < dist) // going towards target
        {
            goal = Goal.Succes;


            navVect = NavCalc();
            core.rBody.AddForce((navVect * speed) - core.rBody.velocity, ForceMode.Acceleration);
        }
        else // reached target
        {
            // Debug.Log("inrange");
            isComplete = true;
        }

        //core.eBody.AddForce(Vector3.up * 100f);
    }
    public override void Exit()
    {
        isComplete = false;
        //Debug.Log("ReachedTarget");
    }

    Vector3 NavCalc()
    {
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);
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
