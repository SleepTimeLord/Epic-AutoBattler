using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    private Dictionary<string, CharacterCreate> allyCharacters = new Dictionary<string, CharacterCreate>();
    private Dictionary<string, CharacterCreate> enemyCharacters = new Dictionary<string, CharacterCreate>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to add an ally character
    public void AddAllyCharacter(CharacterCreate character)
    {
        if (character == null) { Debug.LogError("AddAllyCharacter: character is null"); return; }

        if (string.IsNullOrEmpty(character.instanceID))
        {
            Debug.LogError("AddAllyCharacter: character instanceID is null or empty");
            return;
        }

        allyCharacters[character.instanceID] = character;
    }

    // Method to add an enemy character
    public void AddEnemyCharacter(CharacterCreate character)
    {
        if (character == null) { Debug.LogError("AddEnemyCharacter: character is null"); return; }
        if (string.IsNullOrEmpty(character.instanceID))
        {
            Debug.LogError("AddEnemyCharacter: character instanceID is null or empty");
            return;
        }
        enemyCharacters[character.instanceID] = character;
    }

    // Method to get an ally character by instanceID
    public void RemoveAllyCharacter(string instanceID)
    {
        if (allyCharacters.ContainsKey(instanceID))
        {
            allyCharacters.Remove(instanceID);
        }
        else
        {
            Debug.LogWarning($"RemoveAllyCharacter: No ally character found with instanceID {instanceID}");
        }
    }

    // Method to get an enemy character by instanceID
    public void RemoveEnemyCharacter(string instanceID)
    {
        if (enemyCharacters.ContainsKey(instanceID))
        {
            enemyCharacters.Remove(instanceID);
        }
        else
        {
            Debug.LogWarning($"RemoveEnemyCharacter: No enemy character found with instanceID {instanceID}");
        }
    }

    // Method to spawn an ally character from dictionary in the game world
    public void SpawnAllyCharacter(string characterID, Vector3 position)
    {
        // find the character in the allyCharacters dictionary
        if (allyCharacters.TryGetValue(characterID, out CharacterCreate character))
        {
            CharacterBehavior characterBehavior = character.characterIcon.GetComponent<CharacterBehavior>();

            if (characterBehavior == null)
            {
                Debug.LogError($"SpawnAllyCharacter: Character icon does not have CharacterBehavior component for instanceID {characterID}");
                return;
            }

            // set character stats
            SetCharacterStats(characterBehavior, character, CharacterType.Ally);

            // Instantiate the character's icon at the specified position
            Instantiate(character.characterIcon, position, Quaternion.identity);
        }
        else
        {
            Debug.LogError($"SpawnAllyCharacter: No ally character found with instanceID {characterID}");
        }
    }

    public void SpawnEnemyCharacter(string characterID, Vector3 position)
    {
        // find the character in the enemyCharacters dictionary
        if (enemyCharacters.TryGetValue(characterID, out CharacterCreate character))
        {
            CharacterBehavior characterBehavior = character.characterIcon.GetComponent<CharacterBehavior>();

            if (characterBehavior == null)
            {
                Debug.LogError($"SpawnEnemyCharacter: Character icon does not have CharacterBehavior component for instanceID {characterID}");
                return;
            }

            // set character stats
            SetCharacterStats(characterBehavior, character, CharacterType.Enemy);

            // Instantiate the character's icon at the specified position
            Instantiate(character.characterIcon, position, Quaternion.identity);
        }

        else
        {
            Debug.LogError($"SpawnEnemyCharacter: No enemy character found with instanceID {characterID}");
        }
    }

    private void SetCharacterStats(CharacterBehavior characterBehavior, CharacterCreate character, CharacterType characterType)
    {
        // set character type and info
        characterBehavior.characterType = characterType;
        characterBehavior.characterName = character.characterName;
        characterBehavior.instanceID = character.instanceID;
        characterBehavior.description = character.description;
        // set stats
        characterBehavior.intelligence = character.intelligence;
        characterBehavior.health = character.health;
        characterBehavior.speed = character.speed;
        characterBehavior.strength = character.strength;
        // calculate and set cost of character
        characterBehavior.cost = character.cost + character.weapon.additionalCost;
        if (character.abilities != null)
        {
            foreach (var ability in character.abilities)
            {
                characterBehavior.cost += ability.additionalCost;
            }
        }  
        // set abilities
        characterBehavior.ability = character.abilities;
        characterBehavior.weapon = character.weapon;
    }
}