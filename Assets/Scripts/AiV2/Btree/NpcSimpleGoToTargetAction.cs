using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "NpcSimpleGoToTarget", story: "[Npc] go to [target]", category: "NPC_Base", id: "f173aed4f3ff70a70386b6d579ef2bf1")]
public partial class NpcSimpleGoToTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<NpcCoreBase> Npc;
    [SerializeReference] public BlackboardVariable<Vector3> Target;
    [SerializeReference] public BlackboardVariable<float> SpeedMultiplier;
    [SerializeReference] public BlackboardVariable<float> time;
    [SerializeReference] public BlackboardVariable<float> GoalDistance;
    [SerializeReference] public BlackboardVariable<NpcRotationModes> RotationMode;
    float timer;
    Vector3 moveVector;
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
        //watch out endless loop
        while (timer >= 0 || unasiged == true)
        {
            //Debug.Log(timer);
            timer-=Time.deltaTime;
            
            Debug.Log("Goin to: " + Target.Value);
            Vector3 dir = Target.Value - Npc.Value.GetTransform().position;
            Npc.Value.move.SetMoveVector(Vector3.ProjectOnPlane(dir.normalized,Vector3.up));
            Debug.DrawRay(Target.Value,Vector3.up,Color.blue);

            if( Npc.Value.GameplayUtilities.target.GetDistance() <= GoalDistance)
            {
                return Status.Success;
            }

            //Debug.DrawRay()
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

