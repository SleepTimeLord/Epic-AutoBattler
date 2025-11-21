using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class HitCharacter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private CharacterBehavior attacker;
    private void Awake()
    {
        attacker = GetComponentInParent<CharacterBehavior>();

        if (attacker == null) 
        {
            Debug.LogError("Please add character behavior to parent object");
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") || collision.CompareTag("Ally")) 
        {
            CharacterBehavior target = collision.GetComponent<CharacterBehavior>();
            if (attacker != null && target != null && target.characterType != attacker.characterType)
            {
                int damage = attacker.GetAttackDamage();
                // Here you can add logic to apply damage or effects to the character
                target.TakeDamage(damage, attacker.weapon, attacker.transform.position);
            }

            // triggers OnAttackerHit for combos
            attacker.OnAttackHit(target);

        }

    }
}
