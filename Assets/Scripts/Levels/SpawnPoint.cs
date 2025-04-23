using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public enum SpawnName
    {
        RED, GREEN, BONE
    }

    public SpawnName kind;
    public string type;

    void Start()
    {
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
}
