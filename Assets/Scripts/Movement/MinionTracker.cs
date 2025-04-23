using UnityEngine;

// Helper class for tracking minions
public class MinionTracker : MonoBehaviour
{
    public NecromancerController master;
    
    private void OnDestroy()
    {
        if (master != null)
        {
            master.MinionDied();
        }
    }
} 