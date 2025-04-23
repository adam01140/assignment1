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
    
    [Header("Wave UI")]
    public WaveCompletedPanel waveCompletedPanel;
    
    private string currentLevelName;
    private Level currentLevel;
    private int currentWave = 0;
    private int totalWaves = 0;
    
    private WaveStatistics currentWaveStats;
    private List<WaveStatistics> allWaveStats = new List<WaveStatistics>();
    
    private GameObject waveInfoPanel;

    void Awake()
    {
        InitializeWaveCompletedPanel();
        
        EnemyController.OnEnemyDefeated += OnEnemyDefeated;
        PlayerController.OnDamageReceived += OnPlayerDamageReceived;
    }
    
    public void InitializeWaveCompletedPanel()
    {
        if (waveCompletedPanel != null)
        {
            waveCompletedPanel.Initialize(this);
            return;
        }
        
        waveCompletedPanel = FindObjectOfType<WaveCompletedPanel>();
        if (waveCompletedPanel != null)
        {
            waveCompletedPanel.Initialize(this);
        }
    }
    
    public void SetWaveCompletedPanel(WaveCompletedPanel panel)
    {
        if (panel != null)
        {
            waveCompletedPanel = panel;
            waveCompletedPanel.Initialize(this);
        }
    }
    
    void OnDestroy()
    {
        EnemyController.OnEnemyDefeated -= OnEnemyDefeated;
        PlayerController.OnDamageReceived -= OnPlayerDamageReceived;
    }

    void Start()
    {
        CreateLevelSelectionButtons();
    }

    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.WAVEEND && waveCompletedPanel != null)
        {
            if (!waveCompletedPanel.panel.activeSelf)
            {
                if (currentWaveStats != null)
                {
                    currentWaveStats.FinalizeStats();
                    
                    waveCompletedPanel.ShowPanel(currentWave, totalWaves, currentWaveStats);
                }
            }
        }
    }
    
    public int GetCurrentWave()
    {
        return currentWave;
    }
    
    public int GetTotalWaves()
    {
        return totalWaves;
    }
    
    public WaveStatistics GetCurrentWaveStats()
    {
        if (currentWaveStats == null)
        {
            currentWaveStats = WaveStatistics.CreateForWave(currentWave);
            currentWaveStats.FinalizeStats();
        }
        return currentWaveStats;
    }
    
    private void OnEnemyDefeated()
    {
        if (currentWaveStats != null)
        {
            currentWaveStats.AddEnemyDefeated();
        }
    }
    
    private void OnPlayerDamageReceived(int damage)
    {
        if (currentWaveStats != null)
        {
            currentWaveStats.AddDamageReceived(damage);
        }
    }
    
    private void CreateLevelSelectionButtons()
    {
        List<Level> levels = LevelData.Instance.GetAllLevels();
        
        if (levels == null || levels.Count == 0)
        {
            CreateSingleButton("Start", 0);
            return;
        }
        
        float buttonHeight = 60f;
        float spacing = 20f;
        float startY = (levels.Count - 1) * (buttonHeight + spacing) / 2;
        
        for (int i = 0; i < levels.Count; i++)
        {
            Level level = levels[i];
            float yPos = startY - i * (buttonHeight + spacing);
            CreateSingleButton(level.name, yPos);
        }
        
        GameObject titleObj = new GameObject("LevelSelectTitle");
        titleObj.transform.SetParent(level_selector.transform, false);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "SELECT LEVEL";
        titleText.fontSize = 36;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        
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
        
        allWaveStats.Clear();
        
        if (currentLevel != null)
        {
            totalWaves = currentLevel.waves;
            
            if (totalWaves == 0 && currentLevelName == "Endless")
            {
                totalWaves = int.MaxValue;
            }
            
            level_selector.gameObject.SetActive(false);
            
            GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
            
            StartWave();
        }
    }
    
    public void StartWave()
    {
        currentWave++;
        
        if (waveCompletedPanel != null)
        {
            waveCompletedPanel.HidePanel();
        }
        
        if (currentWave <= totalWaves)
        {
            currentWaveStats = WaveStatistics.CreateForWave(currentWave);
            allWaveStats.Add(currentWaveStats);
            
            StartCoroutine(SpawnWave(currentWave));
        }
        else
        {
            GameManager.Instance.state = GameManager.GameState.GAMEOVER;
        }
    }

    public void NextWave()
    {
        StartWave();
    }

    IEnumerator SpawnWave(int waveNumber)
    {
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }
        
        GameManager.Instance.state = GameManager.GameState.INWAVE;
        
        foreach (Spawn spawnConfig in currentLevel.spawns)
        {
            yield return StartCoroutine(SpawnEnemiesOfType(spawnConfig, waveNumber));
        }
        
        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
    }

    IEnumerator SpawnEnemiesOfType(Spawn spawnConfig, int waveNumber)
    {
        Dictionary<string, int> variables = new Dictionary<string, int>();
        variables["wave"] = waveNumber;
        
        Enemy enemyData = EnemyData.Instance.GetEnemy(spawnConfig.enemy);
        if (enemyData == null)
        {
            yield break;
        }
        
        variables["base"] = enemyData.hp;
        
        int count = RPNEvaluator.Evaluate(spawnConfig.count, variables);
        
        currentWaveStats.enemiesSpawned += count;
        
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
        
        float delay = spawnConfig.delay != null ? 
            RPNEvaluator.Evaluate(spawnConfig.delay, variables) : 
            2;
        
        List<int> sequence = spawnConfig.GetSequence();
        
        int enemiesLeft = count;
        int sequenceIndex = 0;
        
        while (enemiesLeft > 0)
        {
            int batchSize = sequence[sequenceIndex % sequence.Count];
            
            batchSize = Mathf.Min(batchSize, enemiesLeft);
            
            for (int i = 0; i < batchSize; i++)
            {
                SpawnSingleEnemy(spawnConfig.enemy, enemyData.sprite, hp, speed, damage, spawnConfig.location);
                enemiesLeft--;
                
                yield return new WaitForSeconds(0.1f);
            }
            
            sequenceIndex++;
            
            if (enemiesLeft > 0)
            {
                yield return new WaitForSeconds(delay);
            }
        }
    }
    
    public GameObject SpawnSingleEnemy(string enemyType, int spriteIndex, int hp, int speed, int damage, string location)
    {
        SpawnPoint spawn_point;
        
        if (location == "random")
        {
            spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        }
        else
        {
            string[] locationParts = location.Split(' ');
            string spawnType = locationParts.Length > 1 ? locationParts[1] : "";
            
            SpawnPoint[] matchingPoints = SpawnPoints.Where(sp => sp.type == spawnType).ToArray();
            
            if (matchingPoints.Length > 0)
            {
                spawn_point = matchingPoints[Random.Range(0, matchingPoints.Length)];
            }
            else
            {
                spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
            }
        }
        
        Vector2 offset = Random.insideUnitCircle * 1.8f;
        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);
        
        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(spriteIndex);
        
        if (enemyType == "necromancer")
        {
            EnemyController oldController = new_enemy.GetComponent<EnemyController>();
            if (oldController != null)
            {
                Destroy(oldController);
            }
            
            NecromancerController necroController = new_enemy.AddComponent<NecromancerController>();
            necroController.hp = new Hittable(hp, Hittable.Team.MONSTERS, new_enemy);
            necroController.speed = speed;
            necroController.damage = damage;
        }
        else
        {
            EnemyController en = new_enemy.GetComponent<EnemyController>();
            en.hp = new Hittable(hp, Hittable.Team.MONSTERS, new_enemy);
            en.speed = speed;
            en.damage = damage;
        }
        
        GameManager.Instance.AddEnemy(new_enemy);
        
        return new_enemy;
    }
}
