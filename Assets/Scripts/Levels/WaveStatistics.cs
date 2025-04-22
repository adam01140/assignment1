using UnityEngine;

[System.Serializable]
public class WaveStatistics
{
    // Basic wave info
    public int waveNumber;
    public float waveTimeInSeconds;
    
    // Enemy statistics
    public int enemiesDefeated;
    public int enemiesSpawned;
    
    // Damage statistics
    public int damageDealt;
    public int damageReceived;
    
    // Other potential stats to track
    public int goldCollected;
    public int healthRestored;
    
    // Timestamp info
    public float startTime;
    public float endTime;
    
    // Create new statistics for a wave
    public static WaveStatistics CreateForWave(int wave)
    {
        WaveStatistics stats = new WaveStatistics();
        stats.waveNumber = wave;
        stats.startTime = Time.time;
        stats.enemiesDefeated = 0;
        stats.enemiesSpawned = 0;
        stats.damageDealt = 0;
        stats.damageReceived = 0;
        stats.goldCollected = 0;
        stats.healthRestored = 0;
        
        return stats;
    }
    
    // Call this when the wave ends to finalize statistics
    public void FinalizeStats()
    {
        endTime = Time.time;
        waveTimeInSeconds = endTime - startTime;
    }
    
    // Add enemy defeated
    public void AddEnemyDefeated()
    {
        enemiesDefeated++;
    }
    
    // Add enemy spawned
    public void AddEnemySpawned()
    {
        enemiesSpawned++;
    }
    
    // Add damage dealt to enemies
    public void AddDamageDealt(int amount)
    {
        damageDealt += amount;
    }
    
    // Add damage received by player
    public void AddDamageReceived(int amount)
    {
        damageReceived += amount;
    }
} 