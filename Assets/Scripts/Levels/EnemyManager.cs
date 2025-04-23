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
        EnemyData.Instance.GetAllEnemies();
    }

    public GameObject CreateEnemy(string enemyName, Vector3 position)
    {
        Enemy enemyData = EnemyData.Instance.GetEnemy(enemyName);
        if (enemyData == null)
        {
            return null;
        }

        GameObject enemyPrefab = Resources.Load<GameObject>("Enemy");
        if (enemyPrefab == null)
        {
            enemyPrefab = GameObject.FindGameObjectWithTag("EnemyPrefab");
            if (enemyPrefab == null)
            {
                return null;
            }
        }

        GameObject enemyInstance = Instantiate(enemyPrefab, position, Quaternion.identity);

        enemyInstance.GetComponent<SpriteRenderer>().sprite = 
            GameManager.Instance.enemySpriteManager.Get(enemyData.sprite);
        
        if (enemyName == "necromancer")
        {
            EnemyController oldController = enemyInstance.GetComponent<EnemyController>();
            if (oldController != null)
            {
                Destroy(oldController);
            }
            
            NecromancerController controller = enemyInstance.AddComponent<NecromancerController>();
            controller.hp = new Hittable(enemyData.hp, Hittable.Team.MONSTERS, enemyInstance);
            controller.speed = enemyData.speed;
            controller.damage = enemyData.damage;
        }
        else
        {
            EnemyController controller = enemyInstance.GetComponent<EnemyController>();
            controller.hp = new Hittable(enemyData.hp, Hittable.Team.MONSTERS, enemyInstance);
            controller.speed = enemyData.speed;
            controller.damage = enemyData.damage;
        }
        
        GameManager.Instance.AddEnemy(enemyInstance);
        
        return enemyInstance;
    }
} 
