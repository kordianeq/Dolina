
using UnityEngine;
using UnityEngine.AI;
public class NpcSimpleKeepDistance : NpcBehaviorStateOvveride
{
    //public NavMeshAgent nav;
    
    public float speed;
    public float minRange;
    public float maxRange;

    public float TurnSpeed;

    public float OptimalRange;
    public float offset;
    public State meleState;
    public State doWhenDontSee;
    public State shootinstate;
    private void Start()
    {

    }
    public override void Enter()
    {
        SetDebugDisplay();
        //Debug.Log("Chasin");
        if (shootinstate != null)
        { SetChild(shootinstate); }

    }
    public override void Do()
    {
        //Debug.Log("Chasin");
        if (!brain.mainCore.CheckIncapacitated())
        {
            //brain.moveBrain.rotationOverrid = true;
            brain.mainCore.CalculateDesiredRotation((brain.target.position - core.transform.position).normalized, TurnSpeed, true);
            //brain.moveBrain.SetDesiredMovementRotation((target.position-core.transform.position).normalized, TurnSpeed);
        }
    }
    public override void FixedDo()
    {

        //Debug.Log("keepingDisatance");
        float eDist = Vector3.Distance(brain.target.position, core.transform.position);
        float middleDist = (minRange + maxRange) / 2;

        if (!brain.target.inLineOfSight)
        {
            //Debug.Log("aaaaaaaaaaaaaaaaaaaaaaaaaa");
            Change(doWhenDontSee);
        }
        else
        {
            if (maxRange > eDist && minRange < eDist)
            {
                // Debug.Log("inWeaponRange");
                //core.eAnimator.SetTrigger("Walk");
                if (minRange + OptimalRange + offset > eDist)
                {
                    brain.mainCore.animator.SetFloat("Dir",1);
                    brain.mainCore.SetMoveVector((core.transform.position - brain.target.position).normalized);
                    //Debug.Log("back off");
                    //Debug.Log("back offffffffffffffffffffffffffff");
                    //core.rBody.AddForce(((target - core.transform.position).normalized * -speed) - core.rBody.velocity, ForceMode.Acceleration);
                }
                else if (maxRange - OptimalRange + offset < eDist)
                {
                    brain.mainCore.animator.SetFloat("Dir",0);
                    brain.mainCore.SetMoveVector(NavCalc().normalized);
                    //core.rBody.AddForce((NavCalc() * speed) - core.rBody.velocity, ForceMode.Acceleration);
                }
                else
                {
                    brain.mainCore.SetMoveVector(Vector3.zero);
                    //idlin
                }

            }
            else
            {
                //Debug.Log("outsideWeaponRange");
                if (minRange < eDist)
                {
                    //Debug.Log("too close");
                    //goal = Goal.Succes;//go in mele
                    //Change(meleState);
                    //isComplete = true;
                }
                else
                {
                    //Debug.Log("skibb");
                    Change(meleState);
                    //goal = Goal.Fail;// end agro 
                }
                //isComplete = true;
            }
        }

        //core.eBody.AddForce(Vector3.up * 100f);
    }
    public override void Exit()
    {
        brain.mainCore.animator.SetFloat("Dir",0);
        //brain.moveBrain.rotationOverrid = false;
        //isComplete = false;
        //Debug.Log("StoppedRanged");
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
    //    Gizmos.DrawWireSphere(transform.position, minRange);
    //    Gizmos.color = Color.yellow;
    //    //Gizmos.DrawWireSphere(transform.position, OptimalRange);
    //    Gizmos.DrawWireSphere(transform.position, maxRange - OptimalRange + offset);
    //    Gizmos.DrawWireSphere(transform.position, minRange + OptimalRange + offset);
    //    //Gizmos.DrawLine(transform.position, patrolPoint[patrolIndex].position);
    //}
}
