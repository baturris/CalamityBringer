using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Set this to the name or index of the scene you want to load
    public string SceneName;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!string.IsNullOrEmpty(SceneName))
            {
                SceneManager.LoadScene(SceneName);
            }
            else
            {
                Debug.LogWarning("Scene name not set in the inspector.");
            }
        }
    }
}
