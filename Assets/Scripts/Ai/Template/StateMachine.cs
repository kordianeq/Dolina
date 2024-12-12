using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public State state;

    public void Set(State newState,StateMachine parent, bool forceReset = false)
    {
        if(state != newState || forceReset)
        {
            state?.ExitAll();
            state = newState;
            parent.state.OnChildChange();
            state.Initialise(parent);
            state.Enter();
        }
    }
}