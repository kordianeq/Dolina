using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NpcMoveStateOverride : State
{
    public NpcMovementBrain brain;
    public override void GetMasterScript()
    {
        brain = core.gameObject.GetComponent<NpcMovementBrain>();      
    }
}
