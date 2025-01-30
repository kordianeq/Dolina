using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcRandomWander : NpcBehaviorStateOvveride
{
    [Header("weypointDetection Range")]
  
    [SerializeField] private float rotationSpeed;

    
    [Header("same level states")]   
    public State AfterHit;


    [Header("childState")]   
    public State chill;

    //[Header("do when passive")]   
    //public State patrolType;
    private void Start() {

    }
    private void Awake() {

    }
    public override void Enter()
    {

        //if (patrolType != null) { SetChild(patrolType); }

        //if(chill!=null){SetChild(chill);}

    }
    public override void Do()
    {
       //if (time >= hitTime)
 

        //Debug.Log("i am moving yo");
        //Change(DoAfter);            
        //brain.moveBrain.rotationOverrid = true;
        brain.mainCore.CalculateDesiredRotation((brain.target.position-core.transform.position).normalized, rotationSpeed,true);
        //brain.moveBrain.SetDesiredMovementRotation((moveTarget.position-core.transform.position).normalized, rotationSpeed);
    }
    public override void FixedDo()
    {
         //brain.mainCore.SetMoveVector(new Vector3(0,0,0));
            // Debug.Log("inrange");
            //Change(AfterHit);

        
    }
    public override void Exit()
    {

    }
}
