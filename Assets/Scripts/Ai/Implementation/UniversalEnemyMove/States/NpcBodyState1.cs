using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBodyState1 : NpcMoveStateOverride
{
    public override void Enter()
    {
        SetDebugDisplay();
        //SetChild(DefaultState,true);
        //Debug.Log("skibo");
    }
    public override void Do()
    {
        //Debug.Log("i am grounded yo");
        //Change(DoAfter);


        /*if(brain.wantJump)
        {
            Change(JumpState);
        }*/
    }
    public override void FixedDo()
    {

    }
    public override void Exit()
    {

    }
}
