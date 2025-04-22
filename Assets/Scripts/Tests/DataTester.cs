using UnityEngine;

public class DataTester : MonoBehaviour
{
    public bool testEnemyData = true;
    public bool testLevelData = true;
    public bool checkResources = true;

    void Start()
    {
        Debug.Log("==== STARTING DATA TESTS ====");

        // First check resources
        if (checkResources)
        {
            gameObject.AddComponent<ResourceChecker>();
            Debug.Log("Added ResourceChecker component");
        }

        // Make sure the GameInitializer is called to load data
        if (Object.FindAnyObjectByType<GameInitializer>() == null)
        {
            GameObject initObj = new GameObject("GameInitializer");
            initObj.AddComponent<GameInitializer>();
            Debug.Log("Created GameInitializer to load data");
        }

        // Add test components based on settings
        if (testEnemyData)
        {
            gameObject.AddComponent<EnemyDataTest>();
            Debug.Log("Added EnemyDataTest component");
        }

        if (testLevelData)
        {
            gameObject.AddComponent<LevelDataTest>();
            Debug.Log("Added LevelDataTest component");
        }
    }
} 