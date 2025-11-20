using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SlashDefinition", menuName = "Weapons/SlashDefinition")]
public class SlashDefinition : WeaponDefinition
{
    [Header("WeaponAnimation")]
    public RuntimeAnimatorController attackAnimation;
    public string rightAttackName;
    public string leftAttackName;

    // TODO: Refactor to use Animation Events instead of Coroutine for better performance and reliability
    // TODO: apply attack speed modifier for the weapon and character stats
    public override void Attack(CharacterBehavior userStats)
    {
        if (userStats.attackAnimation == null || userStats.currentWeapon == null)
        {
            Debug.LogError("Missing Animator or currentWeapon. Cannot attack.");
            return;
        }

        // Set the weapon's animation controller
        userStats.attackAnimation.runtimeAnimatorController = attackAnimation;

        var spriteRenderer = userStats.currentWeapon.GetComponent<SpriteRenderer>();

        // change speed based on weapon base attack speed and character speed stat
        float attackSpeedModifier = attackSpeed + (userStats.speed * 0.01f);

        // Apply to Animator
        userStats.attackAnimation.SetFloat("AttackSpeed", attackSpeedModifier);

        // Determine direction and play animation
        // The flip logic (setting flipY) will now be handled by the Animation Event defined on the clip itself.
        if (spriteRenderer != null && !spriteRenderer.flipY)
        {
            userStats.attackAnimation.Play(rightAttackName);
        }
        else
        {
            userStats.attackAnimation.Play(leftAttackName);
        }
    }
}
