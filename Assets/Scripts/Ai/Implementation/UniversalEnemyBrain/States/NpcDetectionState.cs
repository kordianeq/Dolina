using UnityEditor;
using UnityEngine;

public class NpcDetectionState : NpcBehaviorStateOvveride
{
    [Header("Detection stats...")]
    [SerializeField] private float detectionRange;  
    
    [Header("same level state")]   
    public State DoAfter;

    [Header("children state")]   
    public State patrolType;
    
    private void Awake() {

    }
    public override void Enter()
    {
        SetDebugDisplay();
        if (patrolType != null) 
        { 
            //Debug.Log("InitPatrol");
            SetChild(patrolType,true); 
        }
        //Debug.Log("aaaaa child: "+ childState);
    }
    public override void Do()
    {

    }
    public override void FixedDo()
    {   
        
        // first simply checks distance between target and enemy/ only if in range performs raycast to make sure that target is visible, idk if this is efficient but yolo
        if (brain.target.IsinProximity(detectionRange))
        {
            if(brain.target.IsInLineOfSight(detectionRange))
            {
                // updates target last visible position,
                // it allows the npc to remember the last place where target was visible
                brain.target.UpdateTargetPosition();
                
                //when player is detected change state to diferent state
                Change(DoAfter);
            }
        }
    }

    public override void Exit()
    {

    }

    //void OnDrawGizmosSelected()
    //{
    //    if (!UnityEditor.Selection.Contains(gameObject)) { return; }
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, detectionRange);

    //    Gizmos.color = Color.yellow;
    //    if(brain!= null)
    //    {
    //        Gizmos.DrawWireSphere(brain.target.position, 1f);
    //    }
    //}
}
