using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDefinition", menuName = "Scriptable Objects/WeaponDefinition")]
public class WeaponDefinition : ScriptableObject
{
    public enum WeaponType
    {
        Slash,
        Ranged,
        Peirce
    }

    [Header("Weapon Info")]
    public string weaponName;
    public WeaponType weaponType;
    public GameObject weaponGameObject;
    public Sprite weaponIcon;
    [Header("Weapon Offsets")]
    public Vector3 holdOffset;
    public Vector3 scaleOffset;
    [Header("Weapon Stats")]
    public int damage;
    public float attackSpeed;
    public float range;
    public int additionalCost;

    public virtual void Attack(CharacterBehavior userStats) 
    { 
        
    }
}