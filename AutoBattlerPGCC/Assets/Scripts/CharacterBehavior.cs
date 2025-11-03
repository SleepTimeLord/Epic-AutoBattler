using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

// This class handles character behavior in the game.
// It is intended to be attached to character GameObject found in CharacterDefinition.
// calls CharacterCreate to instantiate character data.

public enum CharacterType
{
    Ally,
    Enemy
}
public class CharacterBehavior : MonoBehaviour
{
    public int intialHealth;
    [Header("Character Info")]
    public string characterName;
    public string instanceID; // Unique identifier for this character instance
    public string description;
    public CharacterType characterType;
    [Header("Stats")]
    public int intelligence;
    public int health;
    public int speed;
    public int strength;
    public int cost;
    [Header("Character Abilities")]
    public List<AbilityDefinition> passiveAbility;
    public List<AbilityDefinition> activeAbility;
    [Header("Weapon")]
    public WeaponDefinition weapon;
    [Header("CharacterCard")]
    public GameObject characterCard;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        // sets up the singleton instance
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}