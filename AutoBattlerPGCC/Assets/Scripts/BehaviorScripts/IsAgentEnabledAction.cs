using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEditor.UI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "IsAgentEnabled", story: "Checks if [Character] agent is enabled", category: "Action", id: "e5ba4a9a54c9d5098c6a28bbb35d75a6")]
public partial class IsAgentEnabledAction : Action
{
    [SerializeReference] public BlackboardVariable<CharacterBehavior> Character;

    protected override Status OnStart()
    {
        return Character.Value.agent.isActiveAndEnabled ? Status.Success : Status.Failure;
    }

}

