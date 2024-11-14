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

    private void Awake()
    {
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    public override void Do()
    {

        Vector3 targetLook = target.position - core.transform.position;
        targetLook = new Vector3(targetLook.x,target.position.y,targetLook.z);
        Vector3 lookDir = Vector3.RotateTowards(core.visualBody.transform.forward, targetLook, TurnSpeed * Time.deltaTime, 0.0f);
        //lookDir = new Vector3(lookDir.x,0,lookDir.y);
        core.visualBody.transform.rotation = Quaternion.LookRotation(lookDir);

        //Debug.Log("vibing time: "+time);
        if (time >= waitTime)
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
