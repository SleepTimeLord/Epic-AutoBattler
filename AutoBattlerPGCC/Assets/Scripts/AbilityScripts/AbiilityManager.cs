using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages all abilities for a character
/// Handles activation, cooldowns, and triggering
/// Add this component to your character GameObject
/// </summary>
public class AbilityManager : MonoBehaviour
{
    private CharacterBehavior character;
    private List<AbilityInstance> abilityInstances = new List<AbilityInstance>();

    // Prevent multiple initializations
    private bool isInitialized = false;

    // Prevents recursive ability triggering
    private HashSet<AbilityTrigger> currentlyProcessingTriggers = new HashSet<AbilityTrigger>();

    private void Awake()
    {
        character = GetComponent<CharacterBehavior>();
    }

    /// <summary>
    /// Initialize all abilities from CharacterBehavior's ability lists
    /// Call this after character is fully set up
    /// </summary>
    public void InitializeAbilities()
    {
        // Prevent multiple initializations
        if (isInitialized)
        {
            Debug.LogWarning($"[AbilityManager] InitializeAbilities already called for {character.characterName}! Skipping.");
            return;
        }


        abilityInstances.Clear();

        // Add passive abilities
        foreach (var abilityDef in character.passiveAbility)
        {
            if (abilityDef != null)
            {
                AbilityInstance instance = new AbilityInstance(abilityDef);
                abilityInstances.Add(instance);
            }
        }

        // Add active abilities
        foreach (var abilityDef in character.activeAbility)
        {
            if (abilityDef != null)
            {
                AbilityInstance instance = new AbilityInstance(abilityDef);
                abilityInstances.Add(instance);
            }
        }

        isInitialized = true;

        // Trigger OnSpawn abilities
        TriggerAbilities(AbilityTrigger.OnSpawn);
    }

    private void Update()
    {
        if (!isInitialized) return;

        // Update cooldowns and intervals for all abilities
        foreach (var ability in abilityInstances)
        {
            ability.UpdateCooldown(Time.deltaTime);
            ability.UpdateInterval(Time.deltaTime);

            // Check for interval-based passives
            if (ability.definition.abilityType == AbilityType.Passive &&
                ability.definition.trigger == AbilityTrigger.OnInterval &&
                ability.CheckAndResetInterval())
            {
                TryActivateAbility(ability, AbilityTrigger.OnInterval);
            }
        }
    }

    /// <summary>
    /// Trigger all passive abilities with the specified trigger
    /// </summary>
    public void TriggerAbilities(AbilityTrigger trigger, CharacterBehavior target = null)
    {
        if (!isInitialized)
        {
            return;
        }

        // Prevent recursive triggering
        if (currentlyProcessingTriggers.Contains(trigger))
        {
            Debug.LogWarning($"[AbilityManager] BLOCKED RECURSIVE TRIGGER: {trigger} for {character.characterName}");
            return;
        }

        // Mark this trigger as being processed
        currentlyProcessingTriggers.Add(trigger);

        try
        {
            int checkedCount = 0;
            int activatedCount = 0;

            foreach (var ability in abilityInstances)
            {
                if (ability.definition.abilityType == AbilityType.Passive &&
                    ability.definition.trigger == trigger)
                {
                    checkedCount++;

                    bool wasActivated = TryActivateAbility(ability, trigger, target);
                    if (wasActivated) activatedCount++;
                }
            }

        }
        finally
        {
            // Always remove the trigger from processing, even if there's an exception
            currentlyProcessingTriggers.Remove(trigger);
        }
    }

    /// <summary>
    /// Try to activate a specific ability instance
    /// Returns true if ability was activated
    /// UPDATED: Now with recursion protection
    /// </summary>
    private bool TryActivateAbility(AbilityInstance ability, AbilityTrigger trigger, CharacterBehavior target = null)
    {
        // ADDED: Prevent recursive ability activation
        if (character.IsProcessingAbility())
        {
            return false;
        }

        // Check if all conditions are met
        if (!ability.definition.CanActivate(character, ability))
        {
            return false;
        }

        // ADDED: Mark ability as being processed
        character.BeginAbilityProcessing(ability.definition.abilityName);

        try
        {
            // Get target if not provided
            if (target == null)
            {
                Transform targetTransform = character.GetClosestTarget();
                target = targetTransform != null ? targetTransform.GetComponent<CharacterBehavior>() : null;
            }

            // Activate the ability
            ability.definition.Activate(character, target, ability);

            // ADDED: Track ability usage for AfterAbilityUse conditions
            character.OnAbilityUsed(ability.definition.abilityName, ability.definition.abilityType);

            // Handle combo abilities - reset counter after activation
            if (ability.definition.trigger == AbilityTrigger.OnCombo)
            {
                ability.ResetCombo();
            }

            return true;
        }
        finally
        {
            // always end processing, even if there's an exception
            character.EndAbilityProcessing(ability.definition.abilityName);
        }
    }

    /// <summary>
    /// Manually activate an active ability by index
    /// Called from input or AI systems
    /// </summary>
    public void ActivateActiveAbility(int abilityIndex)
    {
        if (!isInitialized)
        {
            Debug.LogWarning($"[AbilityManager] ActivateActiveAbility called before initialization!");
            return;
        }

        var activeAbilities = abilityInstances
            .Where(a => a.definition.abilityType == AbilityType.Active)
            .ToList();

        if (abilityIndex < 0 || abilityIndex >= activeAbilities.Count)
        {
            return;
        }

        var ability = activeAbilities[abilityIndex];
        TryActivateAbility(ability, AbilityTrigger.None);
    }

    /// <summary>
    /// Increment combo counters for all combo-based abilities
    /// Call this when the character performs an attack
    /// </summary>
    public void IncrementCombos()
    {
        if (!isInitialized) return;

        foreach (var ability in abilityInstances)
        {
            if (ability.definition.trigger == AbilityTrigger.OnCombo)
            {
                ability.IncrementCombo();

                // Check if combo threshold is met
                TryActivateAbility(ability, AbilityTrigger.OnCombo);
            }
        }
    }

    /// <summary>
    /// Reset all combo counters to zero
    /// Call this when combo is broken (e.g., too much time between attacks)
    /// </summary>
    public void ResetAllCombos()
    {
        if (!isInitialized) return;

        foreach (var ability in abilityInstances)
        {
            if (ability.definition.trigger == AbilityTrigger.OnCombo)
            {
                ability.ResetCombo();
            }
        }
    }

    /// <summary>
    /// Get cooldown timer for a specific ability (for UI display)
    /// </summary>
    public float GetAbilityCooldown(int abilityIndex, AbilityType type)
    {
        if (!isInitialized) return 0f;

        var abilities = abilityInstances
            .Where(a => a.definition.abilityType == type)
            .ToList();

        if (abilityIndex >= 0 && abilityIndex < abilities.Count)
        {
            return abilities[abilityIndex].cooldownTimer;
        }
        return 0f;
    }

    /// <summary>
    /// Get cooldown percentage for UI (0-1, where 1 = ready)
    /// </summary>
    public float GetAbilityCooldownPercentage(int abilityIndex, AbilityType type)
    {
        if (!isInitialized) return 1f;

        var abilities = abilityInstances
            .Where(a => a.definition.abilityType == type)
            .ToList();

        if (abilityIndex >= 0 && abilityIndex < abilities.Count)
        {
            return abilities[abilityIndex].GetCooldownPercentage();
        }
        return 1f;
    }

    /// <summary>
    /// Get all active abilities (for UI display)
    /// </summary>
    public List<AbilityInstance> GetActiveAbilities()
    {
        if (!isInitialized) return new List<AbilityInstance>();

        return abilityInstances
            .Where(a => a.definition.abilityType == AbilityType.Active)
            .ToList();
    }

    /// <summary>
    /// Get all passive abilities (for UI display)
    /// </summary>
    public List<AbilityInstance> GetPassiveAbilities()
    {
        if (!isInitialized) return new List<AbilityInstance>();

        return abilityInstances
            .Where(a => a.definition.abilityType == AbilityType.Passive)
            .ToList();
    }
}