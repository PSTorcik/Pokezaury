using UnityEngine;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI instance;

    [Header("Elementy UI")]
    public GameObject panel;    // Ca³y pasek/t³o
    public TextMeshProUGUI promptText; // Tekst np. "Podnieœ Miecz [E]"

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        // Na start ukrywamy
        if (panel != null) panel.SetActive(false);
    }

    public void ShowPrompt(string message)
    {
        panel.SetActive(true);
        promptText.text = message;
    }

    public void HidePrompt()
    {
        panel.SetActive(false);
    }
}