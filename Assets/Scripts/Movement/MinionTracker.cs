using UnityEngine;

/**
 * MinionTracker - Tracks the lifecycle of minions summoned by a Necromancer
 * 
 * Purpose:
 * - Notifies the Necromancer when a minion is destroyed
 * - Acts as a lifecycle connection between minions and their master
 * 
 * Key Features:
 * - Automatically informs the Necromancer when this minion is destroyed
 * - Simple component that is attached to all summoned minions
 * - Helps the Necromancer keep track of active minion count
 */
public class MinionTracker : MonoBehaviour
{
    // Reference to the master necromancer that summoned this minion
    public NecromancerController master;
    
    /**
     * OnDestroy - Called when the minion is destroyed
     * Notifies the master necromancer that this minion has been eliminated
     */
    private void OnDestroy()
    {
        // Check if the master still exists before notifying
        if (master != null)
        {
            // Tell the master that this minion has died
            master.MinionDied();
        }
    }
} 