using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : State
{
    public Transform chaseTarget;
    public Mod_State MovementType;
    public override void Enter()
    {
        Debug.Log("Skibidi");
        SetChild(MovementType);
    }
    public override void Do()
    {

    }
    public override void FixedDo()
    {
        Debug.Log("TargetDir");
        MovementType.target = (chaseTarget.position - core.transform.position).normalized;
        //Debug.Log(MovementType.direction);
    }
    public override void Exit(){}
}
