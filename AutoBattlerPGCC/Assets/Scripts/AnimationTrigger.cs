using UnityEditor.Rendering;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    private CharacterBehavior characterBehavior;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        characterBehavior = transform.GetComponentInParent<CharacterBehavior>();
        if (characterBehavior == null)
        {
            Debug.LogWarning("CharacterBehavior component not found in parent during AnimationTrigger Awake.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Use this public method that Animation Events can call directly.
    public void SetWeaponFlip(int flipYValue)
    {

        if (characterBehavior == null)
        {
            Debug.LogWarning("CharacterBehavior component not found in parent when attempting to flip weapon.");
            return;
        }
        // Animation events only support boolean or int parameters.
        // We convert the int (0 or 1) back to a boolean.
        bool flipTo = flipYValue != 0;

        if (characterBehavior.currentWeapon != null)
        {
            SpriteRenderer sr = characterBehavior.currentWeapon.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.flipY = flipTo;
            }
            else
            {
                Debug.LogWarning("Current weapon is missing a SpriteRenderer when attempting to flip.");
            }
        }
    }

    public void EnableWeaponCollider(int enableValue) 
    { 
        Transform weapon = characterBehavior.currentWeapon.transform;
        
        if (weapon == null)
            {
            Debug.LogWarning("Current weapon transform not found when attempting to enable collider.");
            return;
        }

        bool enable = enableValue != 0;

        weapon.GetComponent<Collider2D>().enabled = enable;
    }
}
