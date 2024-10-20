using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor.Rendering;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    public MachineCore core;
    public bool isComplete {get; protected set;}
    public enum Goal{None,Fail,Succes};
    public Goal goal;
    protected float startTime;
    public float time => Time.time - startTime;

    //unused
    public StateMachine parent;

    //thoese two are connected...
    public StateMachine branchMachine;
    public State childState => branchMachine.state;
    

    public virtual void Enter(){}
    public virtual void Do(){}
    public virtual void FixedDo(){}
    public virtual void Exit(){}

    public void DoBranch() 
    {
        Do();
        childState?.DoBranch();
    }

    public void FixedDoBranch() 
    {
        FixedDo();
        childState?.FixedDoBranch();
    }

    //redundant, but idk tho
    protected void SetChild(State newState, bool forceReset = false)
    {
        ExitChild();
        branchMachine.Set(newState,forceReset);
        
    }

    public void ExitChild() 
    {
        childState?.Exit();
        childState?.ExitChild();
    }

    public void SetCore(MachineCore _core)
    {
        branchMachine = new StateMachine();
        core = _core;
    }

    public void Initialise()
    {
        parent = null;
        isComplete = false;
        startTime = Time.time;
    }

    //unused
    public void Initialise(StateMachine _parent)
    {
        parent = _parent;
        isComplete = false;
        startTime = Time.time;
    }
}
