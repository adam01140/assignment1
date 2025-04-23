using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/**
 * WaveCompletedPanel - Handles the UI panel that appears between waves or at the end of the game
 * 
 * Purpose:
 * - Displays a panel to the player after each wave is completed
 * - Shows statistics about the completed wave (time, enemies defeated, damage received)
 * - Provides a continue button to proceed to the next wave
 * - Can also function as a game over panel when the game is complete
 * 
 * Key Features:
 * - Automatically finds and initializes UI elements if not assigned in the inspector
 * - Can be created dynamically at runtime if not present in the scene
 * - Customizable display options for different stats
 * - Handles both "continue to next wave" and "game over" scenarios
 */
public class WaveCompletedPanel : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject panel;
    public TextMeshProUGUI waveInfoText;
    public TextMeshProUGUI statsText;
    public Button continueButton;
    public TextMeshProUGUI buttonText;

    [Header("Stats Display")]
    public bool showEnemiesDefeated = true;
    public bool showTimeSpent = true;
    public bool showWaveNumber = true;
    public bool showDamageReceived = true;

    [Header("Messages")]
    public string gameOverMessage = "Game Over!";
    public string victoryMessage = "Congratulations!\nYou've Completed All Waves!";
    public string returnToStartText = "Return to Start";
    public string continueText = "Continue";

    private EnemySpawner spawner;
    private bool isGameOver = false;
    private int currentWaveNumber;
    private int totalWaveCount;
    
    /**
     * Awake - Called when the script instance is being loaded
     * Sets up the panel and its components, finding them in the hierarchy if not assigned
     */
    private void Awake()
    {
        // Initialize the UI panel
        // If the panel is not assigned, try to find it
        if (panel == null)
        {
            Debug.LogWarning("WaveCompletedPanel: panel reference is missing. Try to find it by name.");
            
            // First try to find panel by name in children
            Transform panelTransform = transform.Find("Panel");
            if (panelTransform != null)
            {
                panel = panelTransform.gameObject;
                Debug.Log("WaveCompletedPanel: Found panel by name.");
            }
        }
        
        // Set up continue button click event
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners(); // Remove any existing listeners first
            continueButton.onClick.AddListener(OnButtonClicked);
            Debug.Log("WaveCompletedPanel: Button click listener added to continueButton");
            
            // Check if the button already has a SceneLoader component
            SceneLoader sceneLoader = continueButton.GetComponent<SceneLoader>();
            if (sceneLoader == null && isGameOver)
            {
                // If there's no SceneLoader component and we're in game-over mode, add one
                sceneLoader = continueButton.gameObject.AddComponent<SceneLoader>();
                Debug.Log("WaveCompletedPanel: Added SceneLoader component to ContinueButton for game-over state");
            }
        }
        else
        {
            Debug.LogError("WaveCompletedPanel: ContinueButton reference is missing! This will cause errors when displaying the panel.");
        }

        // Find the button text if not assigned
        if (buttonText == null && continueButton != null)
        {
            buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                Debug.Log("WaveCompletedPanel: Found buttonText in ContinueButton children");
            }
        }
        
        // If the panel is still null, use this GameObject
        if (panel == null)
        {
            panel = gameObject;
            Debug.LogWarning("WaveCompletedPanel: Using first child as panel since no 'Panel' was found.");
        }
        
        // Try to find remaining references if not set
        if (waveInfoText == null)
        {
            waveInfoText = transform.Find("HeaderText")?.GetComponent<TextMeshProUGUI>();
            if (waveInfoText != null) Debug.Log("WaveCompletedPanel: Found waveInfoText by name.");
        }
        
        if (statsText == null)
        {
            statsText = transform.Find("StatsText")?.GetComponent<TextMeshProUGUI>();
            if (statsText != null) Debug.Log("WaveCompletedPanel: Found statsText by name.");
        }
        
        if (continueButton == null)
        {
            continueButton = transform.Find("ContinueButton")?.GetComponent<Button>();
            if (continueButton != null)
            {
                Debug.Log("WaveCompletedPanel: Found continueButton by name.");
                continueButton.onClick.AddListener(OnButtonClicked);
            }
        }
        
        // Initially hide the panel
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
    
    /**
     * Initialize - Sets up the EnemySpawner reference
     * Must be called before using the panel
     */
    public void Initialize(EnemySpawner spawnerReference)
    {
        spawner = spawnerReference;
        
        // Double-check for missing references during initialization
        if (continueButton == null || panel == null)
        {
            FindMissingReferences();
        }
    }

    /**
     * ShowGameOverPanel - Special version of ShowPanel for when the game is completed
     * 
     * @param currentWave - The final wave that was completed
     * @param totalWaves - Total number of waves in the level
     * @param stats - Statistics about the final wave/game
     */
    public void ShowGameOverPanel(int currentWave, int totalWaves, WaveStatistics stats)
    {
        currentWaveNumber = currentWave;
        totalWaveCount = totalWaves;
        isGameOver = true;
        
        if (waveInfoText != null)
        {
            waveInfoText.text = gameOverMessage;
        }
        
        UpdateStatsText(stats);
        UpdateButtonText(returnToStartText);
        ShowPanel();
    }

    /**
     * ShowVictoryPanel - Special version of ShowPanel for when the game is completed
     * 
     * @param currentWave - The final wave that was completed
     * @param totalWaves - Total number of waves in the level
     * @param stats - Statistics about the final wave/game
     */
    public void ShowVictoryPanel(int currentWave, int totalWaves, WaveStatistics stats)
    {
        currentWaveNumber = currentWave;
        totalWaveCount = totalWaves;
        isGameOver = true;
        
        if (waveInfoText != null)
        {
            waveInfoText.text = victoryMessage;
        }
        
        UpdateStatsText(stats);
        UpdateButtonText(returnToStartText);
        ShowPanel();
    }
    
    /**
     * ShowPanel - Displays the panel with information about the completed wave
     * 
     * @param currentWave - The wave that was just completed
     * @param totalWaves - Total number of waves in the level
     * @param stats - Statistics about the completed wave
     */
    public void ShowPanel(int currentWave, int totalWaves, WaveStatistics stats)
    {
        currentWaveNumber = currentWave;
        totalWaveCount = totalWaves;
        isGameOver = false;
        
        if (panel == null)
        {
            Debug.LogError("WaveCompletedPanel: panel reference is missing! Cannot show panel.");
            FindMissingReferences();
            if (panel == null) return; // Still couldn't find it
        }
        
        // Set wave info text
        if (waveInfoText != null)
        {
            if (currentWave < totalWaves || totalWaves == int.MaxValue)
            {
                waveInfoText.text = $"Wave {currentWave} Completed!";
                UpdateButtonText(continueText);
            }
            else
            {
                ShowVictoryPanel(currentWave, totalWaves, stats);
                return;
            }
        }
        
        UpdateStatsText(stats);
        ShowPanel();
    }

    /**
     * UpdateStatsText - Formats and displays the statistics text based on enabled options
     * 
     * @param stats - Statistics about the completed wave
     */
    private void UpdateStatsText(WaveStatistics stats)
    {
        if (statsText != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            if (showWaveNumber && !isGameOver)
            {
                if (totalWaveCount < int.MaxValue)
                {
                    sb.AppendLine($"Wave: {currentWaveNumber}/{totalWaveCount}");
                }
                else
                {
                    sb.AppendLine($"Wave: {currentWaveNumber} (Endless Mode)");
                }
            }
            
            if (showTimeSpent)
            {
                sb.AppendLine($"Time spent: {FormatTime(stats.waveTimeInSeconds)}");
            }
            
            if (showEnemiesDefeated)
            {
                sb.AppendLine($"Enemies defeated: {stats.enemiesDefeated}");
            }
            
            if (showDamageReceived)
            {
                sb.AppendLine($"Damage received: {stats.damageReceived}");
            }
            
            statsText.text = sb.ToString();
        }
    }

    /**
     * UpdateButtonText - Updates the text of the continue button
     * 
     * @param text - New text for the button
     */
    private void UpdateButtonText(string text)
    {
        if (buttonText != null)
        {
            buttonText.text = text;
        }
        else if (continueButton != null)
        {
            // Try to find the buttonText again
            buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = text;
                Debug.Log("WaveCompletedPanel: Found and updated buttonText");
            }
        }
    }
    
    /**
     * ShowPanel - Displays the panel from view
     */
    private void ShowPanel()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
        else
        {
            Debug.LogError("WaveCompletedPanel: Cannot show panel, panel reference is null!");
        }
    }
    
    /**
     * HidePanel - Hides the panel from view
     */
    public void HidePanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
    
    /**
     * OnButtonClicked - Handler for continue button clicks
     * Either proceeds to next wave or returns to menu depending on isGameOver state
     */
    private void OnButtonClicked()
    {
        Debug.Log("WaveCompletedPanel: OnButtonClicked called, isGameOver: " + isGameOver);
        
        if (isGameOver)
        {
            Debug.Log("WaveCompletedPanel: Return to start button clicked - calling ReturnToMainScene");
            
            // Check if the button has a SceneLoader component
            if (continueButton != null)
            {
                SceneLoader sceneLoader = continueButton.GetComponent<SceneLoader>();
                if (sceneLoader != null)
                {
                    Debug.Log("WaveCompletedPanel: Using SceneLoader to return to main menu");
                    sceneLoader.LoadMainScene();
                }
                else
                {
                    // Hide panel before scene transition
                    HidePanel();
                    
                    // Use the public method
                    ReturnToMainScene();
                }
            }
            else
            {
                Debug.LogError("WaveCompletedPanel: continueButton is null in OnButtonClicked!");
                ReturnToMainScene(); // Try to recover anyway
            }
        }
        else
        {
            // Hide the panel and continue to next wave
            HidePanel();
            
            if (spawner != null)
            {
                spawner.NextWave();
            }
            else
            {
                Debug.LogError("WaveCompletedPanel: missing reference to EnemySpawner!");
            }
        }
    }
    
    /**
     * DirectLoadMainScene - Fallback method to directly load the main scene
     * Used when SceneLoader is not available
     */
    private void DirectLoadMainScene()
    {
        Debug.Log("WaveCompletedPanel: DirectLoadMainScene called - Loading Main scene directly");
        
        // Force a clean slate - try to release resources
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
        
        try
        {
            // Try using GameManager first (preferred method)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadScene("Main");
            }
            else
            {
                // Fallback to direct scene loading
                SceneManager.LoadScene("Main");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"WaveCompletedPanel: Error loading Main scene: {e.Message}");
            
            // Last resort
            SceneManager.LoadScene(0);
        }
    }
    
    /**
     * FormatTime - Helper method to format time in minutes:seconds
     * 
     * @param seconds - Time value to format
     * @return Formatted time string (mm:ss)
     */
    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int remainingSeconds = Mathf.FloorToInt(seconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, remainingSeconds);
    }
    
    /**
     * ReturnToMainScene - Loads the main menu scene
     * Used when the game is over and player wants to return to menu
     */
    public void ReturnToMainScene()
    {
        Debug.Log("WaveCompletedPanel: ReturnToMainScene called");
        DirectLoadMainScene();
    }
}