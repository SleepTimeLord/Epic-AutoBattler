using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
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
    [SerializeField]
    public NavMeshAgent agent;
    [SerializeField]
    private BehaviorGraphAgent blackboardAgent;
    [SerializeField] private Rigidbody2D rb;
    [Header("Initial Stats")]
    public int intialHealth;
    [Header("Character Info")]
    public string characterName;
    public CharacterDefinition characterDefinition;
    public string instanceID; // Unique identifier for this character instance
    public string description;
    public CharacterType characterType;
    [Header("Stats")]
    public int intelligence;
    public int health;
    public float speed;
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
    public float force = 10f;

    public bool IsBeingKnockedBack;
    public WeaponDefinition attackedWeapon;

    // Add this to your CharacterBehavior script
    public float stillThreshold = 0.05f;
    public float maxKnockbackTime = 0.5f;
    public float forceMultiplierSet = 10f;

    public bool tookDamage = false;

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

        // get NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) 
        {
            Debug.LogWarning("NavMeshAgent component not found on CharacterBehavior.");
        }

        blackboardAgent = GetComponent<BehaviorGraphAgent>();

        SetSpeed(characterDefinition.speed);

        // Get Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found! Knockback will fail.");
        }

        // IMPORTANT: Ensure the Rigidbody is set to not affect position initially
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
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
        currentWeapon.GetComponent<Collider2D>().enabled = false; // disable collider by default
    }

    public void Attack(InputAction.CallbackContext context) 
    {
        weapon.Attack(this);
    }

    public int GetAttackDamage()
    {
        return (int)(weapon.damage + (strength * 0.1f));
    }

    public void TakeDamage(int damageAmount, WeaponDefinition weapon, Vector3 attackPos) 
    {
        attackedWeapon = weapon;
        tookDamage = true;
        health -= damageAmount;


        if (health <= 0) 
        {
            // Handle character death
            return;
        }
    }

    // --- StartKnockback Method (No Change Needed) ---
    public void StartKnockback(Vector3 direction, float distance, float duration)
    {
        if (IsBeingKnockedBack) return;

        IsBeingKnockedBack = true;

        StartCoroutine(KnockbackRoutine(direction.normalized, distance, duration));
    }

    IEnumerator KnockbackRoutine(Vector3 direction, float distance, float duration)
    {
        // Direction calculation (from the video: use the direction of the hit)
        Vector3 dir = direction.normalized;

        // Calculate an initial force (Impulse Force)
        // You can fine-tune this multiplier. The video uses a very high force * distance.
        // A simple formula: Mass * (Distance / Duration) * Multiplier
        float forceMultiplier = forceMultiplierSet; // Tweak this for strength
        float impulseForce = rb.mass * (distance / duration) * forceMultiplier;

        // --- 1. CRITICAL SETUP: Disable Agent & Enable Physics ---
        if (agent != null && agent.isOnNavMesh)
        {
            // Stop pathing logic FIRST (prevents SetDestination errors)
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            // Fully disable the component to gain direct physics control
            agent.enabled = false;
        }

        if (rb != null)
        {
            // Switch to Dynamic/Active Physics Control
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;

            // Apply the force as a single instantaneous IMPULSE
            rb.AddForce(dir * impulseForce, ForceMode2D.Impulse);
        }

        float knockbackStartTime = Time.time;

        // Wait until velocity is near zero OR the max time is exceeded
        yield return new WaitUntil(() =>
        {
            bool hasStopped = rb.linearVelocity.magnitude < stillThreshold;

            bool timeExpired = Time.time > knockbackStartTime + maxKnockbackTime;

            return hasStopped || timeExpired;
        });

        if (rb != null)
        {
            // Restore Rigidbody to passive state
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (agent != null)
        {
            // Re-enable the component
            agent.enabled = true;

            // Wait one frame for the Agent to re-initialize
            yield return null;

            if (agent.isOnNavMesh)
            {
                // Sync position, essential after physics movement
                agent.Warp(transform.position);

                // Resume movement
                agent.isStopped = false;
            }
        }

        IsBeingKnockedBack = false;
        tookDamage = false;
    }

    public void SetSpeed(float speed) 
    {
        agent.speed = speed * .05f;
    }

    // reference in animation event
    public void ApplyAttackSlowSpeed() 
    { 
        agent.speed = agent.speed - weapon.attackSlow;
    }

    public void ResetSpeed() 
    { 
        agent.speed = 1 + (characterDefinition.speed * .05f);
    }

    public float GetWeaponRange()
    {
        return weapon.range;
    }

    public Transform GetClosestTarget() 
    {
        if (characterType == CharacterType.Ally)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemies.Length == 0)
            {
                return null;
            }
            GameObject closestEnemy = null;
            float closestDistance = Mathf.Infinity;
            Vector3 currentPosition = transform.position;
            foreach (GameObject enemy in enemies)
            {
                float distance = Vector3.Distance(currentPosition, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
            return closestEnemy.transform;
        }
        else if (characterType == CharacterType.Enemy)
        {
            GameObject[] allies = GameObject.FindGameObjectsWithTag("Ally");
            if (allies.Length == 0)
            {
                return null;
            }
            GameObject closestAlly = null;
            float closestDistance = Mathf.Infinity;
            Vector3 currentPosition = transform.position;
            foreach (GameObject ally in allies)
            {
                float distance = Vector3.Distance(currentPosition, ally.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestAlly = ally;
                }
            }
            return closestAlly.transform;
        }
        else 
        {
            // should not reach here
            return null;
        }
    }

    public bool IsTargetInWeaponRange()
    {
        Transform target = GetClosestTarget();
        if (target == null)
        {
            return false;
        }
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        return distanceToTarget <= GetWeaponRange();
    }
}