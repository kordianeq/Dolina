using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PatrollState : State
{

    public Transform target;
    public Transform[] patrolPoint;
    public Mod_State MovementType;
    public State DoInRange;
    public float detectionRange;

    public int patrolIndex = 0;

    private void Awake()
    {
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    public override void Enter()
    {
        //patrolIndex = 0;
        //Debug.Log("InPatrolMode");
        SetChild(MovementType);
        MovementType.target = patrolPoint[patrolIndex].position;
    }
    public override void Do()
    {

    }
    public override void FixedDo()
    {
        //when leaf state is compleate select new state (example: between movement and idle)
        if (childState.isComplete)
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

        if (Vector3.Distance(core.transform.position, target.position) < detectionRange)
        {
            isComplete = true;
            //Debug.Log("!!!!!!!!!!!!fOUND TARGET!!!!!!!!");
        }
        //Debug.Log(patrolPoint.Length);



        //Debug.Log("TargetDir");
        //childState.FixedDo();

        //Debug.Log(MovementType.direction);
    }
    public override void Exit() { }


    //only debug, show variable lengths and stuff
    void OnDrawGizmosSelected()
    {
        if (patrolPoint.Length > 0)
        {
            if (!UnityEditor.Selection.Contains(gameObject)) { return; }
            // Display the explosion radius when selected
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            Gizmos.DrawLine(transform.position, patrolPoint[patrolIndex].position);
        }
    }
}
