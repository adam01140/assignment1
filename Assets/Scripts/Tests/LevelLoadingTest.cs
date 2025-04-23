using UnityEngine;
using System.Collections.Generic;
using System.Text;

/**
 * LevelLoadingTest - Tests the level loading system to ensure it functions correctly
 * 
 * Purpose:
 * - Validates that the game can properly load level data from JSON files
 * - Checks that all expected levels (Easy, Medium, Endless) are loaded
 * - Verifies the structure and content of each level
 * 
 * Key Features:
 * - Runs a series of automated tests on the level loading system
 * - Validates level counts, wave counts, and spawn configurations
 * - Checks for the presence of all expected enemy types in spawn configurations
 * - Reports detailed results to the console for debugging
 */
public class LevelLoadingTest : MonoBehaviour
{
    /**
     * Start - Called when the component starts
     * Performs a series of automated tests on the level loading system
     */
    void Start()
    {
        Debug.Log("=============== LEVEL LOADING TEST ===============");

        // Make sure LevelData is initialized
        List<Level> levels = LevelData.Instance.GetAllLevels();
        
        if (levels != null && levels.Count > 0)
        {
            Debug.Log($"Successfully loaded {levels.Count} levels from levels.json");
            
            /**
             * Test 1: Check that all expected levels were loaded
             * Validates that Easy, Medium, and Endless levels exist
             */
            Debug.Log("Test 1: Verifying all levels were loaded...");
            bool foundEasy = false;
            bool foundMedium = false;
            bool foundEndless = false;
            
            foreach (Level level in levels)
            {
                Debug.Log($"Level '{level.name}' has {level.spawns.Count} spawn configurations");
                
                if (level.name == "Easy") foundEasy = true;
                if (level.name == "Medium") foundMedium = true;
                if (level.name == "Endless") foundEndless = true;
            }
            
            Debug.Log($"Found Easy level: {foundEasy}");
            Debug.Log($"Found Medium level: {foundMedium}");
            Debug.Log($"Found Endless level: {foundEndless}");
            
            if (foundEasy && foundMedium && foundEndless)
            {
                Debug.Log("✅ Test 1 PASSED: All expected levels were loaded");
            }
            else
            {
                Debug.LogError("❌ Test 1 FAILED: Not all expected levels were found");
            }
            
            /**
             * Test 2: Check level details for accuracy
             * Validates wave count and spawn count for the Easy level
             */
            Debug.Log("\nTest 2: Verifying level details...");
            
            Level easy = LevelData.Instance.GetLevel("Easy");
            if (easy != null)
            {
                bool easyValid = easy.waves == 10 && easy.spawns.Count == 4;
                Debug.Log($"Easy level - waves: {easy.waves}, spawn count: {easy.spawns.Count}");
                Debug.Log($"Easy level validity: {easyValid}");
                
                if (easyValid)
                {
                    Debug.Log("✅ Easy level has correct structure");
                }
                else
                {
                    Debug.LogError("❌ Easy level has incorrect structure");
                }
            }
            
            /**
             * Test 3: Check spawn details for accuracy
             * Validates that all expected enemy types exist in the Easy level
             */
            Debug.Log("\nTest 3: Verifying spawn details...");
            
            if (easy != null && easy.spawns.Count >= 3)
            {
                StringBuilder sb = new StringBuilder();
                
                // Check each spawn entry in the Easy level
                bool zombieFound = false;
                bool skeletonFound = false;
                bool warlockFound = false;
                bool necromancerFound = false;
                
                foreach (Spawn spawn in easy.spawns)
                {
                    sb.AppendLine($"Spawn: {spawn.enemy}");
                    sb.AppendLine($"- count: {spawn.count}");
                    sb.AppendLine($"- hp: {spawn.hp ?? "base"}");
                    sb.AppendLine($"- delay: {spawn.delay}");
                    sb.AppendLine($"- location: {spawn.location}");
                    sb.AppendLine($"- sequence: {(spawn.sequence != null ? string.Join(",", spawn.sequence) : "null")}");
                    
                    if (spawn.enemy == "zombie") zombieFound = true;
                    if (spawn.enemy == "skeleton") skeletonFound = true;
                    if (spawn.enemy == "warlock") warlockFound = true;
                    if (spawn.enemy == "necromancer") necromancerFound = true;
                }
                
                Debug.Log(sb.ToString());
                
                bool allEnemiesFound = zombieFound && skeletonFound && warlockFound && necromancerFound;
                Debug.Log($"All expected spawn enemies found: {allEnemiesFound}");
                
                if (allEnemiesFound)
                {
                    Debug.Log("✅ Test 3 PASSED: All expected spawn entries found");
                }
                else
                {
                    Debug.LogError("❌ Test 3 FAILED: Not all expected spawn entries found");
                }
            }
            
            Debug.Log("\n🏆 Level loading tests complete!");
        }
        else
        {
            Debug.LogError("Failed to load any levels from levels.json!");
        }
        
        Debug.Log("=============== LEVEL LOADING TEST COMPLETE ===============");
    }
} 