using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData 
{
    public CharacterDefinition characterDefinition;
    public WeaponDefinition weaponDefinition;
    public AbilityDefinition[] abilityDefinition;
}

[System.Serializable]
public class EnemyData 
{
    public CharacterDefinition characterDefinition;
    public WeaponDefinition weaponDefinition;
    public AbilityDefinition[] abilityDefinition;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public List<PlayerData> playerData = new List<PlayerData>();
    public List<EnemyData> enemyData = new List<EnemyData>();

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

    private void Start()
    {
        // each platerData it spawn an instance of a character
        foreach (PlayerData p in playerData)
        {
            CreateCharacter(p.characterDefinition, p.weaponDefinition, p.abilityDefinition, CharacterType.Ally);
        }

        // do same for enemy data
        foreach (EnemyData e in enemyData)
        {
            CreateCharacter(e.characterDefinition, e.weaponDefinition, e.abilityDefinition, CharacterType.Enemy);
        }
    }

    private void CreateCharacter(CharacterDefinition cd, WeaponDefinition wd, AbilityDefinition[] ad, CharacterType ct) 
    {
        CharacterCreate character = CharacterCreate.CreateFromDefinition(cd, wd, ad, ct);
        print("Created character: " + character.characterName + character.instanceID);
    }

    public void CheckIfWinOrLose(Dictionary<string, CharacterCreate> deadCharacters, CharacterType characterType) 
    {
        if (deadCharacters.Count == playerData.Count || characterType == CharacterType.Ally)
        {
            Debug.Log("You lose");
        }
        else if (deadCharacters.Count == playerData.Count || characterType == CharacterType.Enemy) 
        {
            Debug.Log("You Win");
        }
    }
}
