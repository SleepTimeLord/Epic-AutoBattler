using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public List<PlayerData> playerDataGM = new List<PlayerData>();
    public List<EnemyData> enemyDataGM = new List<EnemyData>();

    public EnemyAI enemyAI;

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

        playerDataGM = InventoryScript.Instance.playerData;
        enemyDataGM = InventoryScript.Instance.enemyData;
    }

    private void Start()
    {
        // each platerData it spawn an instance of a character
        foreach (PlayerData p in playerDataGM)
        {
            CreateCharacter(p.characterDefinition, p.weaponDefinition, p.abilityDefinition, CharacterType.Ally);
        }

        // do same for enemy data
        foreach (EnemyData e in enemyDataGM)
        {
            CreateCharacter(e.characterDefinition, e.weaponDefinition, e.abilityDefinition, CharacterType.Enemy);
        }

        StartCoroutine(enemyAI.SpawnEnemyRoutine());
    }

    private void CreateCharacter(CharacterDefinition cd, WeaponDefinition wd, AbilityDefinition[] ad, CharacterType ct) 
    {
        CharacterCreate character = CharacterCreate.CreateFromDefinition(cd, wd, ad, ct);
        print("Created character: " + character.characterName + character.instanceID);
    }

    public void CheckIfWinOrLose(Dictionary<string, CharacterCreate> deadCharacters, CharacterType characterType) 
    {
        if (deadCharacters.Count == playerDataGM.Count && characterType == CharacterType.Ally)
        {
            Debug.Log("You lose");
            SceneManager.LoadScene(4);
        }
        else if (deadCharacters.Count == enemyDataGM.Count && characterType == CharacterType.Enemy) 
        {
            Debug.Log("You Win");
            SceneManager.LoadScene(3);
        }
    }
}
