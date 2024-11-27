using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBodyState : NpcMoveStateOverride
{
    
    [Header("offsets player body for some reason, idk")]
    [SerializeField] private float offset;
    

    //[Header("higher layer state, plz be coutious")]
    //public State JumpState;

    [Header("Same Layer states")]
    public State DoAfter;

    [Header("ChildrenStates")]
    public State DefaultState;

    
    public override void Enter()
    {
        brain.SetHeight(new Vector3(0,offset,0));
        SetChild(DefaultState);
    }
    public override void Do()
    { 

    }
    public override void FixedDo()
    {
        //Debug.Log(gameObject);
        //brain.Bounce(hoverHeight,spring,damp);
    }
    public override void Exit()
    {

    }

    // new things .................................


    
}
