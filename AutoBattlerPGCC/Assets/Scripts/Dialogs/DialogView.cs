using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogView", menuName = "Dialog Objects/DialogView")]

public class DialogView : ScriptableObject, ISerializationCallbackReceiver
{
    
    [SerializeField]
    public List<SceneDialogs>  scenes = new List<SceneDialogs>();
    
    [Serializable]
    public class SceneDialogs : ISerializationCallbackReceiver
    {
        public Dictionary<int, DialogCharacter> sceneCharacterIndex = new Dictionary<int, DialogCharacter>();
        public List<DialogCharacter> characters = new List<DialogCharacter>();
        
        [SerializeField]
        private List<int> keys = new List<int>();
        
        
        [SerializeField]
        private List<DialogCharacter> values = new List<DialogCharacter>();
        
        public int sceneIndex;
        public void OnBeforeSerialize()
        {
            if (sceneCharacterIndex.Count == 0 && characters != null)
            {
                sceneCharacterIndex.Clear();
                for (int i = 0; i < characters.Count; i++)
                {
                    var c = characters[i];
                    if (c != null)
                        sceneCharacterIndex.TryAdd(i, c);
                }
            }
            
            keys.Clear();
            values.Clear();
            foreach (var kvp in sceneCharacterIndex)
            {
                
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            
            sceneCharacterIndex ??= new Dictionary<int, DialogCharacter>();
            sceneCharacterIndex.Clear();

            // Rebuild the dictionary; use the min count to be safe
            int count = Mathf.Min(keys?.Count ?? 0, values?.Count ?? 0);
            for (int i = 0; i < count; i++)
            {
                int key = keys[i];
                var value = values[i];

                // Guard against duplicate keys
                if (!sceneCharacterIndex.ContainsKey(key))
                {
                    sceneCharacterIndex.Add(key, value);
                }
                else
                {
                    Debug.LogWarning($"Duplicate key {key} encountered in SceneDialogs (sceneIndex={sceneIndex}). Skipping.");
                }
            }
        }
    }


    public void OnBeforeSerialize()
    {
        
    }

    public void OnAfterDeserialize()
    {
        scenes ??= new List<SceneDialogs>();
        
    }
}
