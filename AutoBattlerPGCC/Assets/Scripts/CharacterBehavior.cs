using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEditor.Rendering;
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
    [Header("NavMeshAgent")]
    public NavMeshAgent agent;

    [Header("Behavior")]
    [SerializeField] private BehaviorGraphAgent blackboardAgent;

    [Header("Ability System")]
    private AbilityManager abilityManager;
    public Bar healthBar;

    [SerializeField] private Rigidbody2D rb;

    [Header("Initial Stats")]
    public int initialHealth;

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

    [Header("Knockback Settings")]
    public float stillThreshold = 0.05f;
    public float maxKnockbackTime = 0.5f;
    public float forceMultiplierSet = 10f;

    public bool tookDamage = false;

    [Header("Dodge System")]
    [Tooltip("Base dodge chance percentage (0-100)")]
    public float baseDodgeChance = 5f;

    [Tooltip("Dodge chance per point of speed")]
    public float dodgePerSpeed = 0.5f;

    [Tooltip("Maximum dodge chance cap")]
    public float maxDodgeChance = 75f;

    [Header("Status Effects")]
    private bool isInvisible = false;
    private bool isStunned = false;
    private int nextAttackBonusDamage = 0;
    private float lastActiveAbilityTime = -999f;

    [Header("Visual Effects")]
    private GameObject currentStunParticle;
    private GameObject currentInvisParticle;

    [Header("Ability Tracking")]
    private string lastAbilityUsed = "";
    private float lastAbilityTime = -999f;
    private bool isProcessingAbility = false;

    private void Awake()
    {
        initialHealth = characterDefinition.health;

        healthBar.SetMax(initialHealth);
        healthBar.Change(-(initialHealth - health));

        // if ally set to ally if enemy set enemy
        gameObject.tag = characterType.ToString();

        // sets up ability manager
        abilityManager = gameObject.GetComponent<AbilityManager>();
        if (abilityManager == null)
        {
            abilityManager = gameObject.AddComponent<AbilityManager>();
        }

        // sets up the singleton instance
        Transform weaponContainer = transform.Find("Arms/WeaponContainer");
        if (weaponContainer == null)
        {
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

    private void Start()
    {
        // initialize abilities after everything is setup with the character
        if (abilityManager != null)
        {
            abilityManager.InitializeAbilities();
        }
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

    /// <summary>
    /// Get total attack damage including bonuses
    /// </summary>
    public int GetAttackDamage()
    {
        int baseDamage = (int)(weapon.damage + (strength * 0.1f));

        // Add bonus damage from abilities (like Shadow Step)
        if (nextAttackBonusDamage > 0)
        {
            Debug.Log($"[Attack] {characterName} dealing bonus {nextAttackBonusDamage} damage!");
            baseDamage += nextAttackBonusDamage;
            nextAttackBonusDamage = 0; // Consume the bonus
        }

        return baseDamage;
    }


    /// <summary>
    /// Calculate dodge chance based on speed
    /// </summary>
    public float GetDodgeChance()
    {
        float dodgeChance = baseDodgeChance + (speed * dodgePerSpeed);
        return Mathf.Min(dodgeChance, maxDodgeChance);
    }

    /// <summary>
    /// Attempt to dodge an incoming attack
    /// Returns true if dodged
    /// </summary>
    public bool TryDodge()
    {
        float dodgeChance = GetDodgeChance();
        float roll = Random.Range(0f, 100f);
        bool dodged = roll <= dodgeChance;

        if (dodged)
        {
            // You can trigger OnDodge abilities here if you add that trigger type
            // abilityManager?.TriggerAbilities(AbilityTrigger.OnDodge);
        }

        return dodged;
    }

    /// <summary>
    /// Mark that an ability is being processed (recursion guard)
    /// </summary>
    public void BeginAbilityProcessing(string abilityName)
    {
        isProcessingAbility = true;
    }

    /// <summary>
    /// Mark that ability processing is complete
    /// </summary>
    public void EndAbilityProcessing(string abilityName)
    {
        isProcessingAbility = false;
    }

    /// <summary>
    /// Check if currently processing an ability (prevents recursion)
    /// </summary>
    public bool IsProcessingAbility()
    {
        return isProcessingAbility;
    }

    /// <summary>
    /// Track when ANY ability is used
    /// </summary>
    public void OnAbilityUsed(string abilityName, AbilityType abilityType)
    {
        lastAbilityUsed = abilityName;
        lastAbilityTime = Time.time;

        // trigger OnUseAbility if we're not already processing an ability
        if (!isProcessingAbility)
        {
            abilityManager?.TriggerAbilities(AbilityTrigger.OnUseAbility);
        }
    }

    /// <summary>
    /// Get the name of the last ability used
    /// </summary>
    public string GetLastAbilityUsed()
    {
        return lastAbilityUsed;
    }

    /// <summary>
    /// Get time since last ability was used
    /// Returns -1 if never used
    /// </summary>
    public float GetTimeSinceLastAbility()
    {
        if (lastAbilityTime < 0f) return -1f;
        return Time.time - lastAbilityTime;
    }

    /// <summary>
    /// Take damage from an attack
    /// </summary>
    public void TakeDamage(int damageAmount, WeaponDefinition weapon, Vector3 attackPos)
    {
        // Check if stunned (can't dodge while stunned)
        if (!isStunned && TryDodge())
        {
            // Optional: Show "DODGE" text or particle effect
            return;
        }

        attackedWeapon = weapon;
        tookDamage = true;
        healthBar.Change(-damageAmount);
        health -= damageAmount;

        // trigger OnDamaged abilities
        abilityManager?.TriggerAbilities(AbilityTrigger.OnDamaged);

        // trigger OnHealthThreshold abilities
        abilityManager?.TriggerAbilities(AbilityTrigger.OnHealthThreshold);

        if (health <= 0)
        {
            // trigger OnDeath abilities
            abilityManager?.TriggerAbilities(AbilityTrigger.OnDeath);

            // Start kill sequence from CharacterManager (persistent object)
            CharacterManager.Instance.StartKillCharacterCoroutine(instanceID, characterType);
        }
    }

    /// <summary>
    /// Heal the character
    /// </summary>
    public void HealDamage(int healAmount)
    {
        healthBar.Change(healAmount);
        health = Mathf.Min(health + healAmount, initialHealth);
    }

    /// <summary>
    /// Called when character successfully hits a target
    /// </summary>
    public void OnAttackHit(CharacterBehavior target)
    {
        // Increment combo counters for combo-based abilities
        abilityManager?.IncrementCombos();

        // Trigger OnAttack abilities
        abilityManager?.TriggerAbilities(AbilityTrigger.OnAttack, target);

        // Check if target died from this attack
        if (target != null && target.health <= 0)
        {
            // Trigger OnKill abilities
            abilityManager?.TriggerAbilities(AbilityTrigger.OnKill, target);
        }
    }

    /// <summary>
    /// Manually activate an active ability by index
    /// </summary>
    public void UseActiveAbility(int abilityIndex)
    {
        abilityManager?.ActivateActiveAbility(abilityIndex);
    }

    /// <summary>
    /// Reset combo counters (e.g., when too much time between attacks)
    /// </summary>
    public void ResetCombo()
    {
        abilityManager?.ResetAllCombos();
    }

    /// <summary>
    /// Apply invisibility effect
    /// </summary>
    public void ApplyInvisibility(float duration)
    {
        if (isInvisible) return;

        StartCoroutine(InvisibilityRoutine(duration));
    }

    private IEnumerator InvisibilityRoutine(float duration)
    {
        isInvisible = true;

        // Collect all sprite renderers (self + children)
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        // Store originals
        Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();

        gameObject.tag = "Untagged";

        foreach (var r in renderers)
        {
            if (r == null) continue;
            originalColors[r] = r.color;

            // Apply semi-transparent alpha
            Color c = r.color;
            c.a = 0.7f;
            r.color = c;
        }

        // Wait
        yield return new WaitForSeconds(duration);

        // Restore original visibility
        foreach (var kvp in originalColors)
        {
            if (kvp.Key == null) continue; // renderer was destroyed
            kvp.Key.color = kvp.Value;
        }
        gameObject.tag = characterType.ToString();

        // Destroy invis particle if needed
        if (currentInvisParticle != null)
            Destroy(currentInvisParticle);

        isInvisible = false;
    }

    /// <summary>
    /// Apply stun effect
    /// </summary>
    public void ApplyStun(float duration)
    {
        if (isStunned) return;

        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;

        // Stop movement
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
        }

        yield return new WaitForSeconds(duration);

        // Resume movement
        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
        }

        if (currentStunParticle != null)
        {
            Destroy(currentStunParticle);
        }

        isStunned = false;
    }

    /// <summary>
    /// Apply bonus damage to next attack
    /// </summary>
    public void ApplyNextAttackBonus(int bonusDamage)
    {
        nextAttackBonusDamage += bonusDamage;
    }

    /// <summary>
    /// Track when active abilities are used
    /// </summary>
    public void OnActiveAbilityUsed()
    {
        lastActiveAbilityTime = Time.time;
        Debug.Log($"[ActiveAbility] {characterName} used active ability at {Time.time}");
    }

    /// <summary>
    /// Get time since last active ability was used
    /// Returns -1 if never used
    /// </summary>
    public float GetTimeSinceLastActiveAbility()
    {
        if (lastActiveAbilityTime < 0f) return -1f;
        return Time.time - lastActiveAbilityTime;
    }

    /// <summary>
    /// Check if character is currently stunned
    /// </summary>
    public bool IsStunned()
    {
        return isStunned;
    }

    /// <summary>
    /// Check if character is currently invisible
    /// </summary>
    public bool IsInvisible()
    {
        return isInvisible;
    }

    /// <summary>
    /// Start knockback effect
    /// </summary>
    public void StartKnockback(Vector3 direction, float distance, float duration)
    {
        if (IsBeingKnockedBack)
        {
            Debug.Log($"[Knockback] {characterName} already being knocked back, ignoring");
            return;
        }

        IsBeingKnockedBack = true;
        StartCoroutine(KnockbackRoutine(direction.normalized, distance, duration));
    }

    IEnumerator KnockbackRoutine(Vector3 direction, float distance, float duration)
    {
        Vector3 dir = new Vector3(direction.x, direction.y, 0f).normalized;

        // Store starting position for debugging
        Vector3 startPosition = transform.position;

        // Disable NavMeshAgent
        bool wasAgentEnabled = false;
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            wasAgentEnabled = true;
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }

        // Enable physics
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f; // Important: No gravity for 2D top-down

            // Calculate force - UPDATED FORMULA
            // Using a much simpler and more reliable calculation
            float speed = distance / duration;
            Vector2 knockbackVelocity = new Vector2(dir.x, dir.y) * speed;

            // Apply velocity directly instead of force (more reliable)
            rb.linearVelocity = knockbackVelocity;

        }
        else
        {
            IsBeingKnockedBack = false;
            yield break;
        }

        float elapsed = 0f;

        // Wait for knockback to complete (using duration or until stopped)
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Optional: Apply drag to slow down over time
            if (rb != null && rb.linearVelocity.magnitude > 0.1f)
            {
                rb.linearVelocity *= 0.95f; // Gradual slowdown
            }

            yield return null;
        }

        // Calculate actual distance moved
        float actualDistance = Vector3.Distance(startPosition, transform.position);

        // Restore physics state
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Restore NavMeshAgent
        if (agent != null && wasAgentEnabled)
        {
            agent.enabled = true;
            yield return null; // Wait a frame for agent to initialize

            if (agent.isOnNavMesh)
            {
                agent.Warp(transform.position);
                agent.isStopped = false;
            }
            else
            {
                Debug.LogWarning($"[Knockback] {characterName} not on NavMesh after knockback!");
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