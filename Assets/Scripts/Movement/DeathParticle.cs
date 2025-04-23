using UnityEngine;

/**
 * DeathParticle - Handles the visual effects for particles emitted when an entity dies
 * 
 * Purpose:
 * - Creates visual feedback when enemies (especially Necromancers) are destroyed
 * - Controls movement, fading, and lifetime of death effect particles
 * 
 * Key Features:
 * - Particles move in random directions away from death location
 * - Particles gradually slow down, fade out, and shrink over time
 * - Self-destructs after a predefined lifetime
 * - Can be customized with different speeds for varied effects
 */
public class DeathParticle : MonoBehaviour
{
    // Movement properties
    private Vector3 direction;  // Direction the particle moves in
    private float speed;        // How fast the particle moves
    private float lifetime = 1.5f;  // How long the particle survives before being destroyed
    
    /**
     * InitializeWithRandomDirection - Sets up the particle with a random movement direction
     * Called when the particle is created to give it unique movement
     * 
     * @param newSpeed - Initial speed for the particle
     */
    public void InitializeWithRandomDirection(float newSpeed)
    {
        // Set random direction
        float angle = Random.Range(0, Mathf.PI * 2);
        direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
        speed = newSpeed;
        
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }
    
    /**
     * Update - Called every frame to update the particle's appearance and position
     * Handles movement, fading, and size reduction
     */
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