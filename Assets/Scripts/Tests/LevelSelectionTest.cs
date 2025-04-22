using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelSelectionTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("===== LEVEL SELECTION UI TEST =====");
        
        // Find the EnemySpawner
        EnemySpawner spawner = Object.FindAnyObjectByType<EnemySpawner>();
        
        if (spawner == null)
        {
            Debug.LogError("❌ EnemySpawner not found in the scene!");
            return;
        }
        
        Debug.Log("✅ Found EnemySpawner component");
        
        // Check if level_selector is assigned
        if (spawner.level_selector == null)
        {
            Debug.LogError("❌ level_selector is not assigned in the EnemySpawner!");
            return;
        }
        
        Debug.Log("✅ level_selector is properly assigned");
        
        // Check if button prefab is assigned
        if (spawner.button == null)
        {
            Debug.LogError("❌ button prefab is not assigned in the EnemySpawner!");
            return;
        }
        
        Debug.Log("✅ button prefab is properly assigned");
        
        // Count the number of level selection buttons
        int buttonCount = 0;
        MenuSelectorController[] selectors = spawner.level_selector.GetComponentsInChildren<MenuSelectorController>();
        
        if (selectors != null)
        {
            buttonCount = selectors.Length;
            
            Debug.Log($"Found {buttonCount} level selection buttons");
            
            // Verify buttons have proper level names
            foreach (MenuSelectorController selector in selectors)
            {
                Debug.Log($"Button with level name: '{selector.level}'");
            }
            
            // Check if we have the expected number of levels
            List<Level> levels = LevelData.Instance.GetAllLevels();
            if (levels != null && buttonCount == levels.Count)
            {
                Debug.Log($"✅ Number of buttons ({buttonCount}) matches number of levels ({levels.Count})");
            }
            else
            {
                Debug.LogWarning($"⚠️ Number of buttons ({buttonCount}) does not match number of levels ({(levels != null ? levels.Count : 0)})");
            }
        }
        else
        {
            Debug.LogError("❌ No MenuSelectorController components found in level_selector's children!");
        }
        
        Debug.Log("===== LEVEL SELECTION UI TEST COMPLETE =====");
    }
    
    // Method to simulate clicking a level button
    public void SimulateButtonClick(string levelName)
    {
        Debug.Log($"Simulating click on '{levelName}' button");
        
        MenuSelectorController[] selectors = Object.FindObjectsByType<MenuSelectorController>(FindObjectsSortMode.None);
        
        foreach (MenuSelectorController selector in selectors)
        {
            if (selector.level == levelName)
            {
                Debug.Log($"Found button for level '{levelName}', clicking it");
                selector.StartLevel();
                return;
            }
        }
        
        Debug.LogError($"❌ Button for level '{levelName}' not found!");
    }
} 