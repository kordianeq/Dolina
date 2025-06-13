using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBodyState : MoveStateOverride
{
    
    [Header("Hover stats, makes player bouncy")]
    [SerializeField] private float height;
    [SerializeField] private float injuryTreshold;
    [SerializeField] private bool switching;

    [Header("Same Layer states")]
    public State DoAfter;

    [Header("ChildrenStates")]
    public State DefaultState;

    
    public override void Enter()
    {
        SetChild(DefaultState);
        brain.SetHeight(new Vector3(0,height,0));
    }
    public override void Do()
    {
        //Debug.Log("i am grounded yo");
        //Change(DoAfter);
                
        if(switching)
        {
            switching = false;
            Change(DoAfter);
        }

        /*if(brain.wantJump)
        {
            Change(JumpState);
        }*/


    }
    public override void FixedDo()
    {
        //brain.Bounce(hoverHeight,spring,damp);
    }
    public override void Exit()
    {

    }
}
