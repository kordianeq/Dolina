using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcMeleState : NpcBehaviorStateOvveride
{
    [Header("weypointDetection Range")]
  
    [SerializeField] private Transform moveTarget;
    [SerializeField] private float hitTime;

    [SerializeField] private float rotationSpeed;
    [SerializeField] GameObject hurtBox;
    
    [Header("same level states")]   
    public State AfterHit;


    [Header("childState")]   
    public State chill;

    //[Header("do when passive")]   
    //public State patrolType;
    private void Start() {
        if (moveTarget == null)
        {
            moveTarget = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }
    private void Awake() {
        hurtBox.SetActive(false);
    }
    public override void Enter()
    {
        hurtBox.SetActive(true);
        //if (patrolType != null) { SetChild(patrolType); }
        Debug.Log("Hitting");
        //if(chill!=null){SetChild(chill);}

    }
    public override void Do()
    {
       if (time >= hitTime)
        {
            isComplete = true;
            //Debug.Log("aaaaaa");
            Change(AfterHit);
            //Debug.Log("waitStop");
        }

        //Debug.Log("i am moving yo");
        //Change(DoAfter);            
        brain.moveBrain.rotationOverrid = true;
        brain.moveBrain.RotateTowardsVector((moveTarget.position-core.transform.position).normalized, rotationSpeed);
    }
    public override void FixedDo()
    {
         brain.moveBrain.SetMoveVector(new Vector3(0,0,0));
            // Debug.Log("inrange");
            //Change(AfterHit);

        
    }
    public override void Exit()
    {
        hurtBox.SetActive(false);
        brain.moveBrain.rotationOverrid = false;
    }
}
