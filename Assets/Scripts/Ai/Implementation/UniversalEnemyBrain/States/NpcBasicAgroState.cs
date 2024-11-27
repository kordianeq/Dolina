using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBasicAgroState : NpcBehaviorStateOvveride
{
[Header("Move stats, make plyer go brrrr")]
    [SerializeField] private float looseRange;  
    [SerializeField] private Transform target;
    [Header("state when loosing target")]   
    public State DoAfter;
    [Header("do when agro")]   
    public State chaseType;
    
    private void Awake() {
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }
    public override void Enter()
    {
        if (chaseType != null) { SetChild(chaseType); }
    }
    public override void Do()
    {
        //Debug.Log("i am moving yo");
        //Change(DoAfter);
              

    }
    public override void FixedDo()
    {

        if (Vector3.Distance(core.transform.position, target.position) > looseRange)
        {
            Debug.Log("Lost");
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
        Gizmos.DrawWireSphere(transform.position, looseRange);

    }
}
