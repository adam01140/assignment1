using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public Transform target;
    public int speed;
    public int damage = 5; // Default damage value
    public Hittable hp;
    public HealthBar healthui;
    public bool dead;

    public float last_attack;
    
    // Add stats tracking
    public static System.Action OnEnemyDefeated;
    
    // Static flags for error logging
    private static bool hasLoggedGameManagerError = false;
    private static bool hasLoggedPlayerError = false;
    
    // Flag to track if scene is changing
    private bool isSceneChanging = false;
    
    // Timer for retry attempts to find player
    private float retryPlayerFindTimer = 0f;
    private const float RETRY_PLAYER_FIND_INTERVAL = 1f; // Try every second

    void OnEnable()
    {
        // Subscribe to scene loading events
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        
        // Register with GameManager if available
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddEnemy(gameObject);
        }
    }
    
    void OnDisable()
    {
        // Unsubscribe from scene events
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        
        // Unregister from GameManager if available
        if (GameManager.Instance != null && !isSceneChanging)
        {
            GameManager.Instance.RemoveEnemy(gameObject);
        }
    }
    
    // Scene event handlers
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isSceneChanging = false;
        
        // Try to find player again after scene load
        StartCoroutine(FindPlayerAfterDelay(0.5f));
    }
    
    private void OnSceneUnloaded(Scene scene)
    {
        isSceneChanging = true;
    }
    
    // Coroutine to find player after a delay
    private IEnumerator FindPlayerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Use the new TryGetPlayer method
        if (target == null && GameManager.Instance != null)
        {
            GameObject playerObj = GameManager.Instance.TryGetPlayer();
            if (playerObj != null)
            {
                target = playerObj.transform;
                Debug.Log("Enemy found player after scene load");
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        TryInitializeTarget();
        
        if (hp == null)
        {
            Debug.LogError("Enemy hp is null in Start() - enemy was not properly initialized");
            return;
        }
        
        hp.OnDeath += Die;
        
        if (healthui == null)
        {
            Debug.LogWarning("Enemy healthui is null");
        }
        else
        {
            healthui.SetHealth(hp);
        }
    }
    
    // Try to initialize the target (player)
    private bool TryInitializeTarget()
    {
        // Check if we already have a valid target
        if (target != null)
        {
            return true;
        }
        
        if (GameManager.Instance == null)
        {
            if (!hasLoggedGameManagerError)
            {
                Debug.LogError("GameManager.Instance is null in EnemyController.TryInitializeTarget");
                hasLoggedGameManagerError = true;
            }
            return false;
        }
        
        // Use the new TryGetPlayer method
        GameObject playerObj = GameManager.Instance.TryGetPlayer();
        if (playerObj == null)
        {
            if (!hasLoggedPlayerError)
            {
                Debug.LogError("Could not find player through GameManager.TryGetPlayer()");
                hasLoggedPlayerError = true;
            }
            return false;
        }
        
        // Set the target to the player
        target = playerObj.transform;
        Debug.Log("Enemy successfully initialized target to player");
        return true;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // Don't process during scene transitions or game over
        if (isSceneChanging || 
            GameManager.Instance == null || 
            GameManager.Instance.state == GameManager.GameState.GAMEOVER)
        {
            return;
        }
        
        // Try to find player if target is null
        if (target == null)
        {
            retryPlayerFindTimer += Time.deltaTime;
            if (retryPlayerFindTimer >= RETRY_PLAYER_FIND_INTERVAL)
            {
                retryPlayerFindTimer = 0f;
                if (!TryInitializeTarget())
                {
                    return; // Could not find target, skip this frame
                }
            }
            else
            {
                return; // Wait until retry timer is up
            }
        }
        
        try
        {
            // Double-check target validity
            if (target == null)
            {
                return;
            }
            
            // Calculate distance to target
            Vector3 direction = target.position - transform.position;
            float distanceToTarget = direction.magnitude;
            
            // Attack if within range
            if (distanceToTarget < 2f)
            {
                DoAttack();
            }
            else
            {
                // Move towards target
                Unit unit = GetComponent<Unit>();
                if (unit != null)
                {
                    unit.movement = direction.normalized * speed;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Exception in EnemyController.Update: {e.Message}\n{e.StackTrace}");
        }
    }
    
    void DoAttack()
    {
        // Skip attack if scene is changing or game is over
        if (isSceneChanging || 
            GameManager.Instance == null || 
            GameManager.Instance.state == GameManager.GameState.GAMEOVER)
        {
            return;
        }
        
        try
        {
            // Check if enough time has passed for next attack
            if (last_attack + 2 >= Time.time)
            {
                return; // Not ready to attack yet
            }
            
            last_attack = Time.time;
            
            // Verify GameManager
            if (GameManager.Instance == null)
            {
                if (!hasLoggedGameManagerError)
                {
                    Debug.LogError("GameManager.Instance is null in DoAttack");
                    hasLoggedGameManagerError = true;
                }
                return;
            }
            
            // First try TryGetPlayer method which has recovery mechanisms
            GameObject playerObj = null;
            
            try
            {
                playerObj = GameManager.Instance.TryGetPlayer();
                
                // Log more detailed diagnostics if player is null from TryGetPlayer
                if (playerObj == null)
                {
                    Debug.LogWarning($"DoAttack: GameManager.TryGetPlayer returned null. " +
                                   $"GameManager.player is {(GameManager.Instance.player == null ? "null" : "not null")}, " +
                                   $"Scene: {SceneManager.GetActiveScene().name}, " +
                                   $"State: {GameManager.Instance.state}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception in DoAttack when calling TryGetPlayer: {e.Message}");
                return;
            }
            
            // If TryGetPlayer fails, do a direct search as last resort
            if (playerObj == null)
            {
                playerObj = GameObject.FindWithTag("unit");
                
                // If we found the player, update the GameManager reference
                if (playerObj != null)
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.player = playerObj;
                        Debug.Log("EnemyController: Found and restored player reference directly");
                    }
                    else
                    {
                        Debug.LogError("GameManager.Instance became null after TryGetPlayer call");
                        return;
                    }
                }
                else
                {
                    if (!hasLoggedPlayerError)
                    {
                        Debug.LogError("Could not find player even with direct search in DoAttack");
                        hasLoggedPlayerError = true;
                        return;
                    }
                    else
                    {
                        return; // Already logged, skip this frame
                    }
                }
            }
            
            // Extra safety check
            if (playerObj == null)
            {
                Debug.LogError("Player object is still null after recovery attempts");
                return;
            }
            
            // Update target to make sure it's the latest player
            target = playerObj.transform;
            
            // Ensure the gameObject is not null
            if (target == null || target.gameObject == null)
            {
                target = null; // Reset the target so it will be re-acquired
                Debug.LogWarning("Player transform or gameObject became null after setting target");
                return;
            }
            
            // Get PlayerController reference and verify it's valid
            PlayerController playerController = target.gameObject.GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogWarning("PlayerController component not found on target");
                return;
            }
            
            // Check HP component - if null, try to initialize it
            if (playerController.hp == null)
            {
                // Check if the player has already called StartLevel
                playerController.StartLevel();
                
                // Check again if HP is still null
                if (playerController.hp == null)
                {
                    Debug.LogWarning("PlayerController.hp is null, cannot deal damage. Attempted to initialize it without success.");
                    return;
                }
            }
            
            // Deal damage
            playerController.hp.Damage(new Damage(damage, Damage.Type.PHYSICAL));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Exception in DoAttack: {e.Message}\n{e.StackTrace}");
        }
    }

    protected virtual void Die()
    {
        if (!dead)
        {
            dead = true;
            
            // Call the global event for enemy defeat
            OnEnemyDefeated?.Invoke();
            
            // Remove from GameManager's enemy list
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RemoveEnemy(gameObject);
            }
            
            Destroy(gameObject);
        }
    }
}
