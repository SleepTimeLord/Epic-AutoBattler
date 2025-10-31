using UnityEngine;

[CreateAssetMenu(fileName = "RampingBloodlustAbility", menuName = "Abilities/RampingBloodlustAbility")]
public class RampingBloodlustAbility : AbilityDefinition
{
    public int damagePerStack;
    public int intelligencePerStack;
    public int speedPerStack;
    public int maxStacks;

    public override void ActivateAbility(CharacterBehavior user)
    {
        base.ActivateAbility(user);
        // every kill increases damage, intelligence, and speed by defined amounts, up to maxStacks
        Debug.Log($"{user.name} activates Ramping Bloodlust, gaining up to {maxStacks} stacks of increased damage, intelligence, and speed");
    }
}
