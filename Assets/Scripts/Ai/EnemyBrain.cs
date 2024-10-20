using System;
using System.Collections;
using System.Collections.Generic;

using JetBrains.Annotations;
using UnityEngine;

public class EnemyBrain : MachineCore, IDmgeable
{
    [SerializeField]
    private float hp;

    public State detectionState;
    public State agroState;
    //idk what to do witch it... 
    //public State overrideState = null;

    public bool setAgro;

    

    // Start is called before the first frame update
    void Start()
    {
        // iniitial Set UP
        SetupInstances();

        sMachine.Set(detectionState);
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
        if (setAgro)
        {
            setAgro = false;
            sMachine.Set(agroState);
        }

        

        //when any state is complete, select new state
        if (currentState.isComplete)
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
        }

        //constant states;
        StateSelector();

        currentState.DoBranch();
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
        eAnimator.SetTrigger("Die");
        this.enabled = false;
    }

}
