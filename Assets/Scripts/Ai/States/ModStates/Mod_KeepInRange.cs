using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class Mod_KeepInRange : Mod_State
{
    public float TurnSpeed;
    public float OptimalRange;
    public override void Enter()
    {
        goal = Goal.None;
        Debug.Log("Entered RangeMOvementType");
        core.eAnimator.SetTrigger("Walk");
    }
    public override void Do()
    {
        Vector3 lookDir = Vector3.RotateTowards(core.visualBody.transform.forward,target - core.transform.position,TurnSpeed*Time.deltaTime,0.0f);
        core.visualBody.transform.rotation = Quaternion.LookRotation(lookDir);
    }
    public override void FixedDo()
    {
        Debug.Log("keepingDisatance");
        float eDist = Vector3.Distance(target, core.transform.position);
        float middleDist = (minRange+maxRange)/2;
        if(maxRange > eDist && minRange < eDist)
        {Debug.Log("inWeaponRange");
            if(middleDist>eDist)
            {
                core.rBody.AddForce(((target - core.transform.position).normalized * -speed)-core.rBody.velocity,ForceMode.Acceleration);
            }else
            {
                core.rBody.AddForce(((target - core.transform.position).normalized * speed)-core.rBody.velocity,ForceMode.Acceleration);
            }
        }else
        {Debug.Log("outsideWeaponRange");
            if(maxRange > eDist)
            {
                goal = Goal.Succes;//go in mele
            }else
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
        Debug.Log("StoppedRanged");
    }
}
