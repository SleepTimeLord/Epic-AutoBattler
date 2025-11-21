using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CheckIfTarget", story: "Checks if [self] has a Target", category: "Action", id: "6ded4d695d2bdda7ff08732fd7ff4bee")]
public partial class CheckIfTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<CharacterBehavior> Self;
    protected override Status OnStart()
    {
        return Self.Value.GetClosestTarget() ? Status.Success : Status.Failure;
    }
}

