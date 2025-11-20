using UnityEngine;
using UnityEngine.InputSystem;

public class CreateAndSpawn : MonoBehaviour
{
    [SerializeField]
    private InputActionReference createAction, spawnAction, createEnemyAction;
    [Header("For Character Creation")]
    public CharacterDefinition characterDefinition;
    public WeaponDefinition weaponDefinition;
    public AbilityDefinition[] abilityDefinition;
    [Header("For Character Spawning")]
    public Transform characterSpawn;
    public string characterInstanceID;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        createAction.action.performed += CreateCharacter;
        spawnAction.action.performed += SpawnEnemyCharacter;
        createEnemyAction.action.performed += CreateEnemyCharacter;
    }

    private void OnDisable()
    {
        createAction.action.performed -= CreateCharacter;
        spawnAction.action.performed -= SpawnEnemyCharacter;
        createEnemyAction.action.performed -= CreateEnemyCharacter;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CreateCharacter(InputAction.CallbackContext context)
    {
        CharacterCreate character = CharacterCreate.CreateFromDefinition(characterDefinition, weaponDefinition, abilityDefinition, CharacterType.Ally);
        characterInstanceID = character.instanceID;
        print("Created character: " + character.characterName + character.instanceID);
    }
    private void CreateEnemyCharacter(InputAction.CallbackContext context) 
    {
        CharacterCreate character = CharacterCreate.CreateFromDefinition(characterDefinition, weaponDefinition, abilityDefinition, CharacterType.Enemy);
        characterInstanceID = character.instanceID;
        print("Created character: " + character.characterName + character.instanceID);
    }
    private void SpawnEnemyCharacter(InputAction.CallbackContext context)
    {
        // gets card from deck and summons them
        CardSetter card = SummonManager.Instance.GetCardFromDeck(1, CharacterType.Enemy);
        if (card != null) 
        {
            StartCoroutine(SummonManager.Instance.SummonEnemyCharacter(card.instanceID));
        }
        else 
        {
            Debug.Log("cant find card");
        }
    }
}
