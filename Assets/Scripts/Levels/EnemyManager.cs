using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private static EnemyManager instance;
    public static EnemyManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("EnemyManager");
                instance = obj.AddComponent<EnemyManager>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        // Make sure EnemyData is initialized
        EnemyData.Instance.GetAllEnemies();
    }

    // Creates an enemy game object based on the enemy data
    public GameObject CreateEnemy(string enemyName, Vector3 position)
    {
        Enemy enemyData = EnemyData.Instance.GetEnemy(enemyName);
        if (enemyData == null)
        {
            Debug.LogError($"Failed to find enemy data for: {enemyName}");
            return null;
        }

        // Find the enemy prefab to instantiate
        GameObject enemyPrefab = Resources.Load<GameObject>("Enemy");
        if (enemyPrefab == null)
        {
            // If specific enemy prefab doesn't exist, try to use the default one
            enemyPrefab = GameObject.FindGameObjectWithTag("EnemyPrefab");
            if (enemyPrefab == null)
            {
                Debug.LogError("Could not find enemy prefab!");
                return null;
            }
        }

        // Create the enemy instance
        GameObject enemyInstance = Instantiate(enemyPrefab, position, Quaternion.identity);

        // Set the enemy's properties based on the data
        enemyInstance.GetComponent<SpriteRenderer>().sprite = 
            GameManager.Instance.enemySpriteManager.Get(enemyData.sprite);
        
        // Select the appropriate controller based on enemy type
        if (enemyName == "necromancer")
        {
            // Remove the default EnemyController if it exists
            EnemyController oldController = enemyInstance.GetComponent<EnemyController>();
            if (oldController != null)
            {
                Destroy(oldController);
            }
            
            // Add NecromancerController
            NecromancerController controller = enemyInstance.AddComponent<NecromancerController>();
            controller.hp = new Hittable(enemyData.hp, Hittable.Team.MONSTERS, enemyInstance);
            controller.speed = enemyData.speed;
            controller.damage = enemyData.damage;
            
            Debug.Log($"Created necromancer with HP: {enemyData.hp}, Speed: {enemyData.speed}, Damage: {enemyData.damage}");
        }
        else
        {
            // Use the regular EnemyController for other enemy types
            EnemyController controller = enemyInstance.GetComponent<EnemyController>();
            controller.hp = new Hittable(enemyData.hp, Hittable.Team.MONSTERS, enemyInstance);
            controller.speed = enemyData.speed;
            controller.damage = enemyData.damage;
        }
        
        // Add the enemy to the game manager
        GameManager.Instance.AddEnemy(enemyInstance);
        
        return enemyInstance;
    }
} 