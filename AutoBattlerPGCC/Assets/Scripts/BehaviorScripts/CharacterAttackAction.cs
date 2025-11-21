using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CharacterAttack", story: "[Character] attacks", category: "Action", id: "a82a7fc5efb1ee683063dd6da6d61708")]
public partial class CharacterAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<CharacterBehavior> Character;
    protected override Status OnUpdate()
    {
        if (Character.Value.GetClosestTarget() == null || !Character.Value.IsTargetInWeaponRange())
        {
            return Status.Failure;
        }
        Character.Value.weapon.Attack(Character);

        return Status.Running;
    }
}

