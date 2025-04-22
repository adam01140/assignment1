using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class EnemyData : MonoBehaviour
{
    private static EnemyData instance;
    public static EnemyData Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("EnemyData");
                instance = obj.AddComponent<EnemyData>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    private Dictionary<string, Enemy> enemyDictionary = new Dictionary<string, Enemy>();
    private List<Enemy> enemies = new List<Enemy>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        LoadEnemies();
    }

    private void LoadEnemies()
    {
        Debug.Log("EnemyData.LoadEnemies() - Starting to load enemies from JSON");
        
        // Load the JSON file from Resources
        TextAsset enemiesJson = Resources.Load<TextAsset>("enemies");
        
        if (enemiesJson == null)
        {
            Debug.LogError("Failed to load enemies.json from Resources folder! Make sure the file exists at Assets/Resources/enemies.json");
            return;
        }
        
        Debug.Log($"Loaded enemies.json text: {enemiesJson.text.Substring(0, Mathf.Min(100, enemiesJson.text.Length))}...");
        
        try
        {
            // Deserialize the JSON into a list of Enemy objects
            enemies = JsonConvert.DeserializeObject<List<Enemy>>(enemiesJson.text);
            
            if (enemies == null)
            {
                Debug.LogError("Failed to deserialize enemies JSON! Result was null.");
                return;
            }
            
            if (enemies.Count == 0)
            {
                Debug.LogWarning("Deserialized enemies list is empty. Check JSON format.");
                return;
            }

            // Store enemies in a dictionary for quick lookup by name
            enemyDictionary.Clear();
            foreach (Enemy enemy in enemies)
            {
                enemyDictionary[enemy.name] = enemy;
                Debug.Log($"Loaded enemy: {enemy}");
            }
            
            Debug.Log($"Successfully loaded {enemies.Count} enemies!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception during enemy JSON deserialization: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // Get an enemy by name
    public Enemy GetEnemy(string name)
    {
        if (enemyDictionary.TryGetValue(name, out Enemy enemy))
        {
            return enemy;
        }
        
        Debug.LogWarning($"Enemy with name '{name}' not found!");
        return null;
    }

    // Get all enemies
    public List<Enemy> GetAllEnemies()
    {
        return new List<Enemy>(enemies);
    }
} 