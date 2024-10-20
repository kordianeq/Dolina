using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

public class NavKeepInRange : Mod_State
{
    //public NavMeshAgent nav;
    public float TurnSpeed;
    public float OptimalRange;
    public float offset;
    public override void Enter()
    {
        goal = Goal.None;
        //Debug.Log("Entered RangeMOvementType");
        core.eAnimator.SetTrigger("Walk");
    }
    public override void Do()
    {
        Debug.Log("rotating at");
        Debug.DrawRay(core.visualBody.position, core.visualBody.transform.forward);
        Debug.DrawRay(core.visualBody.position, (target - core.transform.position).normalized,Color.red);
        Vector3 lookDir = Vector3.RotateTowards(core.visualBody.transform.forward, target - core.transform.position, TurnSpeed * Time.deltaTime, 0.0f);
        //lookDir = new Vector3(lookDir.x,0,lookDir.y);
        core.visualBody.transform.rotation = Quaternion.LookRotation(lookDir);
    }
    public override void FixedDo()
    {
        //Debug.Log("keepingDisatance");
        float eDist = Vector3.Distance(target, core.transform.position);
        float middleDist = (minRange + maxRange) / 2;

        if (maxRange > eDist && minRange < eDist)
        {
           // Debug.Log("inWeaponRange");
            core.eAnimator.SetTrigger("Walk");
            if (minRange + OptimalRange + offset > eDist)
            {
                //Debug.Log("back offffffffffffffffffffffffffff");
                core.rBody.AddForce(((target - core.transform.position).normalized * -speed) - core.rBody.velocity, ForceMode.Acceleration);
            }
            else if (maxRange - OptimalRange + offset < eDist)
            {
                core.rBody.AddForce((NavCalc() * speed) - core.rBody.velocity, ForceMode.Acceleration);
            }else
            {
                core.eAnimator.SetTrigger("Idle");
            }

        }
        else
        {
            //Debug.Log("outsideWeaponRange");
            if (maxRange > eDist)
            {
                goal = Goal.Succes;//go in mele
            }
            else
            {
                goal = Goal.Fail;// end agro 
            }
            isComplete = true;
        }

        //core.eBody.AddForce(Vector3.up * 100f);
    }
    public override void Exit()
    {
        isComplete = false;
        //Debug.Log("StoppedRanged");
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
        Gizmos.DrawWireSphere(transform.position, minRange);
        Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(transform.position, OptimalRange);
        Gizmos.DrawWireSphere(transform.position, maxRange - OptimalRange + offset);
        Gizmos.DrawWireSphere(transform.position, minRange + OptimalRange + offset);
        //Gizmos.DrawLine(transform.position, patrolPoint[patrolIndex].position);
    }

}
