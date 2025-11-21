using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject that defines an ability
/// Create via: Right-click → Create → Abilities → Ability Definition
/// </summary>
[CreateAssetMenu(fileName = "NewAbility", menuName = "Abilities/Ability Definition")]
public class AbilityDefinition : ScriptableObject
{
    [Header("General Info")]
    [Tooltip("Name of the ability")]
    public string abilityName;

    [Tooltip("Icon displayed in UI")]
    public Sprite abilityIcon;

    [Tooltip("Description of what the ability does")]
    [TextArea(3, 5)]
    public string description;

    [Tooltip("Is this ability passive or active?")]
    public AbilityType abilityType;

    [Header("Trigger Settings")]
    [Tooltip("When should this ability trigger? (Only for passive abilities)")]
    public AbilityTrigger trigger = AbilityTrigger.None;

    [Tooltip("Time between activations (for OnInterval trigger)")]
    public float intervalTime = 5f;

    [Header("Ability Stats")]
    [Tooltip("Cooldown in seconds before ability can be used again")]
    public float cooldown = 3f;

    [Tooltip("Additional cost added to character's total cost")]
    public int additionalCost = 1;

    [Header("Conditions (ALL must be met to activate)")]
    [Tooltip("List of conditions that must all be true for ability to activate")]
    [SerializeReference] // Important: allows polymorphism in inspector
    public List<AbilityCondition> conditions = new List<AbilityCondition>();

    [Header("Effects (Executed in order)")]
    [Tooltip("List of effects that execute when ability activates")]
    [SerializeReference] // Important: allows polymorphism in inspector
    public List<AbilityEffect> effects = new List<AbilityEffect>();

    /// <summary>
    /// Check if all conditions are met for this ability to activate
    /// </summary>
    public bool CanActivate(CharacterBehavior user, AbilityInstance abilityInstance)
    {
        // Always check cooldown for active abilities
        if (abilityType == AbilityType.Active && !abilityInstance.IsOffCooldown())
        {
            return false;
        }

        // Also check cooldown for passive abilities
        if (abilityType == AbilityType.Passive && !abilityInstance.IsOffCooldown())
        {
            return false;
        }

        // Check all custom conditions
        foreach (var condition in conditions)
        {
            if (condition != null && !condition.IsMet(user, abilityInstance))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Execute all effects of this ability
    /// </summary>
    public void Activate(CharacterBehavior user, CharacterBehavior target, AbilityInstance abilityInstance)
    {
        // Execute all effects in order
        foreach (var effect in effects)
        {
            if (effect != null)
            {
                effect.Execute(user, target, abilityInstance);
            }
        }

        // Start cooldown
        abilityInstance.StartCooldown();
    }
}