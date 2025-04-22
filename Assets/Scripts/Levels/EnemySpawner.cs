using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;
    
    private string currentLevelName;
    private Level currentLevel;
    private int currentWave = 0;
    private int totalWaves = 0;
    
    // UI for displaying wave information
    private GameObject waveInfoPanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateLevelSelectionButtons();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void CreateLevelSelectionButtons()
    {
        // Load all the available levels
        List<Level> levels = LevelData.Instance.GetAllLevels();
        
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("No levels found! Creating a default Start button.");
            CreateSingleButton("Start", 0);
            return;
        }
        
        // Calculate vertical spacing and starting position
        float buttonHeight = 60f;
        float spacing = 20f;
        float startY = (levels.Count - 1) * (buttonHeight + spacing) / 2;
        
        // Create a button for each level
        for (int i = 0; i < levels.Count; i++)
        {
            Level level = levels[i];
            float yPos = startY - i * (buttonHeight + spacing);
            CreateSingleButton(level.name, yPos);
        }
        
        // Add a title above the buttons
        GameObject titleObj = new GameObject("LevelSelectTitle");
        titleObj.transform.SetParent(level_selector.transform, false);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "SELECT LEVEL";
        titleText.fontSize = 36;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        
        // Position the title above the buttons
        float titleY = startY + buttonHeight + spacing;
        titleObj.transform.localPosition = new Vector3(0, titleY, 0);
    }
    
    private void CreateSingleButton(string levelName, float yPosition)
    {
        GameObject selector = Instantiate(button, level_selector.transform);
        selector.transform.localPosition = new Vector3(0, yPosition, 0);
        
        MenuSelectorController controller = selector.GetComponent<MenuSelectorController>();
        controller.spawner = this;
        controller.SetLevel(levelName);
    }

    public void StartLevel(string levelName)
    {
        currentLevelName = levelName;
        currentLevel = LevelData.Instance.GetLevel(levelName);
        currentWave = 0;
        
        if (currentLevel != null)
        {
            totalWaves = currentLevel.waves;
            
            // Handle endless mode, which doesn't specify a wave count
            if (totalWaves == 0 && currentLevelName == "Endless")
            {
                totalWaves = int.MaxValue; // Effectively infinite
            }
            
            level_selector.gameObject.SetActive(false);
            
            // Initialize the player
            GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
            
            // Start first wave
            StartWave();
        }
        else
        {
            Debug.LogError($"Could not find level data for '{levelName}'");
        }
    }
    
    public void StartWave()
    {
        // Increase wave count
        currentWave++;
        
        if (currentWave <= totalWaves)
        {
            Debug.Log($"Starting wave {currentWave} of {totalWaves}");
            StartCoroutine(SpawnWave(currentWave));
        }
        else
        {
            // Player has completed all waves
            GameManager.Instance.state = GameManager.GameState.GAMEOVER;
            Debug.Log("All waves completed!");
            // TODO: Show victory screen
        }
    }

    public void NextWave()
    {
        StartWave();
    }

    IEnumerator SpawnWave(int waveNumber)
    {
        // Initial countdown
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        
        Debug.Log($"Wave {waveNumber} countdown starting...");
        
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }
        
        GameManager.Instance.state = GameManager.GameState.INWAVE;
        
        Debug.Log($"Wave {waveNumber} started!");
        
        // Spawn each enemy type according to the level configuration
        foreach (Spawn spawnConfig in currentLevel.spawns)
        {
            yield return StartCoroutine(SpawnEnemiesOfType(spawnConfig, waveNumber));
        }
        
        // Wait until all enemies are defeated
        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        
        Debug.Log($"Wave {waveNumber} completed!");
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
    }
    
    IEnumerator SpawnEnemiesOfType(Spawn spawnConfig, int waveNumber)
    {
        // Variables for RPN evaluation
        Dictionary<string, int> variables = new Dictionary<string, int>();
        variables["wave"] = waveNumber;
        
        // Get the enemy data
        Enemy enemyData = EnemyData.Instance.GetEnemy(spawnConfig.enemy);
        if (enemyData == null)
        {
            Debug.LogError($"Enemy data not found for '{spawnConfig.enemy}'");
            yield break;
        }
        
        // Add base stats from enemy data
        variables["base"] = enemyData.hp;
        
        // Evaluate how many of this enemy to spawn
        int count = RPNEvaluator.Evaluate(spawnConfig.count, variables);
        Debug.Log($"Spawning {count} {spawnConfig.enemy}(s) in wave {waveNumber}");
        
        // Evaluate HP, speed, and damage
        int hp = spawnConfig.hp != null ? 
            RPNEvaluator.Evaluate(spawnConfig.hp, variables) : 
            enemyData.hp;
            
        variables["base"] = enemyData.speed;
        int speed = spawnConfig.speed != null ? 
            RPNEvaluator.Evaluate(spawnConfig.speed, variables) : 
            enemyData.speed;
            
        variables["base"] = enemyData.damage;
        int damage = spawnConfig.damage != null ? 
            RPNEvaluator.Evaluate(spawnConfig.damage, variables) : 
            enemyData.damage;
        
        // Get delay between spawns
        float delay = spawnConfig.delay != null ? 
            RPNEvaluator.Evaluate(spawnConfig.delay, variables) : 
            2;
            
        // Get the spawn sequence
        List<int> sequence = spawnConfig.GetSequence();
        
        // Spawn enemies according to the sequence pattern
        int enemiesLeft = count;
        int sequenceIndex = 0;
        
        while (enemiesLeft > 0)
        {
            // Get number to spawn in this batch from the sequence
            int batchSize = sequence[sequenceIndex % sequence.Count];
            
            // Cap the batch size at the remaining enemies
            batchSize = Mathf.Min(batchSize, enemiesLeft);
            
            // Spawn the enemies in this batch
            for (int i = 0; i < batchSize; i++)
            {
                SpawnSingleEnemy(spawnConfig.enemy, enemyData.sprite, hp, speed, damage, spawnConfig.location);
                enemiesLeft--;
                
                // Small delay between individual enemies in the same batch
                yield return new WaitForSeconds(0.1f);
            }
            
            // Move to next sequence element
            sequenceIndex++;
            
            // Wait for the configured delay between batches if there are more enemies to spawn
            if (enemiesLeft > 0)
            {
                yield return new WaitForSeconds(delay);
            }
        }
    }
    
    public GameObject SpawnSingleEnemy(string enemyType, int spriteIndex, int hp, int speed, int damage, string location)
    {
        // Select a spawn point based on location parameter
        SpawnPoint spawn_point;
        
        if (location == "random")
        {
            // Any spawn point
            spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        }
        else
        {
            // Filter spawn points by type
            string[] locationParts = location.Split(' ');
            string spawnType = locationParts.Length > 1 ? locationParts[1] : "";
            
            SpawnPoint[] matchingPoints = SpawnPoints.Where(sp => sp.type == spawnType).ToArray();
            
            if (matchingPoints.Length > 0)
            {
                spawn_point = matchingPoints[Random.Range(0, matchingPoints.Length)];
            }
            else
            {
                // Fallback to any spawn point
                spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
                Debug.LogWarning($"No spawn points of type '{spawnType}' found. Using random spawn point instead.");
            }
        }
        
        // Add a random offset within the spawn area
        Vector2 offset = Random.insideUnitCircle * 1.8f;
        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        
        // Create the enemy instance
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);
        
        // Set the sprite
        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(spriteIndex);
        
        // Configure the enemy controller
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(hp, Hittable.Team.MONSTERS, new_enemy);
        en.speed = speed;
        en.damage = damage;
        
        // Add to the game manager's enemy list
        GameManager.Instance.AddEnemy(new_enemy);
        
        return new_enemy;
    }
}
