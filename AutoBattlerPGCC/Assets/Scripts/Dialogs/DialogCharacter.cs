using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogCharacter", menuName = "Dialog Objects/DialogCharacter")]
public class DialogCharacter : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField] 
    public List<DialogLine> dialogLines = new List<DialogLine>();

    [Header("Character")]
    public string characterName;
    public Sprite characterImage;
    public eAlignment alignment;

    [Header("Scene")] 
    public Scene scene;
    public bool characterTalking;
    public int dialogIndex = 0;

    public enum Scene
    {
        INTRODUCTION,
        DECKBUILDING
    }
    

    [Serializable]
    public class DialogLine
    {
        [TextArea(3, 10)]
        public string dialogLine;
        public int sceneIndex;
        public List<ReplyOption> replies = new List<ReplyOption>();
        public bool utilizesScripts;
        
    }
    
    [Serializable]
    public enum eAlignment
    {
        LEFT,
        RIGHT
    }
    
    [Serializable]
    public class ReplyOption
    {
        [Header("Replies (2 Required)")]
        public int goToLineIndex;
        [TextArea(3, 10)]
        public string replyText;
    }

    public void OnBeforeSerialize()
    {
        dialogIndex = 0;
    }

    public void OnAfterDeserialize()
    {
        if (dialogLines == null)
        {
            dialogLines = new List<DialogLine>();
        }
        
        foreach (var line in dialogLines)
        {
            if (line != null && line.replies == null)
            {
                line.replies = new List<ReplyOption>();
            }
        }
    }
    
}
