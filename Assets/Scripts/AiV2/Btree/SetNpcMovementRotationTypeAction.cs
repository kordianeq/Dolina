using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetNpcMovementRotationType", story: "Set [Npc] movement rotation to [RotationMode]", category: "NPC_Base", id: "d54a4c79a09889e2b079732025bee506")]
public partial class SetNpcMovementRotationTypeAction : Action
{
    [SerializeReference] public BlackboardVariable<NpcCoreBase> Npc;
    [SerializeReference] public BlackboardVariable<NpcRotationModes> RotationMode;
    protected override Status OnStart()
    {
        Npc.Value.move.SetLookState(RotationMode);
        return Status.Running;
       
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

