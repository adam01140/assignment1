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
    
    private string currentLevel;

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
        currentLevel = levelName;
        level_selector.gameObject.SetActive(false);
        
        // Initialize the player
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        
        // Start spawning enemies
        StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        // Initial countdown
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }
        
        GameManager.Instance.state = GameManager.GameState.INWAVE;
        
        // Spawn 10 zombies for now (will be replaced in next step)
        for (int i = 0; i < 10; ++i)
        {
            yield return SpawnZombie();
        }
        
        // Wait until all enemies are defeated
        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
    }

    IEnumerator SpawnZombie()
    {
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;
                
        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(0);
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(50, Hittable.Team.MONSTERS, new_enemy);
        en.speed = 10;
        GameManager.Instance.AddEnemy(new_enemy);
        yield return new WaitForSeconds(0.5f);
    }
}
