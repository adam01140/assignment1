using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class LevelData : MonoBehaviour
{
    private static LevelData instance;
    public static LevelData Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("LevelData");
                instance = obj.AddComponent<LevelData>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    private Dictionary<string, Level> levelDictionary = new Dictionary<string, Level>();
    private List<Level> levels = new List<Level>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        LoadLevels();
    }

    private void LoadLevels()
    {
        Debug.Log("LevelData.LoadLevels() - Starting to load levels from JSON");
        
        // Load the JSON file from Resources
        TextAsset levelsJson = Resources.Load<TextAsset>("levels");
        
        if (levelsJson == null)
        {
            Debug.LogError("Failed to load levels.json from Resources folder! Make sure the file exists at Assets/Resources/levels.json");
            return;
        }
        
        Debug.Log($"Loaded levels.json text: {levelsJson.text.Substring(0, Mathf.Min(100, levelsJson.text.Length))}...");
        
        try
        {
            // Deserialize the JSON into a list of Level objects
            levels = JsonConvert.DeserializeObject<List<Level>>(levelsJson.text);
            
            if (levels == null)
            {
                Debug.LogError("Failed to deserialize levels JSON! Result was null.");
                return;
            }
            
            if (levels.Count == 0)
            {
                Debug.LogWarning("Deserialized levels list is empty. Check JSON format.");
                return;
            }

            // Store levels in a dictionary for quick lookup by name
            levelDictionary.Clear();
            foreach (Level level in levels)
            {
                levelDictionary[level.name] = level;
                Debug.Log($"Loaded level: {level}");
            }
            
            Debug.Log($"Successfully loaded {levels.Count} levels!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception during level JSON deserialization: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // Get a level by name
    public Level GetLevel(string name)
    {
        if (levelDictionary.TryGetValue(name, out Level level))
        {
            return level;
        }
        
        Debug.LogWarning($"Level with name '{name}' not found!");
        return null;
    }

    // Get all levels
    public List<Level> GetAllLevels()
    {
        return new List<Level>(levels);
    }
} 