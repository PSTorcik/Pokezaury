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
        if (playerStats.team.Count == 0) return;

        // KROK 1: SprawdŸ czy jakikolwiek z 6 stworków wymaga leczenia
        bool needsHealing = false;
        foreach (Pokezaur poke in playerStats.team)
        {
            if (poke.currentHP < poke.maxHP) needsHealing = true;
        }

        if (!needsHealing)
        {
            DialogueManager.instance.StartDialogue(npcName, fullHpLines);
            return;
        }

        // KROK 2: Próba zakupu
        if (playerStats.SpendGold(healCost))
        {
            // --- SUKCES (Mamy kasê) ---
            // Leczymy CA£¥ DRU¯YNÊ
            foreach (Pokezaur poke in playerStats.team)
            {
                poke.currentHP = poke.maxHP;
            }

            if (healSound != null && AudioManager.instance != null) AudioManager.instance.PlaySFX(healSound);
            DialogueManager.instance.StartDialogue(npcName, successLines);
        }
        else
        {
            // --- PORA¯KA (Brak kasy) ---
            if (errorSound != null && AudioManager.instance != null) AudioManager.instance.PlaySFX(errorSound);
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