using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CheckIfTakeDamage", story: "Checks if [Character] tooked damage", category: "Action", id: "7666e333e66dda9b9303d651c98a8f6f")]
public partial class CheckIfTakeDamageAction : Action
{
    [SerializeReference] public BlackboardVariable<CharacterBehavior> Character;

    protected override Status OnStart()
    {

        return Character.Value.tookDamage ? Status.Success : Status.Failure;
    }


}

