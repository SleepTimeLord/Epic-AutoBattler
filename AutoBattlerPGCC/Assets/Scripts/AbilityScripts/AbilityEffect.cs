using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Base class for all ability effects
/// Effects determine WHAT an ability does when activated
/// </summary>
[System.Serializable]
public abstract class AbilityEffect
{
    public abstract void Execute(CharacterBehavior user, CharacterBehavior target, AbilityInstance abilityInstance);

    /// <summary>
    /// Helper method to calculate stat-based bonus
    /// </summary>
    protected int CalculateStatBonus(CharacterBehavior user, StatType statType, float multiplier)
    {
        switch (statType)
        {
            case StatType.Strength:
                return Mathf.RoundToInt(user.strength * multiplier);
            case StatType.Speed:
                return Mathf.RoundToInt(user.speed * multiplier);
            case StatType.Intelligence:
                return Mathf.RoundToInt(user.intelligence * multiplier);
            default:
                return 0;
        }
    }
}

/// <summary>
/// Effect: Deal damage to target
/// </summary>
[System.Serializable]
public class DamageEffect : AbilityEffect
{
    [Tooltip("Base damage amount")]
    public int baseDamage = 10;

    [Tooltip("Damage multiplier (1.0 = normal, 2.0 = double damage)")]
    public float damageMultiplier = 1f;

    [Tooltip("Should damage scale with user's strength stat?")]
    public bool scalesWithStrength = true;

    [Tooltip("Strength scaling multiplier")]
    public float strengthScaling = 0.2f;

    [Tooltip("Should damage scale with user's intelligence stat?")]
    public bool scalesWithIntelligence = false;

    [Tooltip("Intelligence scaling multiplier")]
    public float intelligenceScaling = 0.5f;

    public override void Execute(CharacterBehavior user, CharacterBehavior target, AbilityInstance abilityInstance)
    {
        if (target == null)
        {
            Debug.LogWarning("DamageEffect: No target found");
            return;
        }

        int finalDamage = baseDamage;

        // Apply strength scaling
        if (scalesWithStrength)
        {
            int strengthBonus = CalculateStatBonus(user, StatType.Strength, strengthScaling);
            finalDamage += strengthBonus;
        }

        // Apply intelligence scaling
        if (scalesWithIntelligence)
        {
            int intelligenceBonus = CalculateStatBonus(user, StatType.Intelligence, intelligenceScaling);
            finalDamage += intelligenceBonus;
        }

        // Apply damage multiplier
        finalDamage = Mathf.RoundToInt(finalDamage * damageMultiplier);

        target.TakeDamage(finalDamage, user.weapon, user.transform.position);
        Debug.Log($"{user.characterName} dealt {finalDamage} damage to {target.characterName} with {abilityInstance.definition.abilityName}");
    }
}

/// <summary>
/// Effect: Heal the user or target
/// </summary>
[System.Serializable]
public class HealEffect : AbilityEffect
{
    [Tooltip("Base amount to heal")]
    public int healAmount = 20;

    [Tooltip("If true, healAmount is a percentage of max health")]
    public bool isPercentage = false;

    [Tooltip("Should healing scale with a stat?")]
    public bool scalesWithStat = false;

    [Tooltip("Which stat to scale healing with")]
    public StatType scalingStat = StatType.Intelligence;

    [Tooltip("Stat scaling multiplier")]
    public float scalingMultiplier = 0.3f;

    public override void Execute(CharacterBehavior user, CharacterBehavior target, AbilityInstance abilityInstance)
    {
        int finalHeal = healAmount;

        // Apply percentage calculation
        if (isPercentage)
        {
            finalHeal = Mathf.RoundToInt(user.initialHealth * (healAmount / 100f));
        }

        // Apply stat scaling
        if (scalesWithStat)
        {
            int statBonus = CalculateStatBonus(user, scalingStat, scalingMultiplier);
            finalHeal += statBonus;
        }

        user.HealDamage(finalHeal);
        Debug.Log($"{user.characterName} healed for {finalHeal} HP");
    }
}

/// <summary>
/// Effect: Temporarily modify user's stats
/// </summary>
[System.Serializable]
public class StatModifierEffect : AbilityEffect
{
    [Tooltip("Which stats to modify")]
    public List<StatType> statToModify = new List<StatType>();

    [Tooltip("Base amount to add to the stat (can be negative for debuffs)")]
    public int modifierAmount = 5;

    [Tooltip("How long the buff lasts in seconds")]
    public float duration = 5f;

    [Tooltip("Should the buff amount scale with a stat?")]
    public bool scalesWithStat = false;

    [Tooltip("Which stat to scale the buff with")]
    public StatType scalingStat = StatType.Intelligence;

    [Tooltip("Stat scaling multiplier")]
    public float scalingMultiplier = 0.2f;

    public override void Execute(CharacterBehavior user, CharacterBehavior target, AbilityInstance abilityInstance)
    {
        int finalModifier = modifierAmount;

        // Apply stat scaling
        if (scalesWithStat)
        {
            int statBonus = CalculateStatBonus(user, scalingStat, scalingMultiplier);
            finalModifier += statBonus;
        }

        // Apply stat buffs
        foreach (StatType stat in statToModify)
        {
            switch (stat)
            {
                case StatType.Strength:
                    user.strength += finalModifier;
                    break;
                case StatType.Speed:
                    user.speed += finalModifier;
                    user.SetSpeed(user.speed);
                    break;
                case StatType.Intelligence:
                    user.intelligence += finalModifier;
                    break;
            }
        }

        Debug.Log($"{user.characterName} gained +{finalModifier} to {string.Join(", ", statToModify)} for {duration} seconds");

        // Start coroutine to remove buff after duration
        user.StartCoroutine(RemoveBuffAfterDuration(user, finalModifier, duration));
    }

    private IEnumerator RemoveBuffAfterDuration(CharacterBehavior user, int finalModifier, float duration)
    {
        yield return new WaitForSeconds(duration);

        // Remove stat buffs
        foreach (StatType stat in statToModify)
        {
            switch (stat)
            {
                case StatType.Strength:
                    user.strength -= finalModifier;
                    break;
                case StatType.Speed:
                    user.speed -= finalModifier;
                    user.SetSpeed(user.speed);
                    break;
                case StatType.Intelligence:
                    user.intelligence -= finalModifier;
                    break;
            }
        }

        Debug.Log($"{user.characterName} lost {string.Join(", ", statToModify)} buff");
    }
}

/// <summary>
/// Effect: Apply knockback to target
/// </summary>
[System.Serializable]
public class KnockbackEffect : AbilityEffect
{
    [Tooltip("Base knockback distance")]
    public float knockbackDistance = 3f;

    [Tooltip("Duration of the knockback")]
    public float knockbackDuration = 0.3f;

    [Tooltip("Should knockback distance scale with a stat?")]
    public bool scalesWithStat = false;

    [Tooltip("Which stat to scale knockback with")]
    public StatType scalingStat = StatType.Strength;

    [Tooltip("Stat scaling multiplier (adds to knockback distance)")]
    public float scalingMultiplier = 0.05f;

    public override void Execute(CharacterBehavior user, CharacterBehavior target, AbilityInstance abilityInstance)
    {
        if (target == null) return;

        float finalDistance = knockbackDistance;

        // Apply stat scaling
        if (scalesWithStat)
        {
            float statBonus = CalculateStatBonus(user, scalingStat, scalingMultiplier);
            finalDistance += statBonus;
        }

        Vector3 direction = (target.transform.position - user.transform.position).normalized;
        target.StartKnockback(direction, finalDistance, knockbackDuration);
        Debug.Log($"{user.characterName} knocked back {target.characterName} by {finalDistance} units");
    }
}

/// <summary>
/// Effect: Apply damage over time
/// </summary>
[System.Serializable]
public class DamageOverTimeEffect : AbilityEffect
{
    [Tooltip("Base damage per tick")]
    public int damagePerTick = 5;

    [Tooltip("Time between damage ticks")]
    public float tickInterval = 1f;

    [Tooltip("Total duration of the effect")]
    public float duration = 5f;

    [Tooltip("Should damage scale with a stat?")]
    public bool scalesWithStat = false;

    [Tooltip("Which stat to scale damage with")]
    public StatType scalingStat = StatType.Intelligence;

    [Tooltip("Stat scaling multiplier (adds to damage per tick)")]
    public float scalingMultiplier = 0.3f;

    public override void Execute(CharacterBehavior user, CharacterBehavior target, AbilityInstance abilityInstance)
    {
        if (target == null) return;

        int finalDamagePerTick = damagePerTick;

        // Apply stat scaling
        if (scalesWithStat)
        {
            int statBonus = CalculateStatBonus(user, scalingStat, scalingMultiplier);
            finalDamagePerTick += statBonus;
        }

        user.StartCoroutine(ApplyDOT(user, target, finalDamagePerTick, duration));
    }

    private IEnumerator ApplyDOT(CharacterBehavior user, CharacterBehavior target, int damagePerTick, float duration)
    {
        float elapsed = 0f;
        int tickCount = 0;

        Debug.Log($"[DamageOverTimeEffect] {user.characterName} applied DOT to {target.characterName} ({damagePerTick} damage every {tickInterval}s for {duration}s)");

        while (elapsed < duration && target != null && target.health > 0)
        {
            yield return new WaitForSeconds(tickInterval);

            if (target != null && target.health > 0)
            {
                target.TakeDamage(damagePerTick, user.weapon, user.transform.position);
                tickCount++;
                elapsed += tickInterval;
            }
        }

        Debug.Log($"[DamageOverTimeEffect] DOT finished - dealt {damagePerTick} damage {tickCount} times (Total: {damagePerTick * tickCount})");
    }
}