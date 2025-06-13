using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MoveStateOverride : State
{
    public MovementBrain brain;
    public override void GetMasterScript()
    {
        brain = core.gameObject.GetComponent<MovementBrain>();      
    }
}
