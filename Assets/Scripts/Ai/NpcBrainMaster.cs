using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NpcBrainMaster : MachineCore
{
    public EnemyCore mainCore;
    public virtual void ForceDeadState(){}
    public virtual void ForceResurectState(){}

    public virtual void ForceStunnState(){}
}
