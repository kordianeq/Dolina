using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcTimedRagdollState : NpcMoveStateOverride
{

    [Header("player is ragdolled")]
    [SerializeField] private float ragdollTime;
    [SerializeField] private float rotationCorectionSpeed;
    [SerializeField] private float decelerationRate;

    [Header("higher layer state, plz be coutious")]
    public State DoAfter;

    Quaternion targetRotation;
    public override void Enter()
    {
        brain.mainCore.SetIncapacitated(true);

        //Debug.Log("Skibi");
        SetDebugDisplay();
        brain.RbRotationConstraints(false);
        brain.SetGravity(true);
        
    }
    public override void Do()
    {
        //Debug.Log(Vector3.Dot(brain.GetMainTransform().up , Vector3.up));
        //brain.DeAccelerate(decelerationRate);
        if (ragdollTime != 0 &&time >= ragdollTime)
        {
            //brain.RbRotationConstraints(true);


            if (Mathf.Abs(Vector3.Dot(brain.GetMainTransform().up , Vector3.up)) < 0.99f)
            {
                targetRotation = Quaternion.Euler(0, brain.GetMainTransform().eulerAngles.y, 0);
                brain.GetMainTransform().rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationCorectionSpeed * Time.deltaTime);
            
            
            }
            else
            {

                Change(DoAfter);
            }

            



        }

        //if (brain.moveVector != Vector3.zero && !brain.rotationOverrid)
        //{

        //brain.RotateTowardsVector(brain.moveVector,rotationSpeed);
        // Debug.Log("IhaveInput");

        //}

    }
    public override void FixedDo()
    {

    }
    public override void Exit()
    {
        brain.mainCore.SetIncapacitated(false);
        brain.RbRotationConstraints(true);
    }
}