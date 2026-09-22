using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    public string npcName; // Imiê postaci (wpisz w Unity)

    [TextArea(3, 10)] // To sprawi, ¿e w Unity bêdziesz mia³ du¿e pole do pisania
    public string[] dialogueLines; // Lista zdañ tego NPC

    private bool isPlayerClose;
    private DialogueManager manager;

    void Start()
    {
        // Znajdujemy mened¿era na scenie automatycznie
        manager = Object.FindFirstObjectByType<DialogueManager>();
    }

    void Update()
    {
        if (isPlayerClose && Input.GetKeyDown(KeyCode.E))
        {
            // Tutaj te¿ u¿ywamy instance zamiast zmiennej 'manager'
            if (!DialogueManager.instance.isDialogueActive)
            {
                DialogueManager.instance.StartDialogue(npcName, dialogueLines);
            }
            else
            {
                DialogueManager.instance.DisplayNextSentence();
            }
        }
    }

    // Wykrywanie wejœcia gracza w strefê
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerClose = true;
        }
    }

    // Wykrywanie wyjœcia gracza
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerClose = false;
            // Opcjonalnie: Zamknij dialog, jak gracz odejdzie w trakcie rozmowy
            // manager.EndDialogue(); 
        }
    }
}