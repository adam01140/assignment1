using UnityEngine;

// Simple particle script for death effect
public class DeathParticle : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float lifetime = 1.5f;
    
    public void InitializeWithRandomDirection(float newSpeed)
    {
        // Set random direction
        float angle = Random.Range(0, Mathf.PI * 2);
        direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
        speed = newSpeed;
        
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        // Move in the set direction
        transform.position += direction * speed * Time.deltaTime;
        
        // Slow down over time
        speed *= 0.95f;
        
        // Fade out
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color color = renderer.color;
            color.a = Mathf.Lerp(color.a, 0, Time.deltaTime * 2f);
            renderer.color = color;
        }
        
        // Shrink
        transform.localScale *= 0.99f;
    }
} 