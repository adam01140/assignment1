using UnityEngine;

public class GameManagerSetup : MonoBehaviour
{
    [SerializeField] private GameObject playerObject;
    
    private void Awake()
    {
        // Force creation of GameManager singleton
        var manager = GameManager.Instance;
        Debug.Log("GameManager initialized through GameManagerSetup");
        
        // Set player reference if provided
        if (playerObject != null && GameManager.Instance.player == null)
        {
            GameManager.Instance.player = playerObject;
            Debug.Log("Set player reference in GameManager");
        }
    }
    
    private void Start()
    {
        // Secondary check for player reference
        if (GameManager.Instance != null && GameManager.Instance.player == null)
        {
            // Try to find player by tag
            GameObject player = GameObject.FindWithTag("unit");
            if (player != null)
            {
                GameManager.Instance.player = player;
                Debug.Log("Found player by tag and set reference in GameManager");
            }
        }
    }
}