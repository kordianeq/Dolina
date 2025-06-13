using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class GroundedState : MoveStateOverride
{
    
    [Header("Hover stats, makes player bouncy")]
    [SerializeField] private float spring;
    [SerializeField] private float damp;
    [SerializeField] private float hoverHeight;

    [Header("higher layer state, plz be coutious")]
    public State JumpState;

    [Header("Same Layer states")]
    public State DoAfter;

    [Header("ChildrenStates")]
    public State DefaultState;

    
    public override void Enter()
    {
        brain.SetGravity(false);
        SetChild(DefaultState);
    }
    public override void Do()
    {
        //Debug.Log("i am grounded yo");
        //Change(DoAfter);
        
        
        
        
        if(!brain.groundCheck)
        {
            Change(DoAfter);
        }

        /*if(brain.wantJump)
        {
            Change(JumpState);
        }*/


    }
    public override void FixedDo()
    {
        brain.Bounce(hoverHeight,spring,damp);
    }
    public override void Exit()
    {

    }

    // new things .................................

    

    
}
