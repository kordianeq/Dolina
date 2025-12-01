using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "NpcPlayerDetector", story: "[Npc] [is_detecting] Player", category: "NPC_Base", id: "38846932855ffb2b15b0f6ae98852023")]
public partial class NpcPlayerDetectorAction : Action
{
    [SerializeReference] public BlackboardVariable<NpcCoreBase> Npc;
    [SerializeReference] public BlackboardVariable<bool> Is_detecting;
    [SerializeReference] public BlackboardVariable<float> detectionRange;
    [SerializeReference] public BlackboardVariable<Vector3> saveLastTargetLocation;
    [SerializeReference] public BlackboardVariable<float> saveTargetDistance;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
       Debug.Log("Searching");
       saveTargetDistance.Value = Npc.Value.GameplayUtilities.target.GetDistance();
        //Npc.Value.GameplayUtilities.target.UpdateTargetPosition();
        if (Npc.Value.GameplayUtilities.target.IsinProximity(detectionRange))
            {
                if (Npc.Value.GameplayUtilities.target.IsInLineOfSight(detectionRange))
                {
                    // updates target last visible position,
                    // it allows the npc to remember the last place where target was visible
                    Npc.Value.GameplayUtilities.target.UpdateTargetPosition();
                    saveLastTargetLocation.Value = Npc.Value.GameplayUtilities.target.GetTargetPosition();
                    Is_detecting.Value = true;
                    return Status.Success;
                }
            }
        Is_detecting.Value = false;
        return Status.Failure;
    }

    protected override void OnEnd()
    {
        
    }
}

