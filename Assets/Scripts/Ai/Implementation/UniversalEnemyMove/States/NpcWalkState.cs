using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NpcWalkState : NpcMoveStateOverride
{

    [Header("Move stats, make plyer go brrrr")]
    [SerializeField] private float speed;
    [SerializeField] private float drag;
    [SerializeField] private float rotationSpeed;
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

        if (brain.moveVector != Vector3.zero && !brain.rotationOverrid)
        {

            brain.RotateTowardsVector(brain.moveVector,rotationSpeed);
            // Debug.Log("IhaveInput");

        }

    }
    public override void FixedDo()
    {
        if (brain.wantJump)
        {
            //plz fix this abomination later
            if (JumpState != null)
            { brain.ForceMasterState(JumpState); }
            //MasterSet(JumpState);
        }

        brain.MoveCharacter(speed*brain.velocityMultplier, drag*brain.dragMultiplier);
    }
    public override void Exit()
    {

    }

    // new things .................................




}