using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NpcRandomWander1 : NpcBehaviorStateOvveride
{
    [Header("weypointDetection Range")]
    [SerializeField] private float maxWalkRange;
    [SerializeField] private float minWalkRange;
    [SerializeField] private Vector3 Target;

    public float speedMulti = 0.5f;
    [SerializeField] private float randomTimeMin;
    [SerializeField] private float randomTimeMax;

    [SerializeField]float randomTime;

    [SerializeField] private float randomStepMin;
    [SerializeField] private float randomStepMax;
    [SerializeField] private int currentPatrolIndex;
    Vector3 navVect = Vector3.zero;
    public int patrolIndex = 0;

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
        randomTime = Random.Range(randomTimeMin, randomTimeMax);
        currentPatrolIndex = (int)Random.Range(randomStepMin, randomStepMax);
        GetRandomDir();
        brain.moveBrain.MoveStatsOverride(speedMulti);
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
        //Debug.Log("Patroling yo");
        if (randomTime > 0)
        {
            if (Vector3.Distance(core.transform.position, Target) > 1f)
            {
                navVect = NavCalc();
                brain.mainCore.SetMoveVector(navVect);
            }
            else
            { 
                brain.mainCore.SetMoveVector(new Vector3(0,0,0));
            }
            
            
            randomTime -= Time.deltaTime; 
        }
        else
        {
            randomTime = Random.Range(randomTimeMin, randomTimeMax);
            GetRandomDir();
            currentPatrolIndex -= 1;
        }


        Debug.DrawRay(transform.position, Target, Color.red);

        if (currentPatrolIndex <= 0)
        {
            Change(ReachedTarget);
        }

            // going towards target   

            //core.rBody.AddForce((navVect * speed) - core.rBody.velocity, ForceMode.Acceleration);            




            //Debug.Log("NoPatrolPoints");
            //Change(LostTarget);

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
        ///Debug.Log("SUCCCESSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS");
        isComplete = false;
        currentPatrolIndex = 0;
        brain.moveBrain.MoveStatsClear();
    }

    Vector3 NavCalc()
    {
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, Target, NavMesh.AllAreas, path);
        if (Vector3.Distance(this.transform.parent.position, this.transform.position) > 1)
        {
            // nav.Warp(this.transform.parent.position);
        }
        //ag.Warp(this.transform.parent.position);
        //nav.destination = target;
        //Debug.Log(ag.path.corners[0]);

        if (path.corners.Length > 1)
        {
            // Debug.Log(nav.path.corners[1] - transform.position);
            Debug.DrawRay(path.corners[1], Vector3.up);
            Debug.DrawRay(transform.position, (path.corners[1] - transform.position).normalized);
            return (path.corners[1] - transform.position).normalized;
        }
        return Vector3.zero;
    }

    void GetRandomDir()
    {
        Debug.Log("ChangingDir");
        Target = Vector3.ProjectOnPlane(Random.onUnitSphere, Vector3.up).normalized;
        float walkRange = Random.Range(minWalkRange, maxWalkRange);
        Target = brain.mainCore.transform.position + Target * walkRange;
    }

    //void OnDrawGizmosSelected()
    //{
    //    if (!UnityEditor.Selection.Contains(gameObject)) { return; }
    //    // Display the explosion radius when selected
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, maxRange);

    //    Gizmos.color = Color.yellow;
    //    //Gizmos.DrawWireSphere(transform.position, OptimalRange);
    //    Gizmos.DrawWireSphere(transform.position, minRange);

    //    //Gizmos.DrawLine(transform.position, patrolPoint[patrolIndex].position);
    //}
}
