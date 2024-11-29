using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBehaviorStateOvveride : State
{
    public NpcBehaviorBrain brain;
    public override void GetMasterScript()
    {
        brain = core.gameObject.GetComponent<NpcBehaviorBrain>();      
    }
}
