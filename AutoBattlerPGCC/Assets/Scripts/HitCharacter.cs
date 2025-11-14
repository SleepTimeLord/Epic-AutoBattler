using UnityEngine;

public class HitCharacter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CharacterBehavior user = GetComponentInParent<CharacterBehavior>();
        CharacterBehavior character = collision.GetComponent<CharacterBehavior>();
        if (character != null)
        {
            // Here you can add logic to apply damage or effects to the character
            character.TakeDamage(user.GetAttackDamage());
        }
    }
}
