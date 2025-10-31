using UnityEngine;
using UnityEngine.Rendering;

public enum AbilityType
{
    Passive,
    Active
}

[CreateAssetMenu(fileName = "AbilityDefinition", menuName = "Scriptable Objects/AbilityDefinition")]
public class AbilityDefinition : ScriptableObject
{
    [Header("General Info")]
    public string abilityName;
    public Sprite abilityIcon;
    public string description;
    public AbilityType abilityType;

    [Header("Ability Stats")]
    public float cooldown;
    public int additionalCost;

    public virtual void ActivateAbility(CharacterBehavior user)
    {
        Debug.Log($"{abilityName} activated by {user.name}");
        // Implement ability effect logic here
    }
}