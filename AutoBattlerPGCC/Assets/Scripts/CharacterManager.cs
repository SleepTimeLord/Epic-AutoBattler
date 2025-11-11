using Microsoft.Unity.VisualStudio.Editor;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }
    public GameObject cardContainer;

    private Dictionary<string, CharacterCreate> allyCharacters = new Dictionary<string, CharacterCreate>();
    private Dictionary<string, CharacterCreate> enemyCharacters = new Dictionary<string, CharacterCreate>();

    private Dictionary<string, GameObject> spawnedCharacters = new Dictionary<string, GameObject>();

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
        SetCharacterCost(character);
        SetCharacterCard(character);
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
        SetCharacterCost(character);
    }

    // Method to get an ally character by instanceID
    public void RemoveAllyCharacter(string instanceID)
    {
        if (allyCharacters.ContainsKey(instanceID))
        {
            CharacterCreate character = allyCharacters[instanceID];
            foreach(Transform card in cardContainer.transform)
            {
                if(card.GetComponent<CardSetter>().instanceID == instanceID)
                {
                    Destroy(card.gameObject); break;
                }
            }
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
    public void SpawnCharacter(string characterID, Vector3 position, CharacterType characterType)
    {
        // find the character in the allyCharacters dictionary
        if (allyCharacters.TryGetValue(characterID, out CharacterCreate allyCharacter))
        {
            CharacterBehavior characterBehavior = allyCharacter.characterTopdown.GetComponent<CharacterBehavior>();

            if (characterBehavior == null)
            {
                Debug.LogError($"SpawnAllyCharacter: Character icon does not have CharacterBehavior component for instanceID {characterID}");
                return;
            }

            // set character stats
            SetCharacterStats(characterBehavior, allyCharacter, characterType);
            // set character abilities
            SetCharacterAbilities(characterBehavior, allyCharacter);
            // set character weapon
            SetCharacterWeapon(characterBehavior, allyCharacter);

            // Instantiate the character's icon at the specified position
            GameObject spawnedAlly = Instantiate(allyCharacter.characterTopdown, position, Quaternion.identity);
            spawnedCharacters[characterID] = spawnedAlly;
        }
        else if (enemyCharacters.TryGetValue(characterID, out CharacterCreate enemyCharacter))
        {
            CharacterBehavior characterBehavior = enemyCharacter.characterTopdown.GetComponent<CharacterBehavior>();

            if (characterBehavior == null)
            {
                Debug.LogError($"SpawnAllyCharacter: Character icon does not have CharacterBehavior component for instanceID {characterID}");
                return;
            }

            // sets everything up
            SetCharacterStats(characterBehavior, enemyCharacter, characterType);
            SetCharacterAbilities(characterBehavior, enemyCharacter);
            SetCharacterWeapon(characterBehavior, enemyCharacter);

            // Instantiate the character's icon at the specified position
            GameObject spawnedEnemy = Instantiate(enemyCharacter.characterTopdown, position, Quaternion.identity);
            spawnedCharacters[characterID] = spawnedEnemy;
        }
        else
        {
            Debug.LogError($"SpawnAllyCharacter: No character found with instanceID {characterID}");
        }
    }

    public void DespawnCharacter(string characterID)
    {
        if (spawnedCharacters.TryGetValue(characterID, out GameObject character))
        {
            Destroy(character);
            spawnedCharacters.Remove(characterID);
        }

    }

    // Sets character stats and cost
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
        characterBehavior.cost = character.cost;
    }

    // calculate and set cost of character
    private void SetCharacterCost(CharacterCreate character)
    {
        character.cost = character.cost + character.weapon.additionalCost;
        if (character.abilities != null)
        {
            foreach(var ability in character.abilities)
            {
                character.cost += ability.additionalCost;
            }
        }
    }

    // Sets character abilities include unique ability
    private void SetCharacterAbilities(CharacterBehavior characterBehavior, CharacterCreate character)
    {
        // set abilities
        if (character.abilities != null)
        {
            foreach (var ability in character.abilities)
            {
                if (ability.abilityType == AbilityType.Passive)
                {
                    // makes sure that there are no duplicate ablilities
                    if (characterBehavior.passiveAbility.Contains(ability))
                    {
                        characterBehavior.passiveAbility.Remove(ability);
                        characterBehavior.passiveAbility.Add(ability);
                    }
                    else
                        characterBehavior.passiveAbility.Add(ability);
                }
                else if (ability.abilityType == AbilityType.Active)
                {
                    if (characterBehavior.activeAbility.Contains(ability))
                    {
                        characterBehavior.activeAbility.Remove(ability);
                        characterBehavior.activeAbility.Add(ability);
                    }
                    else
                        characterBehavior.activeAbility.Add(ability);
                }
                else
                {
                    Debug.LogWarning($"SetCharacterAbilities: Unknown ability type for ability {ability.abilityName} in character {character.characterName}");
                    continue;
                }
            }
        }
    }

    private void SetCharacterWeapon(CharacterBehavior characterBehavior, CharacterCreate character)
    {
        // set weapon
        characterBehavior.weapon = character.weapon;
    }

    // set character card and instantiates an instance of it into a container
    private void SetCharacterCard(CharacterCreate character)
    {
        CardSetter cardSetter = character.characterCard.GetComponent<CardSetter>();

        if (cardSetter != null)
        {
            if (character.uniqueAbilities == null)
            {
                Debug.LogError("no uniqueAbilities found");
            }
            else if (character.abilities == null)
            {
                Debug.LogError("no regularAbilities found");
            }
            else 
            {
                cardSetter.instanceID = character.instanceID;
                cardSetter.characterDescriptionText.text = character.description;
                cardSetter.characterCost.text = character.cost.ToString();
                cardSetter.characterHealth.text = character.health.ToString();
                cardSetter.swordIcon.sprite = character.weapon.weaponIcon;
                cardSetter.uniqueSkillPlaceholder.sprite = character.uniqueAbilities[0].abilityIcon;

                // makes sure that uniqueAbility icon is not the same as regular ability
                foreach (var ability in character.abilities)
                {
                    if (ability == character.uniqueAbilities[0])
                    {
                        continue;
                    }
                    else 
                    {
                        cardSetter.regularSkillPlaceholder.sprite = ability.abilityIcon;
                        break;
                    }
                }

                GameObject card = Instantiate(character.characterCard);
                card.transform.SetParent(cardContainer.transform, false);
            }
        }
    }

    public CharacterCreate GetCharacter(string InstanceID) {
        if (allyCharacters.TryGetValue(InstanceID, out CharacterCreate value))
        {
            return value;
        }
        else {
            Debug.LogError("No character in ally characters that have that instanceID");
            return null;
        }
    }
}