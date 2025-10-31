using System;
using UnityEngine;

public class CharacterCreate
{
    public CharacterDefinition characterDefinition;
    public WeaponDefinition weaponDefinition;
    public AbilityDefinition abilityDefinition;

    public string instanceID; // Unique identifier for this character instance
    public string characterDefinitionID; // Reference to CharacterDefinition GUID
    public string characterName;
    public string description;
    public GameObject characterIcon;
    public int intelligence;
    public int health;
    public int speed;
    public int strength;
    public int cost;
    public AbilityDefinition[] abilities;
    public WeaponDefinition weapon;

    // Factory method to create CharacterCreate from CharacterDefinition, WeaponDefinition, and PassiveDefinition array
    public static CharacterCreate CreateFromDefinition(CharacterDefinition charDef, WeaponDefinition weaponDef, AbilityDefinition[] abilitiesDef)
    {
        if (charDef == null)
        {
            Debug.LogError("CharacterCreate.CreateFromDefinition: charDef is null");
            return null;
        }
        if (weaponDef == null)
        {
            Debug.LogError("CharacterCreate.CreateFromDefinition: weaponDef is null");
            return null;
        }
        if (abilitiesDef == null)
        {
            Debug.LogError("CharacterCreate.CreateFromDefinition: passiveDef is null");
            return null;
        }

        var character = new CharacterCreate
        {
            instanceID = Guid.NewGuid().ToString(), // assign a new unique ID for runtime instance
            // Assign properties from CharacterDefinition
            characterDefinition = charDef,
            characterName = charDef.characterName,
            description = charDef.description,
            characterIcon = charDef.characterIcon,
            intelligence = charDef.intelligence,
            health = charDef.health,
            speed = charDef.speed,
            strength = charDef.strength,
            cost = charDef.cost,
            // Assign passive and weapon from parameters
            abilities = abilitiesDef,
            weapon = weaponDef,
        };

        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.AddAllyCharacter(character);
        }
        else
        {
            Debug.LogError("CharacterCreate.CreateFromDefinition: CharacterManager instance is null");
        }

        return character;
    }
}
