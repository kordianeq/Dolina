using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleWaitState : State
{
    public float waitTime;
    public string animName;
    public override void Enter()
    {
        core.eAnimator.SetTrigger(animName);
        isComplete = false;
        //Debug.Log("Starting to vibe");
    }
    public override void Do()
    {
        //Debug.Log("vibing time: "+time);
        if(time >= waitTime)
        {
            isComplete = true;
            //Debug.Log("waitStop");
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
