using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SimpleDetectorState : State
{

    [SerializeField] private Transform playerTarget;
    //public Transform[] patrolPoint;
    public float detectionRange;

    //public int patrolIndex = 0;

    [Header("Exit state (when detecting player)")]
    public State AgroState;
   // [Header("nested states")]
   // public State curentState;
    


    private void Awake()
    {
        if (playerTarget == null)
        {
            playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    public override void Enter()
    {
        //patrolIndex = 0;
        //Debug.Log("InPatrolMode");
        //SetChild(MovementType);
        //curentState = branchMachine.state;
        //MovementType.target = patrolPoint[patrolIndex].position;
    }
    public override void Do()
    {
        if (Vector3.Distance(core.transform.position, playerTarget.position) < detectionRange)
        {
            Debug.Log("DETECTEDp");
            isComplete = true;
            Change(AgroState);
        }
        Debug.Log("patrolling");
    }
    public override void FixedDo()
    {
        /*if (curentState != branchMachine.state)
        {
            if (childState == MovementType)
            {
                //increment
                if (patrolIndex++ >= patrolPoint.Length - 1)
                {
                    patrolIndex = 0;

                }
                MovementType.target = patrolPoint[patrolIndex].position;
                //SetChild(DoInRange);

            }
            Debug.Log("Switched");
        }
        MovementType.target = patrolPoint[patrolIndex].position;
        //Debug.Log("CHILLLZ"+childState);
        if (childState.isComplete)
        {
            Debug.Log("plz dont");
            if (childState == MovementType)
            {
                //increment
                if (patrolIndex++ >= patrolPoint.Length - 1)
                {
                    patrolIndex = 0;

                }

                //SetChild(DoInRange);

            }
            else
            {
                //SetChild(MovementType);
            }
        }

        curentState = branchMachine.state;

        //when leaf state is compleate select new state (example: between movement and idle)
        /*if (childState.isComplete)
        {
            if (childState == MovementType)
            {
                //increment
                if (patrolIndex++ >= patrolPoint.Length - 1)
                {
                    patrolIndex = 0;

                }
                MovementType.target = patrolPoint[patrolIndex].position;
                SetChild(DoInRange);

            }
            else
            {
                SetChild(MovementType);
            }
        }
        */

        //Debug.Log(patrolPoint.Length);



        //Debug.Log("TargetDir");
        //childState.FixedDo();

        //Debug.Log(MovementType.direction);
    }
    public override void Exit()
    {

    }


    //only debug, show variable lengths and stuff
    void OnDrawGizmosSelected()
    {
        //if (patrolPoint.Length > 0)
        //{
            if (!UnityEditor.Selection.Contains(gameObject)) { return; }
            // Display the explosion radius when selected
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            //Gizmos.DrawLine(transform.position, patrolPoint[patrolIndex].position);
       // }
    }
}
