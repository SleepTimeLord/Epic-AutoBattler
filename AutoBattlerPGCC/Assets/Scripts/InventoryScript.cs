using System.Collections;
using UnityEngine;

public class InventoryScript : MonoBehaviour
{
    public static ArrayList PlayerInventory;
    public static ArrayList EnemyInventory;
    private static InventoryScript Instance;
    
    public void createCharacterInInventory(
        CharacterDefinition charDef,  
        WeaponDefinition wepDef, 
        AbilityDefinition[] abilDef,  
        CharacterType charType)
    {

        //InventoryScript.PlayerInventory.Add(CharacterCreate.CreateFromDefinition(, , , CharacterType.Ally)); //////////////////////////////////////////

    }
    public void createEnemyInInventory(
        CharacterDefinition charDef,  
        WeaponDefinition wepDef, 
        AbilityDefinition[] abilDef,  
        CharacterType charType)
    {

        //CharacterCreate.CreateFromDefinition(, , , CharacterType.Ally); ///////////////////////////////////////////////////////////////////////////////

    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerInventory ??= new ArrayList();
        EnemyInventory ??= new  ArrayList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
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
    }
    
}
