using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Potrzebne do Coroutine

public class PlayerPositionHandler : MonoBehaviour
{
    void Start()
    {
    }


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Zamiast ustawiaæ od razu, uruchamiamy opóŸnienie
        StartCoroutine(TeleportAfterFrame());
    }

    IEnumerator TeleportAfterFrame()
    {
        yield return null; // Czekamy klatkê

        int playerCount = FindObjectsOfType<PlayerPositionHandler>().Length;

        if (BattleTransitionManager.spawnPositionActive)
        {
            transform.position = BattleTransitionManager.targetSpawnPosition;
            BattleTransitionManager.spawnPositionActive = false;
        }
    }
}