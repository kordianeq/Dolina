using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBehaviorStateOvveride : State
{
    public NpcBehaviorBrain brain;

    public GameObject debBody;
    public Material debMat;
    public override void GetMasterScript()
    {
        brain = core.gameObject.GetComponent<NpcBehaviorBrain>();      
    }

    public void SetDebugDisplay()
    {
        debBody.GetComponent<Renderer>().material = debMat;
    }
}
