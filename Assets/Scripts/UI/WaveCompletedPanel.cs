using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
    
    private void Awake()
    {
        // Make sure panel is hidden at start
        if (panel != null)
        {
            panel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("WaveCompletedPanel: panel reference is missing. Try to find it by name.");
            // Try to find the panel by name
            Transform panelTransform = transform.Find("Panel");
            if (panelTransform != null)
            {
                panel = panelTransform.gameObject;
                panel.SetActive(false);
                Debug.Log("WaveCompletedPanel: Found panel by name.");
            }
        }
        
        // Find UI components if they aren't set
        FindMissingReferences();
        
        // Set up continue button
        if (continueButton != null)
        {
            // Make sure the OnButtonClicked method is connected to the button click event
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

        // Cache button text component
        if (buttonText == null && continueButton != null)
        {
            buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                Debug.Log("WaveCompletedPanel: Found buttonText in ContinueButton children");
            }
        }
    }
    
    private void FindMissingReferences()
    {
        // Find the panel if it's not assigned
        if (panel == null)
        {
            panel = transform.Find("Panel")?.gameObject;
            if (panel == null && transform.childCount > 0)
            {
                // Use the first child as a fallback
                panel = transform.GetChild(0).gameObject;
                Debug.LogWarning("WaveCompletedPanel: Using first child as panel since no 'Panel' was found.");
            }
        }
        
        // If we found the panel, use it to find other references
        if (panel != null)
        {
            if (waveInfoText == null)
            {
                waveInfoText = panel.transform.Find("HeaderText")?.GetComponent<TextMeshProUGUI>();
                if (waveInfoText != null) Debug.Log("WaveCompletedPanel: Found waveInfoText by name.");
            }
            
            if (statsText == null)
            {
                statsText = panel.transform.Find("StatsText")?.GetComponent<TextMeshProUGUI>();
                if (statsText != null) Debug.Log("WaveCompletedPanel: Found statsText by name.");
            }
            
            if (continueButton == null)
            {
                continueButton = panel.transform.Find("ContinueButton")?.GetComponent<Button>();
                if (continueButton != null) 
                {
                    Debug.Log("WaveCompletedPanel: Found continueButton by name.");
                    
                    // Also try to find the button text
                    if (buttonText == null)
                    {
                        buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
                    }
                }
            }
        }
    }
    
    public void Initialize(EnemySpawner spawnerReference)
    {
        spawner = spawnerReference;
        
        // Double-check for missing references during initialization
        if (continueButton == null || panel == null)
        {
            FindMissingReferences();
        }
    }

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
    
    public void HidePanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
    
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
    
    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int remainingSeconds = Mathf.FloorToInt(seconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, remainingSeconds);
    }
    
    public void ReturnToMainScene()
    {
        Debug.Log("WaveCompletedPanel: ReturnToMainScene called");
        DirectLoadMainScene();
    }
}