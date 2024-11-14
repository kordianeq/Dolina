using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAgroCore : State
{
    public string animName;
    public State StateExit;

    [SerializeField] private Transform playerTarget;
    public float detectionRange;

    public Mod_State ChaseMode;
    public override void Enter()
    {
        //core.eAnimator.SetTrigger(animName);
        isComplete = false;
        //Debug.Log("Starting to vibe");
        SetChild(ChaseMode);
    }
    public override void Do()
    {
        //Debug.Log("Agrooo");
        //Debug.Log("vibing time: "+time);
        if (Vector3.Distance(core.transform.position, playerTarget.position) > detectionRange)
        {
            Debug.Log("Lost");
            isComplete = true;
            Change(StateExit);
        }
        
        //Change(ChaseMode);
    }
    public override void FixedDo()
    {
        ChaseMode.target = playerTarget.position;
        //Debug.Log("..Vibing..");
        //core.eBody.AddForce(Vector3.up * 100f);
    }
    public override void Exit()
    {
        isComplete = false;
    }

    void OnDrawGizmosSelected()
    {
        
            if (!UnityEditor.Selection.Contains(gameObject)) { return; }
            // Display the explosion radius when selected
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
        
    }
}
