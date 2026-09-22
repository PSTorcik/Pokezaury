using UnityEngine;

public class HealerNPC : MonoBehaviour
{
    [Header("Konfiguracja NPC")]
    public string npcName = "Pani Doktor";

    [Header("Cennik i Dialogi")]
    public int healCost = 50; // Ile kosztuje leczenie

    [TextArea] public string[] successLines; // Dialog gdy zap³acimy i uleczy
    [TextArea] public string[] noMoneyLines; // Dialog gdy nie mamy kasy
    [TextArea] public string[] fullHpLines;  // Dialog gdy jesteœmy ju¿ zdrowi (opcjonalne)

    [Header("Dane")]
    public PlayerData playerStats;

    [Header("DŸwiêki")]
    public AudioClip healSound;
    public AudioClip errorSound; // DŸwiêk b³êdu (opcjonalnie)

    private bool isPlayerClose;

    void Update()
    {
        if (isPlayerClose && Input.GetKeyDown(KeyCode.E))
        {
            // 1. Jeœli dialog nie trwa -> Próbujemy uleczyæ
            if (!DialogueManager.instance.isDialogueActive)
            {
                TryToHeal(); // Zmieniamy nazwê funkcji na "Spróbuj Uleczyæ"
            }
            // 2. Jeœli trwa -> przewijaj tekst
            else
            {
                DialogueManager.instance.DisplayNextSentence();
            }
        }
    }

    void TryToHeal()
    {
        // KROK 1: SprawdŸ czy gracz w ogóle potrzebuje leczenia
        if (playerStats.currentHP >= playerStats.maxHP)
        {
            DialogueManager.instance.StartDialogue(npcName, fullHpLines);
            return; // Koñczymy funkcjê, nie zabieramy kasy
        }

        // KROK 2: Próba zakupu (SpendGold zwraca true jeœli siê uda, false jeœli nie)
        if (playerStats.SpendGold(healCost))
        {
            // --- SUKCES (Mamy kasê) ---

            // Logika leczenia
            playerStats.currentHP = playerStats.maxHP;

            // DŸwiêk
            if (healSound != null && AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX(healSound);
            }

            // Dialog sukcesu
            DialogueManager.instance.StartDialogue(npcName, successLines);
        }
        else
        {
            // --- PORA¯KA (Brak kasy) ---
            // Opcjonalny dŸwiêk b³êdu
            if (errorSound != null && AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX(errorSound);
            }

            // Dialog biedy
            DialogueManager.instance.StartDialogue(npcName, noMoneyLines);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerClose = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerClose = false;
    }
}