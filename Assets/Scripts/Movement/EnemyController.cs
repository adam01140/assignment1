using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public Transform target;
    public int speed;
    public int damage = 5; // Default damage value
    public Hittable hp;
    public HealthBar healthui;
    public bool dead;

    public float last_attack;
    
    // Add stats tracking
    public static System.Action OnEnemyDefeated;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null");
            return;
        }
        
        if (GameManager.Instance.player == null)
        {
            Debug.LogError("GameManager.Instance.player is null");
            return;
        }
        
        target = GameManager.Instance.player.transform;
        
        if (hp == null)
        {
            Debug.LogError("Enemy hp is null in Start() - enemy was not properly initialized");
            return;
        }
        
        hp.OnDeath += Die;
        
        if (healthui == null)
        {
            Debug.LogWarning("Enemy healthui is null");
        }
        else
        {
            healthui.SetHealth(hp);
        }
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            if (target == null)
            {
                // Try to recover the target
                if (GameManager.Instance != null && GameManager.Instance.player != null)
                {
                    target = GameManager.Instance.player.transform;
                    Debug.Log("Enemy recovered target reference");
                }
                else
                {
                    return; // Cannot proceed without target
                }
            }
            
            Vector3 direction = target.position - transform.position;
            if (direction.magnitude < 2f)
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
        catch (System.Exception e)
        {
            Debug.LogError($"Exception in EnemyController.Update: {e.Message}\n{e.StackTrace}");
        }
    }
    
    void DoAttack()
    {
        try
        {
            if (last_attack + 2 < Time.time)
            {
                last_attack = Time.time;
                
                Debug.Log("Enemy attempting to attack");
                
                // Comprehensive null checks
                if (GameManager.Instance == null)
                {
                    Debug.LogError("GameManager.Instance is null in DoAttack");
                    return;
                }
                
                if (GameManager.Instance.player == null)
                {
                    Debug.LogError("GameManager.Instance.player is null in DoAttack");
                    return;
                }
                
                if (target == null)
                {
                    Debug.LogError("target is null in DoAttack");
                    target = GameManager.Instance.player.transform;
                    if (target == null) return;
                }
                
                if (target.gameObject == null)
                {
                    Debug.LogError("target.gameObject is null in DoAttack");
                    return;
                }
                
                PlayerController playerController = target.gameObject.GetComponent<PlayerController>();
                if (playerController == null)
                {
                    Debug.LogError("playerController is null in DoAttack");
                    return;
                }
                
                if (playerController.hp == null)
                {
                    Debug.LogError("playerController.hp is null in DoAttack");
                    return;
                }
                
                // Now that we've verified everything, deal damage
                playerController.hp.Damage(new Damage(damage, Damage.Type.PHYSICAL));
                Debug.Log("Enemy attack successful");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Exception in DoAttack: {e.Message}\n{e.StackTrace}");
        }
    }


    void Die()
    {
        if (!dead)
        {
            dead = true;
            
            // Call the global event for enemy defeat
            OnEnemyDefeated?.Invoke();
            
            GameManager.Instance.RemoveEnemy(gameObject);
            Destroy(gameObject);
        }
    }
}
