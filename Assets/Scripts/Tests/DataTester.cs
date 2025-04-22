using UnityEngine;

public class DataTester : MonoBehaviour
{
    public bool testEnemyData = true;
    public bool testLevelData = true;
    public bool checkResources = true;
    public bool runLevelLoadingTest = true;
    public bool testLevelSelection = true;
    public bool testRPNEvaluator = true;
    public bool testWaveSpawning = true;

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
        
        if (runLevelLoadingTest)
        {
            gameObject.AddComponent<LevelLoadingTest>();
            Debug.Log("Added LevelLoadingTest component");
        }
        
        if (testLevelSelection)
        {
            gameObject.AddComponent<LevelSelectionTest>();
            Debug.Log("Added LevelSelectionTest component");
        }
        
        if (testRPNEvaluator)
        {
            gameObject.AddComponent<RPNEvaluatorTest>();
            Debug.Log("Added RPNEvaluatorTest component");
        }
        
        if (testWaveSpawning)
        {
            gameObject.AddComponent<WaveSpawningTest>();
            Debug.Log("Added WaveSpawningTest component");
        }
    }
} 