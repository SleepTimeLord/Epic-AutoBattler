using UnityEngine;

[CreateAssetMenu(fileName = "GoodEatsAbility", menuName = "Abilities/GoodEatsAbility")]
public class GoodEatsAbility : AbilityDefinition
{
    public int healthRestored;
    public int uses;

    public override void ActivateAbility(GameObject user)
    {
        base.ActivateAbility(user);
        // Implement Good Eats specific logic here
        Debug.Log($"{user.name} uses Good Eats to restore {healthRestored} health!");
        // Example: user.GetComponent<Health>().RestoreHealth(healthRestored);
    }
}
