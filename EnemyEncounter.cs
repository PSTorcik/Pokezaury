using System.Collections; // WA¯NE: Dodane do obs³ugi Coroutine
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyEncounter : MonoBehaviour
{
    public EnemyData myData;
    public string uniqueID;

    // NOWE: Flaga bezpieczeñstwa
    private bool canEncounter = false;

    void Start()
    {
        // 1. Czy ten wróg ju¿ nie ¿yje?
        if (BattleTransitionManager.defeatedEnemies.Contains(uniqueID))
        {
            Destroy(gameObject);
            return; // Koñczymy, ¿eby nie odpalaæ reszty kodu
        }

        // 2. NOWOŒÆ: Nie atakuj od razu!
        // Uruchamiamy licznik, który w³¹czy "agresjê" dopiero po chwili.
        StartCoroutine(EnableEncounterAfterDelay());
    }

    // Ta funkcja czeka, zanim pozwoli na walkê
    IEnumerator EnableEncounterAfterDelay()
    {
        // Czekamy 3 sekundy. To czas dla gracza na zorientowanie siê i ruch.
        yield return new WaitForSeconds(2f);

        canEncounter = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // NOWE ZABEZPIECZENIE:
        // Jeœli wróg jeszcze siê "nie obudzi³" (minê³o mniej ni¿ 1.5s od za³adowania sceny), ignorujemy gracza.
        if (!canEncounter) return;

        if (other.CompareTag("Player"))
        {
            StartBattle(other.gameObject);
        }
    }

    void StartBattle(GameObject player)
    {
        BattleTransitionManager.enemyToFight = myData;
        BattleTransitionManager.lastPlayerPosition = player.transform.position;
        BattleTransitionManager.isReturningFromBattle = true;
        BattleTransitionManager.currentEnemyID = uniqueID;

        SceneManager.LoadScene("BattleScene");
    }
}