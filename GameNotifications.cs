using UnityEngine;
using TMPro;
using System.Collections;

public class GameNotifications : MonoBehaviour
{
    public static GameNotifications instance;

    [Header("UI Elements")]
    public GameObject messagePanel; // T³o komunikatu
    public TextMeshProUGUI messageText; // Sam tekst

    [Header("Ustawienia")]
    public float defaultDuration = 2f; // Ile czasu wisi komunikat

    private Coroutine currentCoroutine;

    void Awake()
    {
        // Robimy Singletona, ¿eby ³atwo siê odwo³ywaæ: GameNotifications.instance.Show(...)
        if (instance == null) instance = this;
        else Destroy(gameObject);

        // Na start ukrywamy panel
        if (messagePanel != null) messagePanel.SetActive(false);
    }

    // Funkcja do wywo³ywania komunikatów
    public void ShowMessage(string text, float duration = 2f)
    {
        if (messagePanel == null) return;

        messagePanel.SetActive(true);
        messageText.text = text;

        // Jeœli ju¿ coœ siê wyœwietla, przerywamy odliczanie i zaczynamy nowe
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(HideAfterTime(duration));
    }

    IEnumerator HideAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        messagePanel.SetActive(false);
    }
}