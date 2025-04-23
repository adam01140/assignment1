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
        TextAsset enemiesJson = Resources.Load<TextAsset>("enemies");
        
        if (enemiesJson == null)
        {
            return;
        }
        
        try
        {
            enemies = JsonConvert.DeserializeObject<List<Enemy>>(enemiesJson.text);
            
            if (enemies == null)
            {
                return;
            }
            
            if (enemies.Count == 0)
            {
                return;
            }

            enemyDictionary.Clear();
            foreach (Enemy enemy in enemies)
            {
                enemyDictionary[enemy.name] = enemy;
            }
        }
        catch (System.Exception ex)
        {
        }
    }

    public Enemy GetEnemy(string name)
    {
        if (enemyDictionary.TryGetValue(name, out Enemy enemy))
        {
            return enemy;
        }
        
        return null;
    }

    public List<Enemy> GetAllEnemies()
    {
        return new List<Enemy>(enemies);
    }
} 
