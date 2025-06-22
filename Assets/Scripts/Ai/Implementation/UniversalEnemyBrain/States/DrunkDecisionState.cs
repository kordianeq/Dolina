using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrunkDecisionState : NpcBehaviorStateOvveride
{
    public int DRINKcHANCE;
    public float waitTime;
    public State DoAfter;
    public State DoDrink;

    public override void Enter()
    {
        ForceStateAnim();
    }
    public override void Do()
    {
        if (time >= waitTime)
        {
            isComplete = true;
            int choic = Random.Range(0, DRINKcHANCE);
            if (choic == 0)
            {
                Change(DoDrink);
            }
            else
            {
                Change(DoAfter);
            }
            //Debug.Log("waitStop");
            }
    }
    public override void FixedDo()
    {
        brain.mainCore.SetMoveVector(new Vector3(0,0,0));
    }
    public override void Exit()
    {

    }
}

