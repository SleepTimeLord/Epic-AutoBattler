using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "InWeaponRange", story: "[Character] target is in attack range", category: "Action", id: "3fb106670046df4af95283bff6956da3")]
public partial class InWeaponRangeAction : Action
{
    [SerializeReference] public BlackboardVariable<CharacterBehavior> Character;

    protected override Status OnStart()
    {
        if (Character.Value.GetClosestTarget() == null)
        {
            return Status.Failure;
        }
        return Character.Value.IsTargetInWeaponRange() ? Status.Success : Status.Failure;
    }
}

