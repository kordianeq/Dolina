using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
using UnityEngine;

public class NpcBasicAgroState : NpcBehaviorStateOvveride
{
    [Header("Agro state stats...")]
    [SerializeField] private float looseRange;
    [SerializeField] private bool findForEver;
    [SerializeField] private float giveUpTime;

    [Header("Same level state")]   
    public State DoAfter;
    
    [Header("child state")]   
    public State chaseType;
    
    public override void Enter()
    {
        brain.target.lost = false;
        if (chaseType != null) { SetChild(chaseType,true); }
        SetDebugDisplay();
    }
    public override void Do()
    {

    }
    public override void FixedDo()
    {
        // first simply checks distance between target and enemy/ only if in range performs raycast to make sure that target is visible, idk if this is efficient but yolo
        if (!brain.target.IsinProximity(looseRange) || brain.target.lost)
        {
            //when outside of range, change state
            Change(DoAfter);
        }
        else
        {
            //if target is in line of sight, good. if not try to go to the last known position
            if(brain.target.IsInLineOfSight(looseRange))
            {
                // updates target last visible position,
                // it allows the npc to remember the last place where target was visible
                brain.target.UpdateTargetPosition();
                
            }else
            {
                // continue agro even when can't see the player... idk 
                if(findForEver)
                {
                    
                }
                else
                {
                    Change(DoAfter);
                }
                
            }
        }
    }
    public override void Exit()
    {

    }

    //void OnDrawGizmosSelected()
    //{

    //    if (!UnityEditor.Selection.Contains(gameObject)) { return; }
    //    // Display the explosion radius when selected
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, looseRange);

    //    Gizmos.color = Color.green;
    //    if(brain!= null)
    //    {
    //        Gizmos.DrawWireSphere(brain.target.position, 1f);
    //    }

    //}
}
