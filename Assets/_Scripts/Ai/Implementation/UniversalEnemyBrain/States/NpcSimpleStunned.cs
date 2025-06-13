using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcSimpleStunned : NpcBehaviorStateOvveride
{
    [SerializeField] State DoAfter; 
    public override void Enter()
    {
        brain.mainCore.SetStunned(true);
        SetDebugDisplay();
    }
    public override void Do()
    {


    }
    public override void FixedDo()
    {
        if (time >= brain.mainCore.CheckStunnTime() || !brain.mainCore.CheckStunned())
        {
            Change(DoAfter);
        }
    }
    public override void Exit()
    {
        brain.mainCore.SetStunned(true);
    }
}
