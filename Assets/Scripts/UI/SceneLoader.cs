using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A simple, self-contained scene loader that doesn't depend on other systems.
/// Attach this to any button and call LoadMainScene() directly from the button click event.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    // Public method that can be called from button events in the inspector
    public void LoadMainScene()
    {
        Debug.Log("SceneLoader.LoadMainScene called");
        
        // Force garbage collection to clean up resources
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
        
        try
        {
            SceneManager.LoadScene(0);
            Debug.Log("SceneLoader: Successfully called LoadScene(0)");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SceneLoader: Error loading scene by index: {e.Message}");
            
            try
            {
                SceneManager.LoadScene("Main");
                Debug.Log("SceneLoader: Successfully called LoadScene(\"Main\")");
            }
            catch (System.Exception e2)
            {
                Debug.LogError($"SceneLoader: Error loading scene by name: {e2.Message}");
            }
        }
    }
    
    // Reload the current scene
    public void ReloadCurrentScene()
    {
        Debug.Log("SceneLoader.ReloadCurrentScene called");
        
        try
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
            Debug.Log($"SceneLoader: Successfully reloaded scene: {currentScene.name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SceneLoader: Error reloading current scene: {e.Message}");
        }
    }
    
    // Load a scene by name
    public void LoadSceneByName(string sceneName)
    {
        Debug.Log($"SceneLoader.LoadSceneByName called with name: {sceneName}");
        
        try
        {
            SceneManager.LoadScene(sceneName);
            Debug.Log($"SceneLoader: Successfully loaded scene: {sceneName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SceneLoader: Error loading scene '{sceneName}': {e.Message}");
        }
    }
    
    // Load a scene by build index
    public void LoadSceneByIndex(int buildIndex)
    {
        Debug.Log($"SceneLoader.LoadSceneByIndex called with index: {buildIndex}");
        
        try
        {
            SceneManager.LoadScene(buildIndex);
            Debug.Log($"SceneLoader: Successfully loaded scene at index: {buildIndex}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SceneLoader: Error loading scene at index {buildIndex}: {e.Message}");
        }
    }
} 