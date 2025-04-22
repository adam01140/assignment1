using UnityEngine;
using System.Collections.Generic;

public class LevelDataTest : MonoBehaviour
{
    void Start()
    {
        // Log all loaded levels
        Debug.Log("--- TESTING LEVEL DATA LOADING ---");
        List<Level> levels = LevelData.Instance.GetAllLevels();
        
        if (levels != null && levels.Count > 0)
        {
            Debug.Log($"Successfully loaded {levels.Count} levels:");
            foreach (Level level in levels)
            {
                Debug.Log($"Level: {level.name} (Waves: {level.waves}, Spawns: {level.spawns.Count})");
                
                // Log details of each spawn in the level
                foreach (Spawn spawn in level.spawns)
                {
                    Debug.Log($"  Spawn: {spawn.enemy} (Count: {spawn.count}, HP: {spawn.hp ?? "base"}, " +
                              $"Speed: {spawn.speed ?? "base"}, Damage: {spawn.damage ?? "base"}, " +
                              $"Delay: {spawn.delay}, Location: {spawn.location}, " +
                              $"Sequence: {(spawn.sequence != null ? string.Join(",", spawn.sequence) : "default")})");
                }
            }
        }
        else
        {
            Debug.LogError("Failed to load levels or no levels were found!");
        }
        
        // Try to access specific levels by name
        Debug.Log("\n--- TESTING LEVEL LOOKUP ---");
        Level easy = LevelData.Instance.GetLevel("Easy");
        if (easy != null)
        {
            Debug.Log($"Found Easy level: Waves={easy.waves}, Spawns={easy.spawns.Count}");
        }
        
        Level medium = LevelData.Instance.GetLevel("Medium");
        if (medium != null)
        {
            Debug.Log($"Found Medium level: Waves={medium.waves}, Spawns={medium.spawns.Count}");
        }
        
        Level endless = LevelData.Instance.GetLevel("Endless");
        if (endless != null)
        {
            Debug.Log($"Found Endless level: Waves={endless.waves}, Spawns={endless.spawns.Count}");
        }
        
        // Try an invalid name to test error handling
        Level invalid = LevelData.Instance.GetLevel("invalid_level");
        if (invalid == null)
        {
            Debug.Log("Correctly returned null for invalid level name");
        }
    }
} 