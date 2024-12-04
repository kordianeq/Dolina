using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBehaviorBrain : MachineCore
{
    [SerializeField] public NpcMovementBrain moveBrain;
    [SerializeField] public State startState;
    [SerializeField] public State othersrtatsa;

    [SerializeField]private int hp;
    private bool isAgroed;
    
    void Awake()
    {        
        SetupInstances();
        sMachine.Set(startState,sMachine);
        //currentState.Change(startState);
        //currentState.Change(othersrtatsa);
        //currentState
        //need to fix,need to figure out what the fuck i did 
        //currentState = startState; nevermind doesnt work
    }

    // Update is called once per frame
    void Update()
    {
        currentState.DoBranch();
        //Debug.Log(currentState.childState);
    }

    private void FixedUpdate() {
        currentState.FixedDoBranch();
    }

    void Die()
    {
        Debug.Log("Dead i am");
    }

    public void TakeDmg(int _dmg)
    {
        isAgroed = true;
        hp-=_dmg;
        if(hp<=0)
        {
            Die();
        }
    }

    public bool IsAgroed()
    {
        if(isAgroed)
        {
            isAgroed = false;
            return true;
        }
        return false;
    }

}
