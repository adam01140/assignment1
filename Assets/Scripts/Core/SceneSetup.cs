using UnityEngine;

/// <summary>
/// Add this component to a GameObject in your scene to ensure proper initialization
/// This is especially important for GameManager and player references
/// </summary>
public class SceneSetup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerReference;
    
    [Header("Settings")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool dontDestroyOnLoad = false;
    
    private void Awake()
    {
        // Make sure GameManager.enableDebugLogging is set correctly
        GameManager.enableDebugLogging = enableDebugLogging;
        
        // Force creation of GameManager singleton
        GameManager manager = GameManager.Instance;
        
        if (enableDebugLogging)
        {
            Debug.Log("SceneSetup: Initialized GameManager");
        }
        
        // If this GameObject should persist between scenes
        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
            if (enableDebugLogging)
            {
                Debug.Log("SceneSetup: Set DontDestroyOnLoad on this GameObject");
            }
        }
    }
    
    private void Start()
    {
        // Make sure the player reference is properly set
        if (GameManager.Instance != null)
        {
            // First try using the specified reference
            if (playerReference != null)
            {
                GameManager.Instance.player = playerReference;
                if (enableDebugLogging)
                {
                    Debug.Log("SceneSetup: Set player reference in GameManager");
                }
            }
            // If no reference is specified but player is not set, try to find it
            else if (GameManager.Instance.player == null)
            {
                GameObject playerObj = GameManager.Instance.TryGetPlayer();
                if (playerObj != null && enableDebugLogging)
                {
                    Debug.Log("SceneSetup: Found and restored player reference");
                }
            }
        }
    }
}