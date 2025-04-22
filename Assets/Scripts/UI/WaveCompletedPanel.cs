using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveCompletedPanel : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject panel;
    public TextMeshProUGUI waveInfoText;
    public TextMeshProUGUI statsText;
    public Button continueButton;

    [Header("Stats Display")]
    public bool showEnemiesDefeated = true;
    public bool showTimeSpent = true;
    public bool showWaveNumber = true;
    public bool showDamageDealt = true;
    public bool showDamageReceived = true;

    private EnemySpawner spawner;
    
    private void Awake()
    {
        // Make sure panel is hidden at start
        if (panel != null)
        {
            panel.SetActive(false);
        }
        
        // Set up continue button
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }
    }
    
    public void Initialize(EnemySpawner spawnerReference)
    {
        spawner = spawnerReference;
    }
    
    public void ShowPanel(int currentWave, int totalWaves, WaveStatistics stats)
    {
        if (panel == null)
        {
            Debug.LogError("Wave completed panel is missing panel reference");
            return;
        }
        
        // Set wave info text
        if (waveInfoText != null)
        {
            if (currentWave < totalWaves || totalWaves == int.MaxValue)
            {
                waveInfoText.text = $"Wave {currentWave} Completed!";
            }
            else
            {
                waveInfoText.text = "All Waves Completed!";
                
                // For the final wave, change the continue button text to "Finish"
                if (continueButton != null && continueButton.GetComponentInChildren<TextMeshProUGUI>() != null)
                {
                    continueButton.GetComponentInChildren<TextMeshProUGUI>().text = "Finish";
                }
            }
        }
        
        // Build the stats text
        if (statsText != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            if (showWaveNumber && totalWaves < int.MaxValue)
            {
                sb.AppendLine($"Wave: {currentWave}/{totalWaves}");
            }
            else if (showWaveNumber && totalWaves == int.MaxValue)
            {
                sb.AppendLine($"Wave: {currentWave} (Endless Mode)");
            }
            
            if (showTimeSpent)
            {
                sb.AppendLine($"Time spent: {FormatTime(stats.waveTimeInSeconds)}");
            }
            
            if (showEnemiesDefeated)
            {
                sb.AppendLine($"Enemies defeated: {stats.enemiesDefeated}");
            }
            
            if (showDamageDealt)
            {
                sb.AppendLine($"Damage dealt: {stats.damageDealt}");
            }
            
            if (showDamageReceived)
            {
                sb.AppendLine($"Damage received: {stats.damageReceived}");
            }
            
            statsText.text = sb.ToString();
        }
        
        // Show the panel
        panel.SetActive(true);
    }
    
    public void HidePanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
    
    private void OnContinueButtonClicked()
    {
        // Hide the panel
        HidePanel();
        
        // Tell the spawner to move to the next wave
        if (spawner != null)
        {
            spawner.NextWave();
        }
        else
        {
            Debug.LogError("WaveCompletedPanel missing reference to EnemySpawner");
        }
    }
    
    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60F);
        int secs = Mathf.FloorToInt(seconds - minutes * 60);
        return $"{minutes:00}:{secs:00}";
    }
} 