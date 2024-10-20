using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleLookAtState : State
{
    public float waitTime;
    public string animName;
    public Transform target;
    public float TurnSpeed;
    public override void Enter()
    {
        core.eAnimator.SetTrigger(animName);
        isComplete = false;
        
    }
    public override void Do()
    {

        Vector3 lookDir = Vector3.RotateTowards(core.visualBody.transform.forward,target.position - core.transform.position,TurnSpeed*Time.deltaTime,0.0f);
        
        core.visualBody.transform.rotation = Quaternion.LookRotation(lookDir);

        //Debug.Log("vibing time: "+time);
        if(time >= waitTime)
        {
            isComplete = true;
            
        }
    }
    public override void FixedDo()
    {
        //Debug.Log("..Vibing..");
        //core.eBody.AddForce(Vector3.up * 100f);
    }
    public override void Exit()
    {
        isComplete = false;
    }
}
