using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public Hittable hp;
    public HealthBar healthui;
    public ManaBar manaui;

    public SpellCaster spellcaster;
    public SpellUI spellui;

    public int speed;

    public Unit unit;
    
    // Add stats tracking
    public static System.Action<int> OnDamageReceived;

    // Reference to the wave completed panel - make it [SerializeField] to allow setting in Inspector
    [SerializeField] private WaveCompletedPanel waveCompletedPanel;
    [SerializeField] private EnemySpawner enemySpawner;
    
    // Flag to track if we've logged the initialization message
    private bool hasLoggedInitialization = false;
    
    // Flag to track if we've already initialized HP
    private bool hasInitializedHP = false;

    private void Awake()
    {
        // Set the player reference immediately on Awake
        if (GameManager.Instance != null)
        {
            GameManager.Instance.player = gameObject;
            Debug.Log("Player reference set in GameManager during Awake");
        }
        else
        {
            Debug.LogError("GameManager.Instance is null in PlayerController.Awake");
        }
        
        // Try to initialize HP early if possible
        TryInitializeHP();
    }

    private void OnEnable()
    {
        // Ensure player reference is set when the object is enabled
        if (GameManager.Instance != null)
        {
            GameManager.Instance.player = gameObject;
            if (!hasLoggedInitialization)
            {
                Debug.Log("Player reference set in GameManager during OnEnable");
                hasLoggedInitialization = true;
            }
        }
        else
        {
            Debug.LogError("GameManager.Instance is null in PlayerController.OnEnable");
        }
    }

    private void OnDisable()
    {
        // Only clear the reference if it's pointing to this gameObject
        if (GameManager.Instance != null && GameManager.Instance.player == gameObject)
        {
            // Don't set player to null when disabling, only track that it's disabled
            // This prevents issues with enemies that might be checking for the player
            Debug.Log("PlayerController disabled, but maintaining reference in GameManager");
            // GameManager.Instance.player = null; - Commented out to maintain reference
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unit = GetComponent<Unit>();
        
        // Double-check the player reference in Start as well
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.player == null)
            {
                GameManager.Instance.player = gameObject;
                Debug.Log("Player reference restored in GameManager during Start");
            }
        }
        else
        {
            Debug.LogError("GameManager.Instance is null in PlayerController.Start");
        }
        
        // Initialize hp if it wasn't done in Awake
        if (!hasInitializedHP)
        {
            TryInitializeHP();
        }
        
        // Ensure we have references to required components
        EnsureUIComponents();
        
        // Find the EnemySpawner in the scene if not set in inspector
        if (enemySpawner == null)
        {
            enemySpawner = GameObject.FindObjectOfType<EnemySpawner>();
            
            // Log warning instead of error
            if (enemySpawner == null)
            {
                Debug.LogWarning("Could not find EnemySpawner in the scene! Some game functionality may be limited.");
            }
        }
    }

    // Update checks periodically that the player reference is intact
    void Update()
    {
        // Make sure the player reference is maintained
        if (GameManager.Instance != null && GameManager.Instance.player == null)
        {
            GameManager.Instance.player = gameObject;
            Debug.LogWarning("Player reference was null and has been restored in Update");
        }
        
        // Make sure HP is initialized
        if (hp == null && !hasInitializedHP)
        {
            TryInitializeHP();
        }
    }
    
    // Try to initialize HP if it's null
    private void TryInitializeHP()
    {
        if (hp == null)
        {
            hp = new Hittable(100, Hittable.Team.PLAYER, gameObject);
            hp.OnDeath += Die;
            hp.OnDamage += OnDamageTaken;
            hp.team = Hittable.Team.PLAYER;
            hasInitializedHP = true;
            
            // Update UI if available
            if (healthui != null)
            {
                healthui.SetHealth(hp);
            }
            
            Debug.Log("Initialized HP in PlayerController");
        }
    }

    // This method ensures the UI components exist in the scene
    private void EnsureUIComponents()
    {
        // Try to find the WaveCompletedPanel if not already set
        if (waveCompletedPanel == null)
        {
            waveCompletedPanel = GameObject.FindObjectOfType<WaveCompletedPanel>();
            
            // If still not found, try to create one
            if (waveCompletedPanel == null)
            {
                CreateWaveCompletedPanel();
            }
        }
        
        // Make sure the created/found panel is initialized
        if (waveCompletedPanel != null && enemySpawner != null)
        {
            waveCompletedPanel.Initialize(enemySpawner);
            enemySpawner.SetWaveCompletedPanel(waveCompletedPanel);
        }
    }
    
    // Creates a Wave Completed Panel if one doesn't exist
    private void CreateWaveCompletedPanel()
    {
        // Find Canvas to parent the panel to
        Canvas canvas = FindMainCanvas();
        if (canvas == null)
        {
            Debug.LogWarning("Could not find a Canvas to create WaveCompletedPanel. Game will continue without showing wave completion UI.");
            return;
        }
        
        // Create the panel structure
        GameObject panelObj = new GameObject("WaveCompletedPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        
        // Add the WaveCompletedPanel component
        waveCompletedPanel = panelObj.AddComponent<WaveCompletedPanel>();
        
        // Create the panel background
        GameObject panelBackground = new GameObject("Panel");
        panelBackground.transform.SetParent(panelObj.transform, false);
        
        // Add UI components
        Image panelImage = panelBackground.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        // Set RectTransform properties for centered panel
        RectTransform panelRect = panelBackground.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(500, 400);
        
        // Create header text
        GameObject headerObj = new GameObject("WaveInfoText");
        headerObj.transform.SetParent(panelBackground.transform, false);
        TMPro.TextMeshProUGUI headerText = headerObj.AddComponent<TMPro.TextMeshProUGUI>();
        headerText.text = "Wave Completed!";
        headerText.fontSize = 36;
        headerText.alignment = TMPro.TextAlignmentOptions.Center;
        headerText.color = Color.white;
        
        // Position the header
        RectTransform headerRect = headerObj.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.sizeDelta = new Vector2(0, 80);
        headerRect.anchoredPosition = new Vector2(0, 0);
        
        // Create stats text
        GameObject statsObj = new GameObject("StatsText");
        statsObj.transform.SetParent(panelBackground.transform, false);
        TMPro.TextMeshProUGUI statsText = statsObj.AddComponent<TMPro.TextMeshProUGUI>();
        statsText.text = "Loading stats...";
        statsText.fontSize = 24;
        statsText.alignment = TMPro.TextAlignmentOptions.Left;
        statsText.color = Color.white;
        
        // Position the stats
        RectTransform statsRect = statsObj.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0, 0);
        statsRect.anchorMax = new Vector2(1, 1);
        statsRect.pivot = new Vector2(0.5f, 0.5f);
        statsRect.sizeDelta = new Vector2(-40, -160);
        statsRect.anchoredPosition = new Vector2(0, -20);
        
        // Create continue button
        GameObject buttonObj = new GameObject("ContinueButton");
        buttonObj.transform.SetParent(panelBackground.transform, false);
        Button continueButton = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.2f);
        
        // Button text
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        TMPro.TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TMPro.TextMeshProUGUI>();
        buttonText.text = "Continue";
        buttonText.fontSize = 24;
        buttonText.alignment = TMPro.TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        
        // Position the button text
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = new Vector2(0, 0);
        buttonTextRect.anchorMax = new Vector2(1, 1);
        buttonTextRect.pivot = new Vector2(0.5f, 0.5f);
        buttonTextRect.sizeDelta = new Vector2(0, 0);
        
        // Position the button
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0);
        buttonRect.anchorMax = new Vector2(0.5f, 0);
        buttonRect.pivot = new Vector2(0.5f, 0);
        buttonRect.sizeDelta = new Vector2(200, 60);
        buttonRect.anchoredPosition = new Vector2(0, 40);
        
        // Set up references in the WaveCompletedPanel component
        waveCompletedPanel.panel = panelBackground;
        waveCompletedPanel.waveInfoText = headerText;
        waveCompletedPanel.statsText = statsText;
        waveCompletedPanel.continueButton = continueButton;
        waveCompletedPanel.buttonText = buttonText;
        
        // Initially hide the panel
        panelBackground.SetActive(false);
        
        Debug.Log("Created WaveCompletedPanel since one was not found in the scene.");
    }
    
    // Finds the main canvas in the scene
    private Canvas FindMainCanvas()
    {
        Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>();
        
        // First try to find the Canvas object
        foreach (Canvas canvas in canvases)
        {
            if (canvas.gameObject.name == "Canvas")
            {
                return canvas;
            }
        }
        
        // If not found, return any canvas
        if (canvases.Length > 0)
        {
            return canvases[0];
        }
        
        // If no canvas exists, create one
        GameObject canvasObj = new GameObject("Canvas");
        Canvas newCanvas = canvasObj.AddComponent<Canvas>();
        newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        return newCanvas;
    }

    public void StartLevel()
    {
        // Ensure player reference is set
        if (GameManager.Instance != null && GameManager.Instance.player == null)
        {
            GameManager.Instance.player = gameObject;
            Debug.Log("Player reference restored in StartLevel");
        }
        
        // Only initialize components if not already done
        if (spellcaster == null)
        {
            spellcaster = new SpellCaster(125, 8, Hittable.Team.PLAYER);
            StartCoroutine(spellcaster.ManaRegeneration());
        }
        
        // Only initialize HP if it hasn't been done yet
        if (!hasInitializedHP)
        {
            TryInitializeHP();
        }

        // tell UI elements what to show
        if (healthui != null)
        {
            healthui.SetHealth(hp);
        }
        
        if (manaui != null)
        {
            manaui.SetSpellCaster(spellcaster);
        }
        
        if (spellui != null && spellcaster != null)
        {
            spellui.SetSpell(spellcaster.spell);
        }
    }
    
    void OnDamageTaken(Damage damage)
    {
        // Report damage received for statistics
        OnDamageReceived?.Invoke(damage.amount);
    }

    void OnAttack(InputValue value)
    {
        // Ensure player reference is still valid
        if (GameManager.Instance != null && GameManager.Instance.player == null)
        {
            GameManager.Instance.player = gameObject;
        }
        
        // Check if spellcaster is initialized
        if (spellcaster == null)
        {
            Debug.LogWarning("Spellcaster is null in OnAttack - initializing new spellcaster");
            spellcaster = new SpellCaster(125, 8, Hittable.Team.PLAYER);
            StartCoroutine(spellcaster.ManaRegeneration());
            
            // Update UI if available
            if (manaui != null)
            {
                manaui.SetSpellCaster(spellcaster);
            }
            
            if (spellui != null)
            {
                spellui.SetSpell(spellcaster.spell);
            }
        }
        
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;
        StartCoroutine(spellcaster.Cast(transform.position, worldPos));
    }

    void OnMove(InputValue value)
    {
        unit.movement = value.Get<Vector2>() * speed;
    }

    void Die()
    {
        // Ensure we maintain the player reference even after death
        // This allows enemy controllers to still find the player and avoid errors
        
        // Show game over panel
        ShowGameOverPanel();
    }

    private void ShowGameOverPanel()
    {
        if (enemySpawner != null && waveCompletedPanel != null)
        {
            // Get current wave info
            int currentWave = enemySpawner.GetCurrentWave();
            int totalWaves = enemySpawner.GetTotalWaves();
            WaveStatistics currentStats = enemySpawner.GetCurrentWaveStats();
            
            // Show game over panel
            waveCompletedPanel.ShowGameOverPanel(currentWave, totalWaves, currentStats);
            
            // Set game state to game over
            if (GameManager.Instance != null)
            {
                GameManager.Instance.state = GameManager.GameState.GAMEOVER;
            }
        }
    }
}

