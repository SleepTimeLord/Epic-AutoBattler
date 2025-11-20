using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ChaseEnemy", story: "[Self] chases [Target]", category: "Action", id: "c49bd39c6db5079d3af51ccd51290013")]
public partial class ChaseEnemyAction : Action
{
    [SerializeReference] public BlackboardVariable<CharacterBehavior> Self;
    [SerializeReference] public BlackboardVariable<GoToTarget> Target;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self.Value.GetClosestTarget() == null)
        {
            return Status.Failure;
        }
        if (!Self.Value.agent.isActiveAndEnabled) 
        { 
            return Status.Failure;
        }
        Target.Value.ChaseTarget(Self.Value.GetClosestTarget());
        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}