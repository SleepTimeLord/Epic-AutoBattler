using UnityEngine;
using UnityEngine.InputSystem;

public class CreateAndSpawn : MonoBehaviour
{
    [SerializeField]
    private InputActionReference createAction, spawnAction;
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
        spawnAction.action.performed += SpawnCharacter;
    }

    private void OnDisable()
    {
        createAction.action.performed -= CreateCharacter;
        spawnAction.action.performed -= SpawnCharacter;
    }

    // Update is called once per frame
    void Update()
    {
/*        // Create character
        if (Input.GetKeyDown(KeyCode.C))
        {
            CharacterCreate character = CharacterCreate.CreateFromDefinition(characterDefinition, weaponDefinition, abilityDefinition);
            characterInstanceID = character.instanceID;
            print("Created character: " + character.characterName + character.instanceID);

        }

        // Spawn character
        if (Input.GetKeyDown(KeyCode.S))
        {
            CharacterManager.Instance.SpawnAllyCharacter(characterInstanceID, characterSpawn.position);
        }*/
    }

    private void CreateCharacter(InputAction.CallbackContext context)
    {
        CharacterCreate character = CharacterCreate.CreateFromDefinition(characterDefinition, weaponDefinition, abilityDefinition);
        characterInstanceID = character.instanceID;
        print("Created character: " + character.characterName + character.instanceID);
    }
    private void SpawnCharacter(InputAction.CallbackContext context)
    {
        CharacterManager.Instance.SpawnAllyCharacter(characterInstanceID, characterSpawn.position);
    }
}
