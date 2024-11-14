using System.Collections;
using System.Collections.Generic;

using UnityEditor.Rendering;
using UnityEngine;

public class Mod_Rigidbody_Destination : Mod_State
{
    //public float distance;
    public float TurnSpeed;
    public float randomMidRange;
    public float range;
    public override void Enter()
    {
        goal = Goal.None;
        range = Random.Range(randomMidRange, minRange);
        core.eAnimator.SetTrigger("Walk");
        Debug.Log("Entered Destination MovementMode");
    }
    public override void Do()
    {
        Vector3 lookDir = Vector3.RotateTowards(core.visualBody.transform.forward, target - core.transform.position, TurnSpeed * Time.deltaTime, 0.0f);
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
        {//Debug.Log("outside");
            goal = Goal.Fail;
            isComplete = true;
        }
        else if (range < dist)
        {
            goal = Goal.Succes;
            core.rBody.AddForce(((target - core.transform.position).normalized * speed) - core.rBody.velocity, ForceMode.Acceleration);
        }
        else
        {
            // Debug.Log("inrange");
            isComplete = true;
        }

        //core.eBody.AddForce(Vector3.up * 100f);
    }
    public override void Exit()
    {
        isComplete = false;
        Debug.Log("ReachedTarget");
    }
}
