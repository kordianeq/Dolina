using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MachineCore : MonoBehaviour
{
    public StateMachine sMachine;

    public State currentState => sMachine.state;
    // Start is called before the first frame update

    //redundant, idk
    /*protected void Set(State newState, bool forceReset = false)
    {
        sMachine.Set(newState,forceReset);
    }*/

    public void SetupInstances()
    {
        sMachine = new StateMachine();
        State[] allStates = GetComponentsInChildren<State>();
        foreach (State state in allStates)
        {
            state.SetCore(this);
        }
    }

}
