using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CardPickerManager : MonoBehaviour
{
    public static CardPickerManager Instance { get; private set; }

    [Header("UI Containers")]
    public GameObject weaponPickerContainer;
    public GameObject abilityPickerContainer;

    // Current picking state
    private CardPicker selectedCard;
    private WeaponDefinition selectedWeapon;
    private List<AbilityDefinition> selectedAbilities = new List<AbilityDefinition>();

    public bool picked = false;

    [Header("Settings")]
    public int maxAbilities = 2;

    public int characterCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnCardPicked(CardPicker card)
    {
        selectedCard = card;
        selectedAbilities.Clear();

        // Show weapon picker
        weaponPickerContainer.SetActive(true);
    }

    public void OnWeaponPicked(WeaponDefinition weapon)
    {
        selectedWeapon = weapon;
        weaponPickerContainer.SetActive(false);

        // Show ability picker
        abilityPickerContainer.SetActive(true);
    }

    public void OnAbilityPicked(AbilityDefinition ability)
    {
        if (!selectedAbilities.Contains(ability))
        {
            selectedAbilities.Add(ability);

            // Check if we have enough abilities
            if (selectedAbilities.Count >= maxAbilities)
            {
                FinalizePicking();
            }
        }
    }

    private void FinalizePicking()
    {
        abilityPickerContainer.SetActive(false);

        // Create the player data
        PlayerData newPlayer = new PlayerData
        {
            characterDefinition = selectedCard.characterDefinition,
            weaponDefinition = selectedWeapon,
            abilityDefinition = selectedAbilities.ToArray()
        };

        // Save to inventory
        InventoryScript.Instance.playerData.Add(newPlayer);

        Debug.Log($"Saved character to inventory: {newPlayer.characterDefinition.characterName}");

        // Clean up
        ResetPicking();
    }

    private void ResetPicking()
    {
        Destroy(selectedCard.gameObject);
        picked = false;
        selectedCard = null;
        selectedWeapon = null;
        selectedAbilities.Clear();
        characterCount += 1;

        if (characterCount == 2)
        {
            GoNextScene();
        }
    }

    private void GoNextScene()
    {
        PickEnemy();
        SceneManager.LoadScene(2);
    }

    private void PickEnemy()
    {
        List<EnemyData> enemiesToRemove = new List<EnemyData>();

        foreach (EnemyData ed in InventoryScript.Instance.enemyData)
        {
            foreach (PlayerData pd in InventoryScript.Instance.playerData)
            {
                if (pd.characterDefinition == ed.characterDefinition)
                {
                    // Mark enemy for removal if it matches a player character
                    enemiesToRemove.Add(ed);
                    break;
                }
            }
        }

        // Remove all marked enemies
        foreach (EnemyData enemyToRemove in enemiesToRemove)
        {
            InventoryScript.Instance.enemyData.Remove(enemyToRemove);
        }
    }

}