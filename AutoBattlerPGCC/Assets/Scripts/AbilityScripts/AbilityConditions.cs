using UnityEngine;

/// <summary>
/// Base class for all ability conditions
/// Conditions determine WHEN an ability can activate
/// </summary>
[System.Serializable]
public abstract class AbilityCondition
{
    public abstract bool IsMet(CharacterBehavior user, AbilityInstance abilityInstance);
}

/// <summary>
/// Condition: Check if cooldown is ready
/// </summary>
[System.Serializable]
public class CooldownCondition : AbilityCondition
{
    public override bool IsMet(CharacterBehavior user, AbilityInstance abilityInstance)
    {
        return abilityInstance.IsOffCooldown();
    }
}

/// <summary>
/// Condition: Check if user's health is above/below threshold
/// </summary>
[System.Serializable]
public class HealthThresholdCondition : AbilityCondition
{
    [Tooltip("Health percentage threshold (0-100)")]
    public float healthPercentage = 50f;

    [Tooltip("True = activate when ABOVE threshold, False = activate when BELOW threshold")]
    public bool above = false;

    public override bool IsMet(CharacterBehavior user, AbilityInstance abilityInstance)
    {
        float healthPercent = (user.health / (float)user.initialHealth) * 100f;
        return above ? healthPercent >= healthPercentage : healthPercent <= healthPercentage;
    }
}

/// <summary>
/// Condition: Check if it's the Nth attack in a combo
/// </summary>
[System.Serializable]
public class ComboCountCondition : AbilityCondition
{
    [Tooltip("Activate every Nth attack (e.g., 3 = every 3rd attack)")]
    public int requiredComboCount = 3;

    public override bool IsMet(CharacterBehavior user, AbilityInstance abilityInstance)
    {
        return abilityInstance.comboCounter >= requiredComboCount;
    }
}

/// <summary>
/// Condition: Check if target is within range
/// </summary>
[System.Serializable]
public class RangeCondition : AbilityCondition
{
    [Tooltip("Maximum range to target")]
    public float range = 5f;

    public override bool IsMet(CharacterBehavior user, AbilityInstance abilityInstance)
    {
        Transform target = user.GetClosestTarget();
        if (target == null) return false;

        float distance = Vector3.Distance(user.transform.position, target.position);
        return distance <= range;
    }
}

/// <summary>
/// Condition: Check if user has a specific buff/debuff
/// </summary>
[System.Serializable]
public class BuffCondition : AbilityCondition
{
    [Tooltip("Name of the required buff")]
    public string requiredBuffName;

    public override bool IsMet(CharacterBehavior user, AbilityInstance abilityInstance)
    {
        // TODO: Implement based on your buff system
        // Example: return user.HasBuff(requiredBuffName);
        return true; // Placeholder
    }
}

/// <summary>
/// Condition: Check if a random chance succeeds
/// </summary>
[System.Serializable]
public class ChanceCondition : AbilityCondition
{
    [Tooltip("Chance to succeed (0-100)")]
    [Range(0f, 100f)]
    public float chance = 50f;

    public override bool IsMet(CharacterBehavior user, AbilityInstance abilityInstance)
    {
        float roll = Random.Range(0f, 100f);
        bool success = roll <= chance;
        return success;
    }
}

[System.Serializable]
public class AfterAbilityUseCondition : AbilityCondition
{
    [Tooltip("Must trigger within this many seconds after using ANY OTHER ability")]
    public float timeWindow = 2f;

    [Tooltip("Only trigger after ACTIVE abilities (ignores passive)")]
    public bool onlyActiveAbilities = true;

    public override bool IsMet(CharacterBehavior user, AbilityInstance abilityInstance)
    {
        // Get the last ability that was used
        string lastAbilityUsed = user.GetLastAbilityUsed();
        float timeSinceLastUse = user.GetTimeSinceLastAbility();

        // CRITICAL: Prevent triggering on itself
        if (lastAbilityUsed == abilityInstance.definition.abilityName)
        {
            return false;
        }

        // Check if within time window
        bool withinWindow = timeSinceLastUse >= 0f && timeSinceLastUse <= timeWindow;

        if (withinWindow)
        {
            Debug.Log($"[AfterAbilityUseCondition] '{lastAbilityUsed}' used {timeSinceLastUse:F2}s ago - TRIGGERING");
        }

        return withinWindow;
    }
}