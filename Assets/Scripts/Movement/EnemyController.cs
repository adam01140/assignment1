using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public Transform target;
    public int speed;
    public int damage = 5;
    public Hittable hp;
    public HealthBar healthui;
    public bool dead;

    public float last_attack;
    
    public static System.Action OnEnemyDefeated;
    
    private static bool hasLoggedGameManagerError = false;
    private static bool hasLoggedPlayerError = false;
    
    private bool isSceneChanging = false;
    
    private float retryPlayerFindTimer = 0f;
    private const float RETRY_PLAYER_FIND_INTERVAL = 1f;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddEnemy(gameObject);
        }
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        
        if (GameManager.Instance != null && !isSceneChanging)
        {
            GameManager.Instance.RemoveEnemy(gameObject);
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isSceneChanging = false;
        
        StartCoroutine(FindPlayerAfterDelay(0.5f));
    }
    
    private void OnSceneUnloaded(Scene scene)
    {
        isSceneChanging = true;
    }
    
    private IEnumerator FindPlayerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (target == null && GameManager.Instance != null)
        {
            GameObject playerObj = GameManager.Instance.TryGetPlayer();
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
        }
    }

    protected virtual void Start()
    {
        TryInitializeTarget();
        
        if (hp == null)
        {
            return;
        }
        
        hp.OnDeath += Die;
        
        if (healthui != null)
        {
            healthui.SetHealth(hp);
        }
    }
    
    private bool TryInitializeTarget()
    {
        if (target != null)
        {
            return true;
        }
        
        if (GameManager.Instance == null)
        {
            return false;
        }
        
        GameObject playerObj = GameManager.Instance.TryGetPlayer();
        if (playerObj == null)
        {
            return false;
        }
        
        target = playerObj.transform;
        return true;
    }

    protected virtual void Update()
    {
        if (isSceneChanging || 
            GameManager.Instance == null || 
            GameManager.Instance.state == GameManager.GameState.GAMEOVER)
        {
            return;
        }
        
        if (target == null)
        {
            retryPlayerFindTimer += Time.deltaTime;
            if (retryPlayerFindTimer >= RETRY_PLAYER_FIND_INTERVAL)
            {
                retryPlayerFindTimer = 0f;
                if (!TryInitializeTarget())
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }
        
        try
        {
            if (target == null)
            {
                return;
            }
            
            Vector3 direction = target.position - transform.position;
            float distanceToTarget = direction.magnitude;
            
            if (distanceToTarget < 2f)
            {
                DoAttack();
            }
            else
            {
                Unit unit = GetComponent<Unit>();
                if (unit != null)
                {
                    unit.movement = direction.normalized * speed;
                }
            }
        }
        catch (System.Exception ex)
        {
            target = null;
        }
    }
    
    void DoAttack()
    {
        if (isSceneChanging || GameManager.Instance.state == GameManager.GameState.GAMEOVER)
        {
            return;
        }
        
        if (target == null)
        {
            return;
        }
        
        if (Time.time < last_attack + 0.5f)
        {
            return;
        }
        
        last_attack = Time.time;
        
        if (GameManager.Instance == null)
        {
            return;
        }
        
        GameObject player = GameManager.Instance.TryGetPlayer();
        if (player == null)
        {
            hasLoggedGameManagerError = true;
            
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                GameManager.Instance.player = player;
            }
            else
            {
                return;
            }
        }
        
        if (player == null || player.gameObject == null)
        {
            return;
        }
        
        target = player.transform;
        
        if (target == null || target.gameObject == null)
        {
            target = null;
            return;
        }
        
        PlayerController pc = target.GetComponent<PlayerController>();
        if (pc == null)
        {
            return;
        }
        
        Hittable playerHP = pc.hp;
        if (playerHP == null)
        {
            if (pc.HasStarted())
            {
                playerHP = pc.GetComponent<Hittable>();
                if (playerHP == null)
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }
        
        playerHP.TakeDamage(damage);
    }
    
    protected virtual void Die()
    {
        if (dead)
        {
            return;
        }
        
        dead = true;
        
        if (OnEnemyDefeated != null)
        {
            OnEnemyDefeated();
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RemoveEnemy(gameObject);
        }
        
        Destroy(gameObject);
    }
}
