using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawningTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("===== WAVE SPAWNING TEST =====");
        
        // Find the EnemySpawner
        EnemySpawner spawner = Object.FindAnyObjectByType<EnemySpawner>();
        
        if (spawner == null)
        {
            Debug.LogError("❌ EnemySpawner not found in the scene!");
            return;
        }
        
        Debug.Log("✅ Found EnemySpawner component");
        
        // Check if we can access level data
        List<Level> levels = LevelData.Instance.GetAllLevels();
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("❌ No level data available!");
            return;
        }
        
        Debug.Log($"✅ Found {levels.Count} levels");
        
        // Test RPN expressions for a sample wave
        Dictionary<string, int> variables = new Dictionary<string, int>()
        {
            { "wave", 3 }, // Test with wave 3
            { "base", 20 } // Test with base 20
        };
        
        // Test the expression for zombie count in Easy level, wave 3
        Level easyLevel = LevelData.Instance.GetLevel("Easy");
        if (easyLevel != null && easyLevel.spawns.Count > 0)
        {
            Spawn zombieSpawn = easyLevel.spawns.Find(s => s.enemy == "zombie");
            if (zombieSpawn != null)
            {
                int zombieCount = RPNEvaluator.Evaluate(zombieSpawn.count, variables);
                Debug.Log($"In wave 3 of Easy level, expected to spawn {zombieCount} zombies");
                
                if (zombieCount == 8) // 5 + 3
                {
                    Debug.Log("✅ Zombie count calculation is correct");
                }
                else
                {
                    Debug.LogError($"❌ Zombie count calculation is incorrect (expected 8, got {zombieCount})");
                }
                
                // Test HP calculation
                variables["base"] = 20; // Base HP for zombie
                int zombieHp = RPNEvaluator.Evaluate(zombieSpawn.hp, variables);
                Debug.Log($"In wave 3 of Easy level, zombies should have {zombieHp} HP");
            }
        }
        
        // Test spawning a few enemies manually
        StartCoroutine(TestEnemySpawning(spawner));
        
        Debug.Log("===== WAVE SPAWNING TEST INITIATED =====");
    }
    
    private IEnumerator TestEnemySpawning(EnemySpawner spawner)
    {
        // Wait a bit for the scene to setup
        yield return new WaitForSeconds(1f);
        
        Debug.Log("Testing direct enemy spawning...");
        
        // Try to spawn one of each enemy type at a random spawn point
        string[] enemyTypes = { "zombie", "skeleton", "warlock" };
        
        foreach (string enemyType in enemyTypes)
        {
            Enemy enemyData = EnemyData.Instance.GetEnemy(enemyType);
            if (enemyData != null)
            {
                GameObject spawnedEnemy = spawner.SpawnSingleEnemy(
                    enemyType,
                    enemyData.sprite,
                    enemyData.hp,
                    enemyData.speed,
                    enemyData.damage,
                    "random"
                );
                
                if (spawnedEnemy != null)
                {
                    Debug.Log($"✅ Successfully spawned a {enemyType}");
                }
                else
                {
                    Debug.LogError($"❌ Failed to spawn a {enemyType}");
                }
                
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                Debug.LogError($"❌ Enemy data for {enemyType} not found");
            }
        }
        
        yield return new WaitForSeconds(1f);
        
        Debug.Log("===== WAVE SPAWNING TEST COMPLETE =====");
    }
} 