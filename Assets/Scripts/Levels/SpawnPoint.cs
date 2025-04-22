using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public enum SpawnName
    {
        RED, GREEN, BONE
    }

    public SpawnName kind;
    
    // Type string used for filtering - set in inspector based on kind
    [Tooltip("Used for selecting spawn points by type in level data (red, green, bone)")]
    public string type;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // If type is not manually set, derive it from the enum
        if (string.IsNullOrEmpty(type))
        {
            switch (kind)
            {
                case SpawnName.RED:
                    type = "red";
                    break;
                case SpawnName.GREEN:
                    type = "green";
                    break;
                case SpawnName.BONE:
                    type = "bone";
                    break;
                default:
                    type = "";
                    break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
