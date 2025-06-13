
using UnityEngine;
using UnityEngine.AI;

public class NpcFindTryState : NpcBehaviorStateOvveride
{
    [Header("weypointDetection Range")]
    [SerializeField] private float maxRange;
    [SerializeField] private float minRange;

    Vector3 navVect = Vector3.zero;
    [SerializeField] private float rotationSpeed;

    [Header("same level states")]
    public State ReachedTarget;
    public State LostTarget;

    [Header("childState")]
    public State chill;

    //[Header("do when passive")]   
    //public State patrolType;

    private void Start()
    {

    }
    public override void Enter()
    {
        SetDebugDisplay();
        //if (patrolType != null) { SetChild(patrolType); }
        //Debug.Log("Chasing yo");
        if (chill != null) { SetChild(chill); }

    }
    public override void Do()
    {
        //Debug.Log("i am moving yo");
        //Change(DoAfter);            
        //brain.moveBrain.rotationOverrid = true;
        brain.mainCore.CalculateDesiredRotation((brain.target.position - core.transform.position).normalized, rotationSpeed, true);
        //brain.moveBrain.SetDesiredMovementRotation((moveTarget.position-core.transform.position).normalized, rotationSpeed);
    }
    public override void FixedDo()
    {
        //Debug.Log("Patroling yo");
        float dist = Vector3.Distance(brain.target.position, core.transform.position);
        //Debug.Log(dist);

        if (maxRange < dist)
        {//Debug.Log("outside"); lost target
        Debug.Log("lost target out of rangeee");
            brain.target.lost = true;
            Change(LostTarget);
        }
        else if (minRange < dist) // going towards target
        {
            if(brain.target.inLineOfSight)
            {
                Debug.Log("Found in line of sight");
                Change(ReachedTarget);
            }

            navVect = NavCalc();
            brain.mainCore.SetMoveVector(navVect);
            //core.rBody.AddForce((navVect * speed) - core.rBody.velocity, ForceMode.Acceleration);
        }
        else // reached target
        {
            Debug.Log("kind of reached target");
            // Debug.Log("inrange");
            if (!brain.target.inLineOfSight)
            {
                Debug.Log("target not present");
                brain.target.lost = true;
                Change(LostTarget);
            }
            else
            {
                Debug.Log("fuckkkk");
                Change(ReachedTarget);
            }
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
        //brain.moveBrain.rotationOverrid = false;
    }

    Vector3 NavCalc()
    {
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, brain.target.position, NavMesh.AllAreas, path);
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
