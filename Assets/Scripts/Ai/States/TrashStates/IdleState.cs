using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
    public override void Enter()
    {
        Debug.Log("Starting to vibe");
    }
    public override void Do()
    {}
    public override void FixedDo()
    {
        Debug.Log("..Vibing..");
        //core.eBody.AddForce(Vector3.up * 100f);
    }
    public override void Exit(){}
}
