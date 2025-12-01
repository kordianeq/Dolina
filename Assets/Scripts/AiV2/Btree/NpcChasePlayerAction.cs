using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "NpcChasePlayer", story: "[Npc] pathfind to [Target]", category: "NPC_Base", id: "7a78104b9ded9198db78d1f46e2d9653")]
public partial class NpcChasePlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<NpcCoreBase> Npc;
    [SerializeReference] public BlackboardVariable<Vector3> Target;
    [SerializeReference] public BlackboardVariable<float> SpeedMultiplier;
    [SerializeReference] public BlackboardVariable<float> time;
    [SerializeReference] public BlackboardVariable<float> GoalDistance;
    [SerializeReference] public BlackboardVariable<NpcRotationModes> RotationMode;

    float timer = 0;
    bool unasiged = false;
    protected override Status OnStart()
    {
        if(time == 0 || time == null)
        {
            unasiged = true;
        }else
        {
            timer = time;
        }
        Npc.Value.move.SetLookState(RotationMode);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        while (timer >= 0 || unasiged == true)
        {
            //Debug.Log(timer);
            timer-=Time.deltaTime;
            
            Debug.Log("Goin to: " + Target.Value);

            Vector3 dir = Target.Value - Npc.Value.GetTransform().position;


            Vector3 navVect =  Npc.Value.GameplayUtilities.NavCalc();
            Npc.Value.move.SetMoveVector(navVect);  
            //Npc.Value.move.SetMoveVector(Vector3.ProjectOnPlane(dir.normalized,Vector3.up));
            Debug.DrawRay(Target.Value,Vector3.up,Color.blue);

            if( Npc.Value.GameplayUtilities.target.GetDistance() <= GoalDistance)
            {
                return Status.Success;
            }

            
            return Status.Running;
        }

        Npc.Value.move.SetMoveVector(new(0,0,0));
        return Status.Failure;
    }

    protected override void OnEnd()
    {
        Npc.Value.move.SetMoveVector(new(0,0,0));
        Npc.Value.move.SetLookState(NpcRotationModes.disabled);
    }
}

