using System;
using System.Runtime.CompilerServices;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "LookAtTarget", story: "[Self] looks at [Character] Target", category: "Action", id: "c130f37e31a84fac47b5e26a67e40268")]
public partial class LookAtTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<CharacterBehavior> Character;
    private Transform transform;

    protected override Status OnStart()
    {
        transform = Self.Value.transform;
        return Status.Running;

    }

    protected override Status OnUpdate()
    {
        if (Character.Value.GetClosestTarget() == null)
        {
            return Status.Failure;
        }
        Vector2 direction = Character.Value.GetClosestTarget().position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        return Status.Running;
    }
}

