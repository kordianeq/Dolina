using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "NpcLookAtTarget", story: "[Npc] look at [Target]", category: "NPC_Base", id: "8033de3b4062378c8af355153a0afa22")]
public partial class NpcLookAtTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<NpcCoreBase> Npc;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<float> lookSpeed;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Npc.Value.move.CalculateDesiredRotation((Target.Value.position - Npc.Value.transform.position).normalized,lookSpeed);
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

