using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcDetectionState : NpcBehaviorStateOvveride
{
[Header("Move stats, make plyer go brrrr")]
    [SerializeField] private float detectionRange;  
    [SerializeField] private Transform target;
    [Header("state when detecting target")]   
    public State DoAfter;
    [Header("do when passive")]   
    public State patrolType;
    
    private void Awake() {
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }
    public override void Enter()
    {
        //Debug.Log("my child: "+ childState);
        if (patrolType != null) 
        { 
            //Debug.Log("InitPatrol");
            SetChild(patrolType); 
        }
        //Debug.Log("aaaaa child: "+ childState);
    }
    public override void Do()
    {
        //Debug.Log("=hild: "+ childState);
        //Debug.Log("i am moving yo");
        //Change(DoAfter);
        //Debug.Log("my child: "+ childState);    

    }
    public override void FixedDo()
    {

        if (Vector3.Distance(core.transform.position, target.position) < detectionRange || brain.IsAgroed())
        {
            //Debug.Log("DETECTED");
            Change(DoAfter);
        }
       /* if (brain.wantJump)
        {
            //plz fix this abomination later
            if (JumpState != null)
            { brain.ForceMasterState(JumpState); }
            //MasterSet(JumpState);
        }
        

        brain.MoveCharacter(speed, drag);*/
    }
    public override void Exit()
    {

    }

    void OnDrawGizmosSelected()
    {

        if (!UnityEditor.Selection.Contains(gameObject)) { return; }
        // Display the explosion radius when selected
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

    }
}
