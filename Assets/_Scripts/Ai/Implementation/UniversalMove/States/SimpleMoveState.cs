using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class SimpleMoveState : MoveStateOverride
{
    
    [Header("Move stats, make plyer go brrrr")]
    [SerializeField] private float speed;
    [SerializeField] private float drag;

    [Header("higher layer state, plz be coutious")]
    public State JumpState;

    public State DoAfter;
    public override void Enter()
    {

    }
    public override void Do()
    {
        //Debug.Log("i am moving yo");
        //Change(DoAfter);
        
        
        
        
        
        
        
        
        
        if(brain.moveVector != Vector3.zero)
        {
           // Debug.Log("IhaveInput");
        }else
        {
            //Debug.Log("no input");
        }

    }
    public override void FixedDo()
    {
        if(brain.wantJump)
        {
            //plz fix this abomination later
            if(JumpState!= null)
            {brain.ForceMasterState(JumpState);}
            //MasterSet(JumpState);
        }

        brain.MoveCharacter(speed,drag);
    }
    public override void Exit()
    {

    }

    // new things .................................

    

    
}
