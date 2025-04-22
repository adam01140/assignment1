using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("GameInitializer.Awake() - Starting initialization");
        
        // Initialize data singletons to load from JSON
        Debug.Log("Loading enemy data...");
        try {
            var enemies = EnemyData.Instance.GetAllEnemies();
            Debug.Log($"EnemyData initialized, got {enemies.Count} enemies");
        } catch (System.Exception ex) {
            Debug.LogError($"Failed to load enemy data: {ex.Message}\n{ex.StackTrace}");
        }
        
        Debug.Log("Loading level data...");
        try {
            var levels = LevelData.Instance.GetAllLevels();
            Debug.Log($"LevelData initialized, got {levels.Count} levels");
        } catch (System.Exception ex) {
            Debug.LogError($"Failed to load level data: {ex.Message}\n{ex.StackTrace}");
        }
        
        // Ensure EnemyManager is also initialized
        Debug.Log("Initializing EnemyManager...");
        try {
            EnemyManager.Instance.Start();
            Debug.Log("EnemyManager initialized");
        } catch (System.Exception ex) {
            Debug.LogError($"Failed to initialize EnemyManager: {ex.Message}\n{ex.StackTrace}");
        }
        
        Debug.Log("GameInitializer.Awake() - Finished initialization");
    }
} 