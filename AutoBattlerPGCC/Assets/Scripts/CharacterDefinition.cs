using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDefinition", menuName = "Scriptable Objects/CharacterDefinition")]
public class CharacterDefinition : ScriptableObject
{
    [SerializeField]
    private string guid;
    public string GUID => guid;

#if UNITY_EDITOR
    public void OnValidate()
    {
        if (string.IsNullOrEmpty(guid))
        {
            guid = System.Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }
    }
#endif

    [Header("Character Info")]
    public string characterName;
    public string description;
    public GameObject characterIcon;
    [Header("Character Stats")]
    public int intelligence;
    public int health;
    public int speed;
    public int strength;
    public int cost;
    [Header("Character Abilities")]
    public AbilityDefinition[] abilities;
    public WeaponDefinition weapon;
}
