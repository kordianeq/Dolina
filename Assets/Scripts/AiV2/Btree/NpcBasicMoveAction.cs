using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "NpcBasicMove", story: "[npc] move [moveVector]", category: "NPC_Base", id: "d4f0a0f2620ccb28d0d4cd6a44cd4378")]
public partial class NpcBasicMoveAction : Action
{
    [SerializeReference] public BlackboardVariable<NpcCoreBase> Npc;
    [SerializeReference] public BlackboardVariable<Vector3> moveVector;
    [SerializeReference] public BlackboardVariable<float> time;
    float timer;
    protected override Status OnStart()
    {
        
        timer = time;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        while (timer>= 0)
        {
            //Debug.Log(timer);
            timer-=Time.deltaTime;
            Npc.Value.move.SetMoveVector(moveVector);
            //Npc.Value.move.MoveCharacter(1,1);
            //Npc.Value.move.AddDirectionalForce(moveVector,1f,ForceMode.Force);
            return Status.Running;
        }
        Npc.Value.move.SetMoveVector(new(0,0,0));
        return Status.Success;
    }

    

    protected override void OnEnd()
    {
        Npc.Value.move.SetMoveVector(new(0,0,0));
        Npc.Value.move.SetLookState(NpcRotationModes.disabled);
    }
}

