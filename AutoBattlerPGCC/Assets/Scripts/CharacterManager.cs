using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.tvOS;

// TODO: make enemy container inside the scene so the enemies can acutally spawn too
public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }
    public GameObject cardContainer;
    public GameObject enemyCardContainer;

    public EnemyAI enemyAI;

    private Dictionary<string, CharacterCreate> allyCharacters = new Dictionary<string, CharacterCreate>();
    private Dictionary<string, CharacterCreate> enemyCharacters = new Dictionary<string, CharacterCreate>();

    public Dictionary<string, GameObject> spawnedCharacters = new Dictionary<string, GameObject>();

    private Dictionary<string, CharacterCreate> deadAllyCharacters = new Dictionary<string, CharacterCreate>();
    private Dictionary<string, CharacterCreate> deadEnemyCharacters = new Dictionary<string, CharacterCreate>();

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
            return;
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
        CharacterBehavior cb = character.characterTopdown.GetComponent<CharacterBehavior>();
        cb.characterType = CharacterType.Ally;
        cb.gameObject.tag = "Ally";
        SetCharacterCost(character);
        SetCharacterCard(character);
        SpawnCharacterCard(character);
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
        CharacterBehavior cb = character.characterTopdown.GetComponent<CharacterBehavior>();
        cb.characterType = CharacterType.Enemy;
        cb.gameObject.tag = "Enemy";
        SetCharacterCost(character);
        SetCharacterCard(character);
        SpawnCharacterCard(character);
    }

    public void RemoveAllyCharacter(string instanceID)
    {
        if (!allyCharacters.ContainsKey(instanceID))
        {
            Debug.LogWarning($"no ally found with instanceID {instanceID}");
            return;
        }

        CharacterCreate character = allyCharacters[instanceID];

        // Remove the UI card
        foreach (Transform card in cardContainer.transform)
        {
            CardSetter setter = card.GetComponent<CardSetter>();

            if (setter != null && setter.instanceID == instanceID)
            {
                Destroy(card.gameObject);
                break;
            }
        }

        // Remove from dictionary
        allyCharacters.Remove(instanceID);
    }

    // Method to get an enemy character by instanceID
    public void RemoveEnemyCharacter(string instanceID)
    {
        if (!enemyCharacters.ContainsKey(instanceID))
        {
            Debug.LogWarning($"no enemy found with instanceID {instanceID}");
            return;
        }

        CharacterCreate character = enemyCharacters[instanceID];

        // Remove the UI card
        foreach (Transform card in enemyCardContainer.transform)
        {
            CardSetter setter = card.GetComponent<CardSetter>();

            if (setter != null && setter.instanceID == instanceID)
            {
                Destroy(card.gameObject);
                break;
            }
        }

        // Remove from dictionary
        enemyCharacters.Remove(instanceID);
    }

    // CHANGED: Added wrapper method to start coroutine from CharacterManager instead of CharacterBehavior
    // This is critical because if we start the coroutine from CharacterBehavior and then destroy that GameObject,
    // the coroutine will be killed mid-execution and RemoveAllyCharacter will never be called
    public void StartKillCharacterCoroutine(string instanceID, CharacterType characterType)
    {
        StartCoroutine(KillCharacter(instanceID, characterType));
    }

    // handle the kill sequence:
    public IEnumerator KillCharacter(string instanceID, CharacterType characterType)
    {
        if (characterType == CharacterType.Ally)
        {
            if (!allyCharacters.ContainsKey(instanceID))
            {
                Debug.LogWarning($"no ally found with ID {instanceID}");
                yield break;
            }

            if (spawnedCharacters.ContainsKey(instanceID))
            {
                // destroy the physical character GameObject first
                DespawnCharacter(instanceID, characterType);

                // animate the card back to hand (reparents but doesn't clear currentSummoned)
                yield return StartCoroutine(SummonManager.Instance.DesummonCard(instanceID, characterType));

                // clear the currentSummoned reference so a new character can be summoned
                SummonManager.Instance.ClearCurrentSummoned(characterType);
            }

            // move to dead pool and remove from active dictionaries
            deadAllyCharacters[instanceID] = GetCharacter(instanceID, characterType);
            RemoveAllyCharacter(instanceID);
            // check if character won or not
            GameManager.Instance.CheckIfWinOrLose(deadAllyCharacters, characterType);
        }
        else
        {
            if (!enemyCharacters.ContainsKey(instanceID))
            {
                Debug.LogWarning($"no enemy found with ID {instanceID}");
                yield break;
            }

            if (spawnedCharacters.ContainsKey(instanceID))
            {
                // Same sequence for enemies
                DespawnCharacter(instanceID, characterType);
                yield return StartCoroutine(SummonManager.Instance.DesummonCard(instanceID, characterType));

                // ADDED - Clear enemy reference
                SummonManager.Instance.ClearCurrentSummoned(characterType);
            }

            deadEnemyCharacters[instanceID] = GetCharacter(instanceID, characterType);
            RemoveEnemyCharacter(instanceID);
            GameManager.Instance.CheckIfWinOrLose(deadEnemyCharacters, characterType);
            // spawns new enemy when done
            StartCoroutine(enemyAI.SpawnEnemyRoutine());
        }
    }

    // to respawn characters theoretically idk if work
    public void RespawnCharacter(string instanceID, CharacterType characterType)
    {
        if (characterType == CharacterType.Ally)
        {
            if (deadAllyCharacters.TryGetValue(instanceID, out CharacterCreate character))
            {
                // reset to base
                SetCharacterBaseStats(character, character.characterDefinition);
                // update card
                UpdateCharacterCard(instanceID, character.characterCard.GetComponent<CardSetter>(), characterType);
                // bring card to board
                SpawnCharacterCard(character);
            }
            else
            {
                Debug.LogWarning("cant find dead ally");
            }
        }
        else
        {
            if (deadEnemyCharacters.TryGetValue(instanceID, out CharacterCreate character))
            {
                // reset to base
                SetCharacterBaseStats(character, character.characterDefinition);
                // update card
                UpdateCharacterCard(instanceID, character.characterCard.GetComponent<CardSetter>(), characterType);
                // bring card to board
                SpawnCharacterCard(character);
            }
            else
            {
                Debug.LogWarning("cant find dead enemy");
            }
        }
    }

    public void SetCharacterBaseStats(CharacterCreate character, CharacterDefinition characterBase)
    {
        character.health = characterBase.health;
        character.intelligence = characterBase.intelligence;
        character.speed = characterBase.speed;
        character.strength = characterBase.strength;
    }

    // to spawn an ally character from dictionary in the game world
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

    public void DespawnCharacter(string characterID, CharacterType characterType)
    {
        if (spawnedCharacters.TryGetValue(characterID, out GameObject character))
        {
            // update stats one last time
            UpdateCharacterStats(characterID, GetCharacter(characterID, characterType), character.GetComponent<CharacterBehavior>(), characterType);
            // destroy the physical object
            Destroy(character);
            // remove from dictionary
            spawnedCharacters.Remove(characterID);
        }
        else
        {
            Debug.Log("Cant find the character to despawn");
        }
    }

    // Sets character stats and cost
    private void SetCharacterStats(CharacterBehavior characterBehavior, CharacterCreate character, CharacterType characterType)
    {
        // set character type and info
        characterBehavior.characterType = characterType;
        characterBehavior.characterDefinition = character.characterDefinition;
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

    public void UpdateCharacterStats(string characterID, CharacterCreate character, CharacterBehavior characterBehavior, CharacterType characterType)
    {
        if (characterType == CharacterType.Ally)
        {
            if (allyCharacters.TryGetValue(characterID, out CharacterCreate allyCharacter))
            {
                if (characterBehavior != null)
                {
                    // update stats
                    allyCharacter.intelligence = characterBehavior.intelligence;
                    allyCharacter.health = characterBehavior.health;
                    allyCharacter.speed = characterBehavior.speed;
                    allyCharacter.strength = characterBehavior.strength;
                    allyCharacter.cost = characterBehavior.cost;
                }
                else
                {
                    Debug.LogError($"UpdateCharacterStats: No CharacterBehavior found on spawned character with instanceID {characterID}");
                }
            }
            else
            {
                Debug.LogError($"UpdateCharacterStats: No spawned character found with instanceID {characterID}");
            }
        }
        else
        {
            if (enemyCharacters.TryGetValue(characterID, out CharacterCreate enemyCharacter))
            {
                if (characterBehavior != null)
                {
                    // update stats
                    enemyCharacter.intelligence = characterBehavior.intelligence;
                    enemyCharacter.health = characterBehavior.health;
                    enemyCharacter.speed = characterBehavior.speed;
                    enemyCharacter.strength = characterBehavior.strength;
                    enemyCharacter.cost = characterBehavior.cost;
                }
                else
                {
                    Debug.LogError($"UpdateCharacterStats: No CharacterBehavior found on spawned character with instanceID {characterID}");
                }
            }
            else
            {
                Debug.LogError($"UpdateCharacterStats: No spawned character found with instanceID {characterID}");
            }
        }
    }

    // calculate and set cost of character
    private void SetCharacterCost(CharacterCreate character)
    {
        character.cost = character.cost + character.weapon.additionalCost;
        if (character.abilities != null)
        {
            foreach (var ability in character.abilities)
            {
                character.cost += ability.additionalCost;
            }
        }
    }

    // Sets character abilities include unique ability
    private void SetCharacterAbilities(CharacterBehavior characterBehavior, CharacterCreate character)
    {
        // clear all abilities 
        characterBehavior.activeAbility.Clear();
        characterBehavior.passiveAbility.Clear();
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

    public void UpdateCharacterCard(string instanceID, CardSetter card, CharacterType characterType)
    {
        CharacterCreate character = GetCharacter(instanceID, characterType);
        card.characterDescriptionText.text = character.description;
        card.characterCost.text = character.cost.ToString();
        card.characterHealth.text = character.health.ToString();
        card.swordIcon.sprite = character.weapon.weaponIcon;
        card.uniqueSkillPlaceholder.sprite = character.uniqueAbilities[0].abilityIcon;
        // makes sure that uniqueAbility icon is not the same as regular ability
        foreach (var ability in character.abilities)
        {
            if (ability == character.uniqueAbilities[0])
            {
                continue;
            }
            else
            {
                card.regularSkillPlaceholder.sprite = ability.abilityIcon;
                break;
            }
        }
    }

    // instantiate an instance of character card into a container
    private void SpawnCharacterCard(CharacterCreate character)
    {
        CharacterBehavior characterBehavior = character.characterTopdown.GetComponent<CharacterBehavior>();
        if (characterBehavior == null)
        {
            Debug.LogWarning("No character behavior detected");
            return;
        }

        if (characterBehavior.characterType == CharacterType.Ally)
        {
            GameObject card = Instantiate(character.characterCard);
            card.transform.SetParent(cardContainer.transform, false);
        }
        else
        {
            GameObject card = Instantiate(character.characterCard);
            card.transform.SetParent(enemyCardContainer.transform, false);
        }
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
            }
        }
    }

    public CharacterCreate GetCharacter(string InstanceID, CharacterType characterType)
    {
        if (characterType == CharacterType.Ally)
        {
            if (allyCharacters.TryGetValue(InstanceID, out CharacterCreate value))
            {
                return value;
            }
            else
            {
                Debug.LogError("No character in ally characters that have that instanceID");
                return null;

            }
        }
        else if (characterType == CharacterType.Enemy)
        {
            if (enemyCharacters.TryGetValue(InstanceID, out CharacterCreate value))
            {
                return value;
            }
            else
            {
                Debug.LogError("No character in enemy characters that have that instanceID");
                return null;
            }
        }
        else
        {
            return null;
        }
    }
}