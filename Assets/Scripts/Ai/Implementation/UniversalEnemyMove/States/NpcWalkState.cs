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

    [SerializeField] private float deAcceleration;
    [Header("higher layer state, plz be coutious")]
    public State JumpState;
    [Header("Same layer state")]
    public State DoAfter;
    public override void Enter()
    {
        brain.mainCore.animator.SetTrigger("Walk");
        //Debug.Log("Skibi");
        SetDebugDisplay();
    }
    public override void Do()
    {
        //Debug.Log("i am moving yo");
        //Change(DoAfter);      

        if (brain.mainCore.GetMoveVector() != Vector3.zero)
        {
            brain.mainCore.CalculateDesiredRotation(brain.mainCore.GetMoveVector(),rotationSpeed,false);
           // brain.SetDesiredMovementRotation(brain.mainCore.GetMoveVector(),rotationSpeed);
            // Debug.Log("IhaveInput");

        }

    }
    public override void FixedDo()
    {
        /*if (brain.wantJump)
        {
            //plz fix this abomination later
            if (JumpState != null)
            { brain.ForceMasterState(JumpState); }
            //MasterSet(JumpState);
        }
        */
        //Debug.Log(brain.moveVector.magnitude);
        if(brain.mainCore.GetMoveVector().magnitude >0)
        {
            brain.MoveCharacter(speed, drag);
           // brain.mainCore.animator.SetBool("NoInput",false);
            
            //brain.mainCore.animator.SetFloat("Blend",1f);


        }else
        {
            //brain.mainCore.animator.SetBool("NoInput",true);
            brain.DeAccelerate(deAcceleration);
        }
        

    }
    public override void Exit()
    {

    }

    // new things .................................




}