using UnityEngine;

public class EnemySave : MonoBehaviour
{
    public EnemyCore enemyCore;

    [Tooltip("ID prefabu z listy 'Enemy Prefabs' w EnemiesManagerze (np. 0 dla pierwszego typu wroga).")]
    public int prefabID = 0;

    private void Start() // U¿ywamy Start, by mieæ pewnoœæ ¿e GameManager i Manager s¹ gotowe
    {
        if (enemyCore == null)
            enemyCore = GetComponent<EnemyCore>();

        if (GameManager.Instance != null && GameManager.Instance.EnemiesManager != null)
        {
            GameManager.Instance.EnemiesManager.RegisterEnemy(this);
        }
    }

    private void OnDestroy()
    {
        // Kiedy obiekt ginie i jest usuwany ze sceny, automatycznie wypisuje siê z zapisu
        if (GameManager.Instance != null && GameManager.Instance.EnemiesManager != null)
        {
            GameManager.Instance.EnemiesManager.UnregisterEnemy(this);
        }
    }
}