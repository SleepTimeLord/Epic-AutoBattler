using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryScripts : MonoBehaviour
{
    public void exitGame()
    {
        Application.Quit();
        
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void EnterDialog()
    {
        SceneManager.LoadScene(1);
    }
    
    
}
