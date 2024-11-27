using System;
using System.Collections;
using System.Collections.Generic;

using JetBrains.Annotations;
using UnityEngine;

public class EnemyBrain : MachineCore
{
    [SerializeField]
    private float hp;
    public State startState;

    //idk what to do witch it... 
    //public State overrideState = null;

    public bool setAgro;

    

    // Start is called before the first frame update
    void Start()
    {
        // iniitial Set UP
        SetupInstances();

        //main state machine
        sMachine.Set(startState,sMachine);
        //sMachine.state.FixedDo();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // if(currentState.isComplete)
        //{
        //   sMachine.Set(defaultState);
        //}

        //do fixed update in all active states
        currentState.FixedDoBranch();
        //currentState.FixedDo();
    }

    void Update()
    {

        //override States (ignore, only for debug)
        
        //Debug.Log("Main state machine is" + sMachine.state);
        //when any state is complete, select new state
        /*if (currentState.isComplete)
        {
            //if player is detected, become hostile or smth
            if (currentState == detectionState)
            {
                // set ai to be hostile (until death or losing player)
                sMachine.Set(agroState);

            }
            else // stopped being in agro, become passive
            {
                //passive behaviour until finding player
                sMachine.Set(detectionState);

            }
        }*/

        //constant states;
        //StateSelector();

        // Do state's Logic, When legic finished, switch to another state
        currentState.DoBranch();
        
        /*if(!currentState.DoBranch())
        {
            Debug.Log("ThisStateIsDone");
        }*/
    }

    void StateSelector()
    {
        //if() idk, i forgor 💀💀💀
    }


    //interfaces
    public void TakeDmg(float _dmg)
    {
        hp -= _dmg;
        if (hp <= 0)
        {
            Death();
        }
    }

    public void Death()
    {
        Debug.Log("died");
        //eAnimator.SetTrigger("Die");
        this.enabled = false;
    }

}
