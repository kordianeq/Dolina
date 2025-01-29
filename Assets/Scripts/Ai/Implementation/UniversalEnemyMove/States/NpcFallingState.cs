using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcFallingState : NpcMoveStateOverride
{
    [SerializeField] float maxFallSpeed;
     [SerializeField] float gravityAcceleration;
     [Header("Same layer state")]
    public State DoAfter;
     [Header("ChildrenStates")]
    public State DefaultState;
    public override void Enter()
    {
        
        SetDebugDisplay();
        brain.SetGravity(true);
        SetChild(DefaultState,true);
        //Debug.Log("Not skibo");
    }
    public override void Do()
    {
        //Debug.Log("i am falling");
        //Change(DoAfter);

    }
    public override void FixedDo()
    {

        brain.FallAccelerate(maxFallSpeed,gravityAcceleration);

        if(brain.groundCheck)
        {
            Change(DoAfter);
        }
    }
    public override void Exit()
    {

    }
}
