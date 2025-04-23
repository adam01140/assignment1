using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager 
{
    public enum GameState
    {
        PREGAME,
        INWAVE,
        WAVEEND,
        COUNTDOWN,
        GAMEOVER
    }
    public GameState state;

    public int countdown;
    private static GameManager theInstance;
    
    // Add debug logging capabilities
    public static bool enableDebugLogging = true;
    
    public static GameManager Instance {  get
        {
            if (theInstance == null)
            {
                theInstance = new GameManager();
                
                if (enableDebugLogging)
                {
                    Debug.Log("GameManager singleton instance created");
                }
                
                // Subscribe to scene loading events
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.sceneUnloaded += OnSceneUnloaded;
            }
            return theInstance;
        }
    }
    
    // Track if we're in a scene transition
    public bool IsSceneTransitioning { get; private set; } = false;
    
    // Dictionary to map scene names (useful for scene loading)
    public static Dictionary<string, int> SceneNameToIndex = new Dictionary<string, int>()
    {
        { "Main", 0 },
        // Add other scenes as needed
    };

    // Static event handlers for scene management
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        
        // Reset necessary state when a new scene is loaded
        if (theInstance != null)
        {
            theInstance.IsSceneTransitioning = false;
            theInstance.ResetState();
            
            // Try to find player by tag after scene load
            if (theInstance.player == null)
            {
                GameObject playerObj = GameObject.FindWithTag("unit");
                if (playerObj != null)
                {
                    theInstance.player = playerObj;
                    Debug.Log("GameManager: Found player by tag after scene load");
                }
                else
                {
                    Debug.LogWarning("GameManager: Could not find player by tag after scene load");
                }
            }
        }
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"Scene unloaded: {scene.name}");
        
        // Clean up when a scene is unloaded
        if (theInstance != null)
        {
            theInstance.IsSceneTransitioning = true;
            theInstance.CleanupState();
        }
    }
    
    // Public method to trigger a scene transition
    public void LoadScene(int buildIndex)
    {
        if (IsSceneTransitioning)
        {
            Debug.LogWarning("Scene transition already in progress, ignoring additional request");
            return;
        }
        
        Debug.Log($"GameManager: Initiating scene transition to build index {buildIndex}");
        
        IsSceneTransitioning = true;
        CleanupState(); // Clean up before loading
        
        try
        {
            SceneManager.LoadScene(buildIndex);
        }
        catch (Exception e)
        {
            Debug.LogError($"GameManager: Failed to load scene at index {buildIndex}: {e.Message}");
            IsSceneTransitioning = false; // Reset flag if loading failed
        }
    }
    
    // Public method to trigger a scene transition by name
    public void LoadScene(string sceneName)
    {
        int buildIndex;
        if (SceneNameToIndex.TryGetValue(sceneName, out buildIndex))
        {
            LoadScene(buildIndex);
        }
        else
        {
            Debug.LogError($"GameManager: Scene name '{sceneName}' not found in SceneNameToIndex dictionary");
            
            // Try loading by name directly
            try
            {
                IsSceneTransitioning = true;
                CleanupState();
                SceneManager.LoadScene(sceneName);
            }
            catch (Exception e)
            {
                Debug.LogError($"GameManager: Failed to load scene '{sceneName}': {e.Message}");
                IsSceneTransitioning = false;
            }
        }
    }

    public void ResetState()
    {
        Debug.Log("GameManager: Resetting state");
        state = GameState.PREGAME;
        countdown = 0;
        
        // Do not clear player reference, let scene determine this
        // player = null;
        
        ClearEnemies();
        projectileManager = null;
        spellIconManager = null;
        enemySpriteManager = null;
        playerSpriteManager = null;
        relicIconManager = null;
        
        // Ensure enemies list is initialized
        if (enemies == null)
        {
            enemies = new List<GameObject>();
        }
    }
    
    private void ClearEnemies()
    {
        // Initialize list if it's null
        if (enemies == null)
        {
            enemies = new List<GameObject>();
            return;
        }
        
        // Remove references to potentially destroyed game objects
        List<GameObject> enemiesToRemove = new List<GameObject>();
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null)
            {
                enemiesToRemove.Add(enemy);
            }
        }
        
        foreach (GameObject enemy in enemiesToRemove)
        {
            enemies.Remove(enemy);
        }
        
        enemies.Clear();
    }

    public void CleanupState()
    {
        Debug.Log("GameManager: Cleaning up state");
        
        // We now keep the player reference during transitions
        // Store which object was the player but don't nullify it
        // This helps enemies that might try to access it during transitions
        if (player != null)
        {
            previousPlayer = player;
            if (enableDebugLogging)
            {
                Debug.Log("GameManager: Saved previous player reference");
            }
        }
        
        ClearEnemies();
    }

    // Keep track of the player GameObject
    public GameObject player;
    
    // Store previous player reference in case we need to restore it
    private GameObject previousPlayer;
    
    public ProjectileManager projectileManager;
    public SpellIconManager spellIconManager;
    public EnemySpriteManager enemySpriteManager;
    public PlayerSpriteManager playerSpriteManager;
    public RelicIconManager relicIconManager;

    private List<GameObject> enemies;
    public int enemy_count { get { return enemies?.Count ?? 0; } }

    // Try to get a valid player reference
    public GameObject TryGetPlayer()
    {
        // If player is valid, return it
        if (player != null)
        {
            return player;
        }
        
        // Try to restore from previous player
        if (previousPlayer != null)
        {
            player = previousPlayer;
            if (enableDebugLogging)
            {
                Debug.Log("GameManager: Restored player reference from previous player");
            }
            return player;
        }
        
        // Last resort, try to find by tag
        GameObject foundPlayer = GameObject.FindWithTag("unit");
        if (foundPlayer != null)
        {
            player = foundPlayer;
            if (enableDebugLogging)
            {
                Debug.Log("GameManager: Found player by tag");
            }
            return player;
        }
        
        return null;
    }

    public void AddEnemy(GameObject enemy)
    {
        // Initialize the list if it doesn't exist
        if (enemies == null)
        {
            enemies = new List<GameObject>();
        }
        
        if (enemy != null)
        {
            enemies.Add(enemy);
        }
    }

    public void RemoveEnemy(GameObject enemy)
    {
        // Check if enemies list exists
        if (enemies == null)
        {
            enemies = new List<GameObject>();
            return;
        }
        
        if (enemy != null)
        {
            enemies.Remove(enemy);
        }
    }

    public GameObject GetClosestEnemy(Vector3 point)
    {
        // Return null if enemies list doesn't exist or is empty
        if (enemies == null)
        {
            return null;
        }
        
        if (enemies.Count == 0) return null;
        if (enemies.Count == 1) return enemies[0];
        
        // Filter out null enemies
        List<GameObject> validEnemies = enemies.Where(e => e != null).ToList();
        if (validEnemies.Count == 0) return null;
        if (validEnemies.Count == 1) return validEnemies[0];
        
        return validEnemies.Aggregate((a,b) => 
            (a.transform.position - point).sqrMagnitude < (b.transform.position - point).sqrMagnitude ? a : b);
    }

    private GameManager()
    {
        // Always initialize the enemies list in the constructor
        enemies = new List<GameObject>();
        ResetState();
    }
}
