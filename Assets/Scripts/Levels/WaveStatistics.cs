using UnityEngine;

[System.Serializable]
public class WaveStatistics
{
    public int waveNumber;
    public float waveTimeInSeconds;
    
    public int enemiesDefeated;
    public int enemiesSpawned;
    
    public int damageDealt;
    public int damageReceived;
    
    public int goldCollected;
    public int healthRestored;
    
    public float startTime;
    public float endTime;
    
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
    
    public void FinalizeStats()
    {
        endTime = Time.time;
        waveTimeInSeconds = endTime - startTime;
    }
    
    public void AddEnemyDefeated()
    {
        enemiesDefeated++;
    }
    
    public void AddEnemySpawned()
    {
        enemiesSpawned++;
    }
    
    public void AddDamageDealt(int amount)
    {
        damageDealt += amount;
    }
    
    public void AddDamageReceived(int amount)
    {
        damageReceived += amount;
    }
} 
