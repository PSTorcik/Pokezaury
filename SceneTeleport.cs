using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleport : MonoBehaviour
{
    [Header("Gdzie przenieœæ gracza?")]
    public string sceneToLoad;   // Nazwa sceny (np. "HospitalScene")
    public Vector2 spawnPoint;   // Koordynaty X, Y w œrodku budynku

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Zapisujemy, gdzie gracz ma wyl¹dowaæ
            BattleTransitionManager.targetSpawnPosition = spawnPoint;
            BattleTransitionManager.spawnPositionActive = true;

            // £adujemy scenê
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}