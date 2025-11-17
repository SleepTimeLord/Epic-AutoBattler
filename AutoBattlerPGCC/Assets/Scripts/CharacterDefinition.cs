using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEditor;
using UnityEditor.U2D;
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
    public GameObject characterTopdown;
    public Sprite characterSprite;
    [Header("Character Stats")]
    public int intelligence;
    public int health;
    public float speed;
    public int strength;
    public int cost;
    [Header("Character Abilities")]
    public AbilityDefinition[] uniqueAbilities;
    [Header("Character Card")]
    public GameObject characterCard;
}
