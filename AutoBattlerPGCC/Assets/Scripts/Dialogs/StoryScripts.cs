using UnityEngine;

public class StoryScripts : MonoBehaviour
{
    public void exitGame()
    {
        Application.Quit();
        
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    
}
