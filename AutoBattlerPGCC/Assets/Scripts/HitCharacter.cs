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
        CharacterBehavior character = collision.GetComponent<CharacterBehavior>();
        if (character != null)
        {
            Debug.Log($"Hit character: {character.characterName}");
            // Here you can add logic to apply damage or effects to the character
        }
    }
}
