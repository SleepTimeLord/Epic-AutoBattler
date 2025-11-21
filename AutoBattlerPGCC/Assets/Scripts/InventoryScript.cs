using System.Collections;
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

public class InventoryScript : MonoBehaviour
{
    public List<PlayerData> playerData = new List<PlayerData>();
    public List<EnemyData> OGEnemyData = new List<EnemyData>();
    public List<EnemyData> enemyData = new List<EnemyData>();

    public static InventoryScript Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        OGEnemyData = enemyData;
    }
}