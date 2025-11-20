using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "NpcJumpLileAnIdiot", story: "[npc] jump lika a moron", category: "NPC_Base", id: "487c2b752edb580c38aea2a98bae6e14")]
public partial class NpcJumpLileAnIdiotAction : Action
{
    [SerializeReference] public BlackboardVariable<NpcCoreBase> Npc;
    [SerializeReference] public BlackboardVariable<float> jumpT;
    [SerializeReference] public BlackboardVariable<float> forcej;
    float jumpTimer;
    protected override Status OnStart()
    {
        jumpTimer = jumpT;
        Npc.Value.move.AddDirectionalForce(Vector3.up,forcej,ForceMode.Impulse);
        return Status.Running;
        
    }

    protected override Status OnUpdate()
    {
        while (jumpTimer>0)
        {
            jumpTimer-= Time.deltaTime;
            return Status.Running;
        }
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

