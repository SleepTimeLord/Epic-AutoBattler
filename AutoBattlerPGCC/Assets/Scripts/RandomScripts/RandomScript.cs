using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RandomScripts : MonoBehaviour
{
    public Transform obj;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        obj.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        StartCoroutine(scaleUp());
    }
    void Awake()
    {
        if (obj == null) obj = transform; // default to this GameObject
    }

    // Update is called once per frame
    void Update()
    {
        if (obj.Equals(null)) obj = transform; // default to this GameObject

    }
    public void EnterIntroduction()
    {
        SceneManager.LoadScene(1);
    }

    public IEnumerator scaleUp()
    {
        while (true)
        {
            for (int i = 100; i <= 150; i++)
            {
                float s = i * 0.01f;
                obj.localScale = new Vector3(s, s, s);
            
                yield return new WaitForSeconds(0.01f);
            }
            for (int i = 150; i >= 100; i--)
            {
                float s = i * 0.01f;
                obj.localScale = new Vector3(s, s, s);
            
                yield return new WaitForSeconds(0.01f);
            }
        }
    }
}
