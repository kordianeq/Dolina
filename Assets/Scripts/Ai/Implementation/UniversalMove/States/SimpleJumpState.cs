using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleJumpState : MoveStateOverride
{
    
    [Header("Jump stats")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpTime;
    [SerializeField] private float jumpTimer;
    

    [Header("Same Layer states")]
    public State DoAfter;
    [Header("children State")]
    public State DefaultState;
    public override void Enter()
    {
        brain.ClearVerticalVelo();
        Debug.Log("i am jumpingg");
        jumpTimer = jumpTime;
        brain.SetGravity(true);
        brain.AddDirectionalImpulseForce(Vector3.up,jumpForce);
        SetChild(DefaultState);
        //SetChild(DefaultState);
    }
    public override void Do()
    {
        Debug.Log("i goung up");
        //Change(DoAfter);
        
        jumpTimer-=Time.deltaTime;
        
        
        if(jumpTimer<=0)
        {
            Change(DoAfter);
        }

    }
    public override void FixedDo()
    {
        
    }
    public override void Exit()
    {

    }
}
