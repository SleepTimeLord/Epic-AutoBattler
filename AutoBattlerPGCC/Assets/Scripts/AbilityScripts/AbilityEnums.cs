using UnityEngine;

// defines when passive abilities should trigger
public enum AbilityTrigger
{
    None, // Active abilities only
    OnAttack, // When character attacks
    OnDamaged, // When character takes damage
    OnKill, // When character kills an enemy
    OnSpawn, // When character spawns
    OnDeath, // When character dies
    OnInterval, // Every X seconds
    OnCombo, // On specific combo count (3rd hit, 5th hit, etc.)
    OnHealthThreshold, // When health reaches certain %
    OnCooldownReady // When ability comes off cooldown
}

// defines if ability is passive or active
public enum AbilityType
{
    Passive,
    Active
}

// types of stats that can be modified
public enum StatType
{
    Strength,
    Speed,
    Intelligence
}