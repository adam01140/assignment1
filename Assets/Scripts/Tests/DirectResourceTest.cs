using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

public class DirectResourceTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("====== DIRECT RESOURCE TEST STARTING ======");
        Debug.Log("This message should appear in the console");
        
        // Check if Resources directory exists
        string resourcesPath = Path.Combine(Application.dataPath, "Resources");
        Debug.Log($"Resources path: {resourcesPath}");
        Debug.Log($"Resources exists: {Directory.Exists(resourcesPath)}");
        
        if (Directory.Exists(resourcesPath))
        {
            try
            {
                string[] files = Directory.GetFiles(resourcesPath);
                Debug.Log($"Found {files.Length} files in Resources directory");
                foreach (string file in files)
                {
                    Debug.Log($"File: {Path.GetFileName(file)}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error listing files: {ex.Message}");
            }
        }
        
        // Direct resource loading
        try
        {
            TextAsset enemiesFile = Resources.Load<TextAsset>("enemies");
            if (enemiesFile != null)
            {
                Debug.Log($"Successfully loaded enemies.json with content length: {enemiesFile.text.Length}");
                Debug.Log($"First 100 chars: {enemiesFile.text.Substring(0, Mathf.Min(100, enemiesFile.text.Length))}");
                
                try
                {
                    List<Enemy> enemies = JsonConvert.DeserializeObject<List<Enemy>>(enemiesFile.text);
                    if (enemies != null)
                    {
                        Debug.Log($"Successfully deserialized {enemies.Count} enemies");
                        foreach (var enemy in enemies)
                        {
                            Debug.Log($"Enemy: {enemy.name}, HP: {enemy.hp}, Speed: {enemy.speed}, Damage: {enemy.damage}");
                        }
                    }
                    else
                    {
                        Debug.LogError("Deserialized enemies list is null");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error deserializing enemies: {ex.Message}");
                }
            }
            else
            {
                Debug.LogError("Failed to load enemies.json");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception loading enemies: {ex.Message}");
        }
        
        Debug.Log("====== DIRECT RESOURCE TEST COMPLETE ======");
    }
} 