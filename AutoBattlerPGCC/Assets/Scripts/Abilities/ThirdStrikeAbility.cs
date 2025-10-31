using UnityEngine;

[CreateAssetMenu(fileName = "ThirdStrikeAbility", menuName = "Abilities/ThirdStrikeAbility")]
public class ThirdStrikeAbility : AbilityDefinition
{
    public int damage;

    public override void ActivateAbility(GameObject user)
    {
        base.ActivateAbility(user);

        // every third attack deals extra damage

    }
}
