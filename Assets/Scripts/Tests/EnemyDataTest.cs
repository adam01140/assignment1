using UnityEngine;
using System.Collections.Generic;

public class EnemyDataTest : MonoBehaviour
{
    void Start()
    {
        // Log all loaded enemies
        Debug.Log("--- TESTING ENEMY DATA LOADING ---");
        List<Enemy> enemies = EnemyData.Instance.GetAllEnemies();
        
        if (enemies != null && enemies.Count > 0)
        {
            Debug.Log($"Successfully loaded {enemies.Count} enemies:");
            foreach (Enemy enemy in enemies)
            {
                Debug.Log($"Enemy: {enemy.name} (Sprite: {enemy.sprite}, HP: {enemy.hp}, Speed: {enemy.speed}, Damage: {enemy.damage})");
            }
        }
        else
        {
            Debug.LogError("Failed to load enemies or no enemies were found!");
        }
        
        // Try to access specific enemies by name
        Debug.Log("\n--- TESTING ENEMY LOOKUP ---");
        Enemy zombie = EnemyData.Instance.GetEnemy("zombie");
        if (zombie != null)
        {
            Debug.Log($"Found zombie: HP={zombie.hp}, Speed={zombie.speed}, Damage={zombie.damage}");
        }
        
        Enemy skeleton = EnemyData.Instance.GetEnemy("skeleton");
        if (skeleton != null)
        {
            Debug.Log($"Found skeleton: HP={skeleton.hp}, Speed={skeleton.speed}, Damage={skeleton.damage}");
        }
        
        Enemy warlock = EnemyData.Instance.GetEnemy("warlock");
        if (warlock != null)
        {
            Debug.Log($"Found warlock: HP={warlock.hp}, Speed={warlock.speed}, Damage={warlock.damage}");
        }
        
        // Try an invalid name to test error handling
        Enemy invalid = EnemyData.Instance.GetEnemy("invalid_enemy");
        if (invalid == null)
        {
            Debug.Log("Correctly returned null for invalid enemy name");
        }
    }
} 