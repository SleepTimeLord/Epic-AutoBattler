using UnityEngine;

[CreateAssetMenu(fileName = "DialogLine", menuName = "Scriptable Objects/Dialog/Line")]
public class DialogLine : ScriptableObject
{
    [SerializeField] 
    public DialogCharacter Speaker;

    [SerializeField] 
    public string Text;
    
    
}
