using UnityEngine;

/// <summary>
/// This component ensures that GameManager is initialized at the start of the scene
/// and that player references are correctly established.
/// Add this to an object in your scene that loads early.
/// </summary>
public class GameManagerInitializer : MonoBehaviour
{
    // Reference to the player GameObject (set in inspector if possible)
    [SerializeField] private GameObject playerReference;
    
    // Whether to log debug information
    [SerializeField] private bool enableDebugLogs = true;
    
    private void Awake()
    {
        // Initialize GameManager
        var manager = GameManager.Instance;
        if (enableDebugLogs)
        {
            Debug.Log("GameManagerInitializer: Ensured GameManager instance exists");
        }
        
        // Set up debug logging flag
        GameManager.enableDebugLogging = enableDebugLogs;
        
        // Find player if not set
        FindAndSetPlayer();
    }
    
    private void Start()
    {
        // Double-check player reference in Start
        if (GameManager.Instance.player == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("GameManagerInitializer: Player reference was null in Start, trying to find it again");
            }
            FindAndSetPlayer();
        }
    }
    
    private void FindAndSetPlayer()
    {
        // If manually set in inspector, use that reference
        if (playerReference != null)
        {
            GameManager.Instance.player = playerReference;
            if (enableDebugLogs)
            {
                Debug.Log("GameManagerInitializer: Set player reference from inspector-assigned reference");
            }
            return;
        }
        
        // Otherwise try to find by tag
        GameObject player = GameObject.FindWithTag("unit");
        if (player != null)
        {
            GameManager.Instance.player = player;
            // Store for future use
            playerReference = player;
            
            if (enableDebugLogs)
            {
                Debug.Log("GameManagerInitializer: Found and set player reference by tag");
            }
        }
        else if (enableDebugLogs)
        {
            Debug.LogError("GameManagerInitializer: Could not find player GameObject! Ensure it has the 'unit' tag");
        }
    }
    
    private void OnEnable()
    {
        // If GameManager exists and player is null, try to set it
        if (GameManager.Instance != null && GameManager.Instance.player == null)
        {
            FindAndSetPlayer();
        }
    }
    
    // Regular check to ensure player reference remains valid
    private void Update()
    {
        if (GameManager.Instance.player == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("GameManagerInitializer: Player reference was lost, attempting to restore");
            }
            FindAndSetPlayer();
        }
    }
}