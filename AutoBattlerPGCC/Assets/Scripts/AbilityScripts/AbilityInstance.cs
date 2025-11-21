using UnityEngine;

/// <summary>
/// Runtime instance of an ability
/// Tracks state like cooldowns, combo counters, intervals, etc.
/// </summary>
[System.Serializable]
public class AbilityInstance
{
    [Tooltip("Reference to the ability definition ScriptableObject")]
    public AbilityDefinition definition;

    // State tracking variables
    [Tooltip("Current cooldown timer (0 = ready)")]
    public float cooldownTimer = 0f;

    [Tooltip("Current combo count for OnCombo abilities")]
    public int comboCounter = 0;

    [Tooltip("Timer for OnInterval abilities")]
    public float intervalTimer = 0f;

    public AbilityInstance(AbilityDefinition def)
    {
        definition = def;
    }

    /// <summary>
    /// Check if ability is off cooldown
    /// </summary>
    public bool IsOffCooldown()
    {
        return cooldownTimer <= 0f;
    }

    /// <summary>
    /// Start the cooldown timer
    /// </summary>
    public void StartCooldown()
    {
        float previousCooldown = cooldownTimer;
        cooldownTimer = definition.cooldown;
    }

    /// <summary>
    /// Update cooldown timer (call every frame)
    /// </summary>
    public void UpdateCooldown(float deltaTime)
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= deltaTime;
        }
    }

    /// <summary>
    /// Increment combo counter
    /// </summary>
    public void IncrementCombo()
    {
        comboCounter++;
    }

    /// <summary>
    /// Reset combo counter to zero
    /// </summary>
    public void ResetCombo()
    {
        comboCounter = 0;
    }

    /// <summary>
    /// Update interval timer for OnInterval abilities
    /// </summary>
    public void UpdateInterval(float deltaTime)
    {
        if (definition.trigger == AbilityTrigger.OnInterval)
        {
            intervalTimer += deltaTime;
        }
    }

    /// <summary>
    /// Check if interval timer is ready and reset it
    /// </summary>
    public bool CheckAndResetInterval()
    {
        if (intervalTimer >= definition.intervalTime)
        {
            intervalTimer = 0f;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get cooldown percentage (0-1, where 1 = ready)
    /// </summary>
    public float GetCooldownPercentage()
    {
        if (definition.cooldown <= 0f) return 1f;
        return Mathf.Clamp01(1f - (cooldownTimer / definition.cooldown));
    }
}