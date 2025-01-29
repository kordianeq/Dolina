using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NpcMoveStateOverride : State
{
    public NpcMovementBrain brain;

    public GameObject debBody;
    public Material debMat;
    public override void GetMasterScript()
    {
        brain = core.gameObject.GetComponent<NpcMovementBrain>();      
    }

    public void SetDebugDisplay()
    {
        debBody.GetComponent<Renderer>().material = debMat;
    }
}
