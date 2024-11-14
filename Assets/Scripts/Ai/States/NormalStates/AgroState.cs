using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public class AgroState : State
{
    //public Mod_State MovementType;
    public State EntryState;
    public State RangedMode;
    public State MeleMode;
    //public State DoInRange;

    //public float maxDetectionRange;
    
    public override void Enter()
    {

        //Debug.Log("AgroMode");

        if(EntryState!=null)
        {
        SetChild(EntryState);
        }

        //MovementType.target = target.position;
    }
    public override void Do()
    {

    }
    
    public override void FixedDo()
    {   
        /*
        if (Vector3.Distance(core.transform.position, target.position) > maxDetectionRange)
        {
            Debug.Log("LostTarget");
            targetLost = true;
            
        }else
        {
            
            targetLost = false;
        }*/

        //constantly update target position (should change later) like make raycasting or smth.... or another detect state
        //MovementType.target = target.position;

        // if any state is complete figure ou new state
        if (childState.isComplete)
        {
            //if ranged out of range
            if (childState == RangedMode)
            {
                if(childState.goal == Goal.Succes)
                {
                    SetChild(MeleMode);
                    //isComplete = true;
                   // Debug.Log("Reached Mele Distance!");
                }else
                {

                    isComplete = true;
                    //Debug.Log("Out of any distance!");
                }
            }else if(childState == MeleMode)
            {
                if(childState.goal == Goal.Succes)
                {
                    SetChild(MeleMode);
                    //isComplete = true;
                    //Debug.Log("Performed mele i gues?");
                }else
                {
                    isComplete = true;
                    //Debug.Log("fucked up");
                }

            }else // entry state i guess
            {
                //Debug.Log("waitingInEntry");
                SetChild(RangedMode);
            }
            
            /*
            switch (childState)
            {
                case :

                break;
            }
            /*
            if (childState == MovementType)
            {
                //perform Attack if
                if (targetLost)
                {
                    isComplete = true;
                }
                else
                {
                    SetChild(DoInRange);
                    //weaponT.Shoot();
                }


            }
            else
            {
                isComplete = true;
                Debug.Log("FinishedAttack");
            }*/
        }

        //Debug.Log(patrolPoint.Length);



        //Debug.Log("TargetDir");
        //childState.FixedDo();

        //Debug.Log(MovementType.direction);
    }
    public override void Exit() { }
}
