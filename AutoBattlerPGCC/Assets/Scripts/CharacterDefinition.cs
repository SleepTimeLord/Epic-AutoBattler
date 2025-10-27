using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDefinition", menuName = "Scriptable Objects/CharacterDefinition")]
public class CharacterDefinition : ScriptableObject
{
    [Header("Character Info")]
    public string characterName;
    public string description;
    public GameObject characterIcon;
    [Header("Character Stats")]
    public int intelligence;
    public int health;
    public int speed;
    public int strength;
    public int cost;
    [Header("Character Abilities")]
    public PassiveDefinition[] passive;
    public WeaponDefinition weapon;
}
