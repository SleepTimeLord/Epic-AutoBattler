using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.TextCore.Text;

// This class handles character behavior in the game.
// It is intended to be attached to character GameObject found in CharacterDefinition.
// calls CharacterCreate to instantiate character data.

public enum CharacterType
{
    Ally,
    Enemy
}
public class CharacterBehavior : MonoBehaviour
{
    [SerializeField]
    private InputActionReference testAttack;
    [Header("Initial Stats")]
    public int intialHealth;
    [Header("Character Info")]
    public string characterName;
    public string instanceID; // Unique identifier for this character instance
    public string description;
    public CharacterType characterType;
    [Header("Stats")]
    public int intelligence;
    public int health;
    public int speed;
    public int strength;
    public int cost;
    [Header("Character Abilities")]
    public List<AbilityDefinition> passiveAbility;
    public List<AbilityDefinition> activeAbility;
    [Header("Weapon")]
    public WeaponDefinition weapon;
    public GameObject currentWeapon;
    [Header("CharacterCard")]
    public GameObject characterCard;
    [Header("CharacterAnimations")]
    public Animator attackAnimation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        // sets up the singleton instance
        Transform weaponContainer = transform.Find("Arms/WeaponContainer");
        if (weaponContainer == null) {
            Debug.LogWarning("WeaponContainer not found on CharacterBehavior.");
            return; 
        
        }
        // if there is no weapon instantiate it
        if (weaponContainer.childCount == 0) 
        {
            // instantiate new weapon
            AttachWeapon(weaponContainer);
        }
        // if there is a weapon delete previous and instantiate new one
        else
        {
            Debug.Log("Deleting Previous Weapon");
            foreach (Transform child in weaponContainer)
            {
                Destroy(child.gameObject);
            }
            AttachWeapon(weaponContainer);
        }

        attackAnimation = GetComponentInChildren<Animator>();
        if (attackAnimation == null) 
        {
            Debug.LogWarning("Animator component not found on CharacterBehavior.");
        }
    }

    private void OnEnable()
    {
        testAttack.action.performed += Attack;
    }
    private void OnDisable()
    {
        testAttack.action.performed -= Attack;
    }

    private void AttachWeapon(Transform weaponContainer)
    {
        // instantiate new weapon
        currentWeapon = Instantiate(weapon.weaponGameObject, weaponContainer);
        currentWeapon.transform.localPosition = weapon.holdOffset;
        currentWeapon.transform.localRotation = Quaternion.identity;
        currentWeapon.transform.localScale = weapon.scaleOffset;
    }

    private void Update()
    {

    }

    private void Attack(InputAction.CallbackContext context) 
    {
        weapon.Attack(this);
    }

    public void TakeDamage(int damageAmount) 
    {
        health -= damageAmount;
        if (health <= 0) 
        {
            // Handle character death
            return;
        }
    }
}