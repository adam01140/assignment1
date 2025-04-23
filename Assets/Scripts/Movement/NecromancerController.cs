using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Special controller for Necromancer enemies with unique behaviors
public class NecromancerController : EnemyController
{
    [Header("Necromancer Specific")]
    public float summonCooldown = 12f;  // Cooldown between summons in seconds
    public float lastSummonTime = 0f;   // Time of last summon
    public int maxMinions = 3;          // Maximum number of summoned minions at one time
    public Color tintColor = new Color(0.7f, 0.0f, 0.7f, 1.0f);  // Purple tint for necromancers
    
    private SpriteRenderer spriteRenderer;
    private int currentMinions = 0;
    
    public GameObject skeleton;
    public GameObject minionPrefab;
    public Transform summonSpot;

    public float minSummonCooldown = 8f;
    public float maxSummonCooldown = 12f;

    private float nextSummonTime;
    private List<GameObject> activeMinions = new List<GameObject>();

    // Visual effects
    public GameObject summoning;
    public GameObject deathParticles;
    public List<Sprite> necromancerSprites;
    
    // Reference to the enemy spawner (since it's not a singleton)
    private EnemySpawner enemySpawner;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        // Call base class Start method
        base.Start();
        
        // Find the EnemySpawner in the scene
        enemySpawner = GameObject.FindObjectOfType<EnemySpawner>();
        
        // Apply visual effects
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Apply tint to the necromancer
            spriteRenderer.color = tintColor;
            
            // Make necromancer slightly larger
            transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
        }
        
        // Start with a random summon time offset so not all necromancers summon at once
        lastSummonTime = Time.time - Random.Range(0f, summonCooldown * 0.5f);

        // Set a random sprite if available
        if (spriteRenderer != null && necromancerSprites != null && necromancerSprites.Count > 0)
        {
            int randomIndex = Random.Range(0, necromancerSprites.Count);
            spriteRenderer.sprite = necromancerSprites[randomIndex];
        }

        // Set initial summon time
        SetNextSummonTime();
    }
    
    private void SetNextSummonTime()
    {
        nextSummonTime = Time.time + Random.Range(minSummonCooldown, maxSummonCooldown);
    }
    
    // Update is called once per frame
    protected override void Update()
    {
        // Handle basic enemy behavior from base class
        base.Update();
        
        // Don't do anything if we're dead
        if (dead) return;

        // Check if it's time to summon
        if (Time.time >= nextSummonTime && activeMinions.Count < maxMinions)
        {
            StartCoroutine(SummonMinion());
            SetNextSummonTime();
        }

        // Clean up the active minions list
        CleanupMinionsList();
    }
    
    private void CleanupMinionsList()
    {
        // Remove any null references (destroyed minions)
        for (int i = activeMinions.Count - 1; i >= 0; i--)
        {
            if (activeMinions[i] == null)
            {
                activeMinions.RemoveAt(i);
            }
        }
    }
    
    // Coroutine for summoning a minion with visual effects
    private IEnumerator SummonMinion()
    {
        if (summonSpot == null)
        {
            Debug.LogError("NecromancerController: Summon spot is not assigned!");
            yield break;
        }

        // Create summoning visual effect
        if (summoning != null)
        {
            GameObject summoningEffect = Instantiate(summoning, summonSpot.position, Quaternion.identity);
            Destroy(summoningEffect, 2f); // Destroy after 2 seconds
        }

        // Wait for summoning animation
        yield return new WaitForSeconds(1.5f);

        // Get the minion to spawn
        GameObject minionToSpawn = minionPrefab != null ? minionPrefab : skeleton;
        
        if (minionToSpawn == null)
        {
            Debug.LogError("NecromancerController: No minion prefab assigned!");
            yield break;
        }

        // Try to use EnemySpawner for more consistent enemy instantiation
        if (enemySpawner != null)
        {
            // Use SpawnSingleEnemy method instead since SpawnEnemyAtPosition doesn't exist
            GameObject minion = enemySpawner.SpawnSingleEnemy("skeleton", 0, 50, 5, 3, "random");
            
            // Move the spawned minion to the summon spot
            if (minion != null)
            {
                minion.transform.position = summonSpot.position;
                activeMinions.Add(minion);
                
                // Apply any special setup for the minion
                ConfigureMinion(minion);
                
                Debug.Log("Necromancer summoned a minion using EnemySpawner");
            }
        }
        // Fallback to direct instantiation if EnemySpawner is not available
        else if (GameManager.Instance != null)
        {
            GameObject minion = Instantiate(minionToSpawn, summonSpot.position, Quaternion.identity);
            GameManager.Instance.AddEnemy(minion);
            activeMinions.Add(minion);
            
            // Apply any special setup for the minion
            ConfigureMinion(minion);
            
            Debug.Log("Necromancer summoned a minion using direct instantiation");
        }
        else
        {
            Debug.LogError("NecromancerController: Cannot summon minion - both EnemySpawner and GameManager are null");
        }
    }
    
    private void ConfigureMinion(GameObject minion)
    {
        // You can add special properties to the summoned minions here
        // For example, making them weaker, giving them special abilities, etc.
        
        EnemyController controller = minion.GetComponent<EnemyController>();
        if (controller != null)
        {
            // Make summoned minions slightly weaker
            if (controller.hp != null)
            {
                controller.hp.max_hp = Mathf.RoundToInt(controller.hp.max_hp * 0.8f);
                controller.hp.hp = controller.hp.max_hp;
            }
            
            // Slightly slower
            controller.speed = Mathf.RoundToInt(controller.speed * 0.9f);
        }
        
        // You could add a visual indicator that this is a summoned minion
        // e.g., change color, add a particle effect, etc.
    }
    
    // Called by minions when they die
    public void MinionDied()
    {
        currentMinions = Mathf.Max(0, currentMinions - 1);
    }
    
    // Override the Die method to handle minion cleanup
    protected override void Die()
    {
        if (!dead)
        {
            // Play death particles if available
            if (deathParticles != null)
            {
                GameObject particles = Instantiate(deathParticles, transform.position, Quaternion.identity);
                Destroy(particles, 2f); // Destroy after 2 seconds
            }
            
            // Kill all active minions when the necromancer dies
            foreach (GameObject minion in activeMinions)
            {
                if (minion != null)
                {
                    // Apply death effect to minions
                    EnemyController controller = minion.GetComponent<EnemyController>();
                    if (controller != null && controller.hp != null)
                    {
                        // Insta-kill the minion with DARK damage type instead of PURE
                        controller.hp.Damage(new Damage(controller.hp.max_hp, Damage.Type.DARK));
                    }
                }
            }
            
            // Clear the minions list
            activeMinions.Clear();
            
            // Call the base implementation to handle the standard death functionality
            base.Die();
        }
    }
    
    // Create a death effect when the necromancer is defeated
    private void SpawnDeathEffect()
    {
        // Create a simple particle effect for now
        for (int i = 0; i < 20; i++)
        {
            // Create a small particle
            GameObject particle = new GameObject("NecroParticle");
            particle.transform.position = transform.position;
            
            // Add sprite renderer
            SpriteRenderer renderer = particle.AddComponent<SpriteRenderer>();
            
            // Use a simple circle sprite or whatever is available
            if (GameManager.Instance != null && GameManager.Instance.spellIconManager != null)
            {
                renderer.sprite = GameManager.Instance.spellIconManager.Get(0);
            }
            
            // Set color and scale
            renderer.color = tintColor;
            particle.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            
            // Add a script to handle particle movement and lifetime
            DeathParticle particleScript = particle.AddComponent<DeathParticle>();
            particleScript.InitializeWithRandomDirection(2f + Random.value * 3f);
        }
    }
} 