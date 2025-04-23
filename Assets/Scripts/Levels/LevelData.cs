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
        TextAsset levelsJson = Resources.Load<TextAsset>("levels");
        
        if (levelsJson == null)
        {
            return;
        }
        
        try
        {
            levels = JsonConvert.DeserializeObject<List<Level>>(levelsJson.text);
            
            if (levels == null)
            {
                return;
            }
            
            if (levels.Count == 0)
            {
                return;
            }

            levelDictionary.Clear();
            foreach (Level level in levels)
            {
                levelDictionary[level.name] = level;
            }
        }
        catch (System.Exception ex)
        {
        }
    }

    public Level GetLevel(string name)
    {
        if (levelDictionary.TryGetValue(name, out Level level))
        {
            return level;
        }
        
        return null;
    }

    public List<Level> GetAllLevels()
    {
        return new List<Level>(levels);
    }
} 
