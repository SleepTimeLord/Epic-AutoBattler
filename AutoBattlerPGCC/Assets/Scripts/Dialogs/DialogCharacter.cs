using UnityEngine;

[CreateAssetMenu(fileName = "DialogCharacter", menuName = "Scriptable Objects/DialogCharacter")]
public class DialogCharacter : ScriptableObject
{
    [SerializeField] 
    public string characterName;
}
