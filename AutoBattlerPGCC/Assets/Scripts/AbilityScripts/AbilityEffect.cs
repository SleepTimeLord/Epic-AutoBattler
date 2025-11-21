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
    }
}

/// <summary>
/// Effect: Temporarily modify stats (can buff allies or debuff enemies)
/// </summary>
[System.Serializable]
public class StatModifierEffect : AbilityEffect
{
    public enum ModifierTarget
    {
        User,      // Affects the ability user (self-buff)
        Target,    // Affects the target (enemy debuff or ally buff)
        AllAllies, // Affects all allies in range
        AllEnemies // Affects all enemies in range
    }

    [Tooltip("Who to apply the stat modifier to")]
    public ModifierTarget modifierTarget = ModifierTarget.User;

    [Tooltip("Radius for AllAllies or AllEnemies (0 = infinite range)")]
    public float aoeRadius = 0f;

    [Tooltip("Which stats to modify")]
    public List<StatType> statToModify = new List<StatType>();

    [Tooltip("Base amount to add to the stat (can be negative for debuffs)")]
    public int modifierAmount = 5;

    [Tooltip("How long the modifier lasts in seconds")]
    public float duration = 5f;

    [Tooltip("Should the modifier amount scale with a stat?")]
    public bool scalesWithStat = false;

    [Tooltip("Which stat to scale the modifier with")]
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

        // Determine who to affect based on target type
        switch (modifierTarget)
        {
            case ModifierTarget.User:
                ApplyModifierToCharacter(user, user, finalModifier, duration);
                break;

            case ModifierTarget.Target:
                if (target != null)
                {
                    ApplyModifierToCharacter(user, target, finalModifier, duration);
                }
                else
                {
                    Debug.LogWarning("[StatModifierEffect] Target is null!");
                }
                break;

            case ModifierTarget.AllAllies:
                ApplyModifierToAllies(user, finalModifier, duration);
                break;

            case ModifierTarget.AllEnemies:
                ApplyModifierToEnemies(user, finalModifier, duration);
                break;
        }
    }

    private void ApplyModifierToCharacter(CharacterBehavior user, CharacterBehavior target, int finalModifier, float duration)
    {
        // Apply stat modifiers
        foreach (StatType stat in statToModify)
        {
            switch (stat)
            {
                case StatType.Strength:
                    target.strength += finalModifier;
                    break;
                case StatType.Speed:
                    target.speed += finalModifier;
                    target.SetSpeed(target.speed);
                    break;
                case StatType.Intelligence:
                    target.intelligence += finalModifier;
                    break;
            }
        }

        string modType = finalModifier >= 0 ? "buff" : "debuff";

        // Start coroutine to remove modifier after duration
        user.StartCoroutine(RemoveModifierAfterDuration(user, target, finalModifier, duration));
    }

    private void ApplyModifierToAllies(CharacterBehavior user, int finalModifier, float duration)
    {
        GameObject[] allies = GameObject.FindGameObjectsWithTag(user.characterType.ToString());
        int affectedCount = 0;

        foreach (GameObject allyObj in allies)
        {
            CharacterBehavior ally = allyObj.GetComponent<CharacterBehavior>();
            if (ally == null) continue;

            // Check radius (0 = infinite range)
            if (aoeRadius > 0f)
            {
                float distance = Vector3.Distance(user.transform.position, ally.transform.position);
                if (distance > aoeRadius) continue;
            }

            ApplyModifierToCharacter(user, ally, finalModifier, duration);
            affectedCount++;
        }

        Debug.Log($"[StatModifierEffect] Applied modifier to {affectedCount} allies");
    }

    private void ApplyModifierToEnemies(CharacterBehavior user, int finalModifier, float duration)
    {
        // Get opposite team tag
        string enemyTag = user.characterType == CharacterType.Ally ? "Enemy" : "Ally";
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        int affectedCount = 0;

        foreach (GameObject enemyObj in enemies)
        {
            CharacterBehavior enemy = enemyObj.GetComponent<CharacterBehavior>();
            if (enemy == null) continue;

            // Check radius (0 = infinite range)
            if (aoeRadius > 0f)
            {
                float distance = Vector3.Distance(user.transform.position, enemy.transform.position);
                if (distance > aoeRadius) continue;
            }

            ApplyModifierToCharacter(user, enemy, finalModifier, duration);
            affectedCount++;
        }

        Debug.Log($"[StatModifierEffect] Applied modifier to {affectedCount} enemies");
    }

    private IEnumerator RemoveModifierAfterDuration(CharacterBehavior user, CharacterBehavior target, int finalModifier, float duration)
    {
        yield return new WaitForSeconds(duration);

        // Check if target still exists (might have died)
        if (target == null || target.gameObject == null)
        {
            yield break;
        }

        // Remove stat modifiers
        foreach (StatType stat in statToModify)
        {
            switch (stat)
            {
                case StatType.Strength:
                    target.strength -= finalModifier;
                    break;
                case StatType.Speed:
                    target.speed -= finalModifier;
                    target.SetSpeed(target.speed);
                    break;
                case StatType.Intelligence:
                    target.intelligence -= finalModifier;
                    break;
            }
        }

        string modType = finalModifier >= 0 ? "buff" : "debuff";
        Debug.Log($"[StatModifierEffect] {target.characterName} lost {modType} on {string.Join(", ", statToModify)}");
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
    }
}

/// <summary>
/// Effect: Dash backwards and become invisible
/// </summary>
[System.Serializable]
public class DashBackInvisEffect : AbilityEffect
{
    [Tooltip("Distance to dash backwards")]
    public float dashDistance = 3f;

    [Tooltip("Duration of dash movement")]
    public float dashDuration = 0.3f;

    [Tooltip("Duration of invisibility")]
    public float invisDuration = 2f;

    [Tooltip("Next attack bonus damage")]
    public int bonusDamage = 20;

    [Tooltip("Should bonus damage scale with a stat?")]
    public bool scalesWithStat = true;

    [Tooltip("Which stat to scale with")]
    public StatType scalingStat = StatType.Intelligence;

    [Tooltip("Stat scaling multiplier")]
    public float scalingMultiplier = 0.5f;

    public override void Execute(CharacterBehavior user, CharacterBehavior target, AbilityInstance abilityInstance)
    {
        user.StartCoroutine(ExecuteDashBackInvis(user, target));
    }

    private IEnumerator ExecuteDashBackInvis(CharacterBehavior user, CharacterBehavior target)
    {
        // Calculate final bonus damage
        int finalBonus = bonusDamage;
        if (scalesWithStat)
        {
            int statBonus = CalculateStatBonus(user, scalingStat, scalingMultiplier);
            finalBonus += statBonus;
        }

        // Get dash direction (away from attacker)
        Vector3 dashDirection;
        if (target != null)
        {
            dashDirection = (user.transform.position - target.transform.position).normalized;
        }
        else
        {
            dashDirection = -user.transform.forward;
        }

        // Perform dash
        yield return user.StartCoroutine(PerformDash(user, dashDirection, dashDistance, dashDuration));

        // Apply invisibility
        user.ApplyInvisibility(invisDuration);

        // Apply next attack bonus
        user.ApplyNextAttackBonus(finalBonus);
    }

    private IEnumerator PerformDash(CharacterBehavior user, Vector3 direction, float distance, float duration)
    {
        Vector3 startPos = user.transform.position;
        Vector3 endPos = startPos + direction * distance;
        float elapsed = 0f;

        // Disable NavMeshAgent during dash
        if (user.agent != null && user.agent.enabled)
        {
            user.agent.enabled = false;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            user.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        user.transform.position = endPos;

        // Re-enable NavMeshAgent
        if (user.agent != null && !user.agent.enabled)
        {
            user.agent.enabled = true;
            if (user.agent.isOnNavMesh)
            {
                user.agent.Warp(user.transform.position);
            }
        }
    }
}

/// <summary>
/// Effect: Stun the target
/// </summary>
[System.Serializable]
public class StunEffect : AbilityEffect
{
    [Tooltip("Stun duration in seconds")]
    public float stunDuration = 0.5f;

    [Tooltip("Should stun duration scale with a stat?")]
    public bool scalesWithStat = false;

    [Tooltip("Which stat to scale with")]
    public StatType scalingStat = StatType.Intelligence;

    [Tooltip("Stat scaling multiplier (adds seconds to stun)")]
    public float scalingMultiplier = 0.01f;

    public override void Execute(CharacterBehavior user, CharacterBehavior target, AbilityInstance abilityInstance)
    {
        if (target == null) return;

        float finalDuration = stunDuration;
        if (scalesWithStat)
        {
            float statBonus = CalculateStatBonus(user, scalingStat, scalingMultiplier);
            finalDuration += statBonus;
        }

        Debug.Log($"[StunEffect] {user.characterName} stunned {target.characterName} for {finalDuration:F2}s");
        target.ApplyStun(finalDuration);
    }
}

/// <summary>
/// Effect: Apply a visual particle effect
/// </summary>
[System.Serializable]
public class ParticleEffect : AbilityEffect
{
    public enum ParticleTarget
    {
        User,
        Target,
        UserWeapon,
        TargetWeapon
    }

    [Tooltip("Where to spawn the particle")]
    public ParticleTarget particleTarget = ParticleTarget.Target;

    [Tooltip("Particle prefab to spawn")]
    public GameObject particlePrefab;

    [Tooltip("Duration (0 = particle's lifetime)")]
    public float duration = 0f;

    [Tooltip("Should particle follow the target?")]
    public bool followTarget = true;

    [Tooltip("Local position offset")]
    public Vector3 positionOffset = Vector3.zero;

    public override void Execute(CharacterBehavior user, CharacterBehavior target, AbilityInstance abilityInstance)
    {
        if (particlePrefab == null)
        {
            Debug.LogWarning("[ParticleEffect] No particle prefab assigned!");
            return;
        }

        Transform spawnTransform = null;

        switch (particleTarget)
        {
            case ParticleTarget.User:
                spawnTransform = user.transform;
                break;
            case ParticleTarget.Target:
                spawnTransform = target != null ? target.transform : null;
                break;
            case ParticleTarget.UserWeapon:
                spawnTransform = user.currentWeapon != null ? user.currentWeapon.transform : user.transform;
                break;
            case ParticleTarget.TargetWeapon:
                spawnTransform = target != null && target.currentWeapon != null ? target.currentWeapon.transform : null;
                break;
        }

        if (spawnTransform == null)
        {
            Debug.LogWarning($"[ParticleEffect] Could not find spawn transform for {particleTarget}");
            return;
        }

        GameObject particle = Object.Instantiate(particlePrefab, spawnTransform.position + positionOffset, Quaternion.identity);

        if (followTarget)
        {
            particle.transform.SetParent(spawnTransform);
            particle.transform.localPosition = positionOffset;
        }

        if (duration > 0f)
        {
            Object.Destroy(particle, duration);
        }
        else
        {
            // Use particle system's lifetime
            ParticleSystem ps = particle.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Object.Destroy(particle, ps.main.duration + ps.main.startLifetime.constantMax);
            }
        }

        Debug.Log($"[ParticleEffect] Spawned {particlePrefab.name} on {spawnTransform.name}");
    }
}