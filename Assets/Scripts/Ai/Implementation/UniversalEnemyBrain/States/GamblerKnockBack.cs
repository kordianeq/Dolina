using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamblerKnockBack : NpcBehaviorStateOvveride
{
    [Header("weypointDetection Range")]
    
    


    [SerializeField]float knockbackTime;

[SerializeField]float upforce;
 [SerializeField]float backforce;
    

    [Header("same level states")]
    public State ReachedTarget;
    public State LostTarget;
    //[Header("do when passive")]   
    //public State patrolType;

    private void Awake()
    {
        
    }
    public override void Enter()
    {
        SetDebugDisplay();
        ForceStateAnim();
        brain.moveBrain.AddDirectionalForce(Vector3.up, upforce, ForceMode.Impulse);  
        brain.moveBrain.AddDirectionalForce(-transform.forward, backforce,ForceMode.Impulse);  
        //randomTime = Random.Range(randomTimeMin, randomTimeMax);
        //currentPatrolIndex = (int)Random.Range(randomStepMin, randomStepMax);

        //GetRandomDir();
        //if (patrolType != null) { SetChild(patrolType); }
        //Debug.Log("Patroling yo");
    }
    public override void Do()
    {
        //Debug.Log("i am moving yo");
        //Change(DoAfter);


    }
    public override void FixedDo()
    {
        
        if (time >= knockbackTime)
        { 
            Change(ReachedTarget);
                       
            
        }
        else
        {
            brain.mainCore.SetMoveVector(new Vector3(0, 0, 0));

            //Change(LostTarget);
        }

        }
    public override void Exit()
    {
        ///Debug.Log("SUCCCESSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS");
        isComplete = false;
        //currentPatrolIndex = 0;
        brain.moveBrain.MoveStatsClear();
    }

    

}

