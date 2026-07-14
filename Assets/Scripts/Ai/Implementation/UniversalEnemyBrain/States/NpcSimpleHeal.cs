using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcSimpleHeal : NpcBehaviorStateOvveride
{

    public float waitTime;
    public State DoAfter;
    public float HealPower;
    

    public override void Enter()
    {
        ForceStateAnim();
    }
    public override void Do()
    {
        if (time >= waitTime)
        {
            isComplete = true;
            brain.mainCore.dmgMannager.EnemyHp += HealPower;
            Change(DoAfter);
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
