using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class ResourceChecker : MonoBehaviour
{
    void Start()
    {
        Debug.Log("==== RESOURCE CHECKER ====");
        
        // Check if Resources folder exists
        string resourcesPath = Path.Combine(Application.dataPath, "Resources");
        Debug.Log($"Checking if Resources directory exists at: {resourcesPath}");
        Debug.Log($"Resources directory exists: {Directory.Exists(resourcesPath)}");
        
        if (Directory.Exists(resourcesPath))
        {
            string[] files = Directory.GetFiles(resourcesPath);
            Debug.Log($"Files in Resources directory ({files.Length}):");
            foreach (string file in files)
            {
                Debug.Log($"- {Path.GetFileName(file)}");
            }
        }
        
        // Try to load resources through Unity's Resources system
        Debug.Log("\nTrying to load resources through Unity's Resources.Load:");
        TextAsset enemiesJson = Resources.Load<TextAsset>("enemies");
        Debug.Log($"enemies.json loaded: {enemiesJson != null}");
        
        TextAsset levelsJson = Resources.Load<TextAsset>("levels");
        Debug.Log($"levels.json loaded: {levelsJson != null}");
        
        // Check if Newtonsoft.Json is available
        Debug.Log("\nChecking for Newtonsoft.Json:");
        try
        {
            var jsonType = typeof(Newtonsoft.Json.JsonConvert);
            Debug.Log($"Newtonsoft.Json is available (Type: {jsonType.FullName})");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Newtonsoft.Json is NOT available: {ex.Message}");
        }
    }
} 