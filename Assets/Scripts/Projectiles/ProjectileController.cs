using UnityEngine;
using System;
using System.Collections;

public class ProjectileController : MonoBehaviour
{
    public float lifetime;
    public event Action<Hittable,Vector3> OnHit;
    public ProjectileMovement movement;
    private bool isDestroying = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (movement == null)
        {
            Debug.LogWarning("ProjectileController initialized without movement component. Destroying projectile.");
            DestroyProjectile();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isDestroying) return;
        
        try
        {
            // Check if GameManager exists and if we're in game over state
            if (GameManager.Instance != null && GameManager.Instance.state == GameManager.GameState.GAMEOVER)
            {
                DestroyProjectile();
                return;
            }
            
            // Handle movement
            if (movement != null)
            {
                movement.Movement(transform);
            }
            else
            {
                DestroyProjectile();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in ProjectileController.Update: {e.Message}");
            DestroyProjectile();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDestroying) return;
        
        try
        {
            if (collision == null || collision.gameObject == null) return;
            
            if (collision.gameObject.CompareTag("projectile")) return;
            
            if (collision.gameObject.CompareTag("unit"))
            {
                var ec = collision.gameObject.GetComponent<EnemyController>();
                if (ec != null && ec.hp != null && OnHit != null)
                {
                    OnHit(ec.hp, transform.position);
                }
                else
                {
                    var pc = collision.gameObject.GetComponent<PlayerController>();
                    if (pc != null && pc.hp != null && OnHit != null)
                    {
                        OnHit(pc.hp, transform.position);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in ProjectileController.OnCollisionEnter2D: {e.Message}");
        }
        finally
        {
            DestroyProjectile();
        }
    }

    public void SetLifetime(float lifetime)
    {
        if (lifetime > 0)
        {
            StartCoroutine(Expire(lifetime));
        }
    }

    private void DestroyProjectile()
    {
        if (!isDestroying)
        {
            isDestroying = true;
            Destroy(gameObject);
        }
    }

    IEnumerator Expire(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        DestroyProjectile();
    }
}
