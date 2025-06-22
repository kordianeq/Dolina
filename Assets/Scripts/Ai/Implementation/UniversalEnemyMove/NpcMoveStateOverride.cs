using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NpcMoveStateOverride : State
{
    public NpcMovementBrain brain;
    public String animOverride;
    public GameObject debBody;
    public Material debMat;
    public override void GetMasterScript()
    {
        brain = core.gameObject.GetComponent<NpcMovementBrain>();
    }

    public void SetDebugDisplay()
    {
        //debBody.GetComponent<Renderer>().material = debMat;
    }

    public void ForceStateAnim(string ani)
    {
        if (!string.IsNullOrEmpty(ani))
        {
            brain.mainCore.animator.SetTrigger(ani);
        }
    } 

    public void ForceStateAnim()
    {
        if (!string.IsNullOrEmpty(animOverride))
        {
            brain.mainCore.animator.SetTrigger(animOverride);
        }
    } 
}
