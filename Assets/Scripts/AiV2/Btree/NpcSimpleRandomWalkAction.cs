using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;


[Serializable, GeneratePropertyBag]
[NodeDescription(name: "NpcSimpleRandomWalk", story: "[Npc] go in random direction", category: "NPC_Base", id: "5d5b80263cf60c76a2baa08c06575794")]
public partial class NpcSimpleRandomWalkAction : Action
{
    [SerializeReference] public BlackboardVariable<NpcCoreBase> Npc;
    [SerializeReference] public BlackboardVariable<float> time;
    float timer;
    Vector3 moveVector;

    protected override Status OnStart()
    {
        moveVector =  UnityEngine.Random.insideUnitSphere;
        moveVector = Vector3.ProjectOnPlane(moveVector, Vector3.up).normalized;
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

