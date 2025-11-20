using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "NpcDetectPlayer", story: "[npc] try to detect Player", category: "NPC_Base", id: "b6ec61839c9fbe19cd89e5c8ba4534cc")]
public partial class NpcDetectPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<NpcCoreBase> Npc;
    [SerializeReference] public BlackboardVariable<float> detectionRange;
    

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Debug.Log("Searching");
        //Npc.Value.GameplayUtilities.target.UpdateTargetPosition();
        if (Npc.Value.GameplayUtilities.target.IsinProximity(detectionRange))
            {
                if (Npc.Value.GameplayUtilities.target.IsInLineOfSight(detectionRange))
                {
                    // updates target last visible position,
                    // it allows the npc to remember the last place where target was visible
                    Npc.Value.GameplayUtilities.target.UpdateTargetPosition();
                    return Status.Success;
                }
            }

        return Status.Failure;
        //return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

