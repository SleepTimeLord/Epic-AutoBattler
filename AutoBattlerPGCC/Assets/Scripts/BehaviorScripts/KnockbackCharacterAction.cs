using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "KnockbackCharacter", story: "[Character] gets knockedBack", category: "Action", id: "468b9161be02e7dc12a29a32d0ad442a")]
public partial class KnockbackCharacterAction : Action
{
    [SerializeReference] public BlackboardVariable<CharacterBehavior> Character;

    protected override Status OnStart()
    {
        Vector3 dir = -Character.Value.transform.forward;
        Character.Value.StartKnockback(dir, Character.Value.attackedWeapon.attackKnockbackDistance, Character.Value.attackedWeapon.attackKnockbackDuration);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        // Check if knockback duration has ended abd set status as success
        if (Time.time > Character.Value.knockbackEndTime) 
        {
            Character.Value.IsBeingKnockedBack = false;
            return Status.Success;
        }
        else 
        {
            return Status.Running;
        }

    }

    protected override void OnEnd()
    {
    }


}

