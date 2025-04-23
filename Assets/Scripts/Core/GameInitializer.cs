using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    private void Awake()
    {
        try {
            var enemies = EnemyData.Instance.GetAllEnemies();
        } catch (System.Exception ex) {
        }
        
        try {
            var levels = LevelData.Instance.GetAllLevels();
        } catch (System.Exception ex) {
        }
        
        try {
            EnemyManager.Instance.Start();
        } catch (System.Exception ex) {
        }
    }
} 