using UnityEngine;
using TMPro;
using System.Collections.Generic; // Potrzebne do Kolejki (Queue)

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
    public GameObject dialoguePanel; // Przypisz Panel
    public TextMeshProUGUI nameText; // Przypisz tekst imienia
    public TextMeshProUGUI dialogueText; // Przypisz tekst treœci

    // Kolejka przechowuj¹ca zdania do wyœwietlenia
    private Queue<string> sentences;

    // Flaga, ¿eby wiedzieæ, czy dialog trwa (np. ¿eby zablokowaæ ruch gracza)
    public bool isDialogueActive = false;

    private void Awake()
    {
        // Ustawiamy instancjê przy starcie gry
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // Jeœli jakimœ cudem s¹ dwa mened¿ery, niszczymy duplikat
        }
    }

    void Start()
    {
        sentences = new Queue<string>();
    }

    public void StartDialogue(string name, string[] lines)
    {
        isDialogueActive = true;
        dialoguePanel.SetActive(true); // W³¹czamy okienko
        nameText.text = name;

        sentences.Clear(); // Czyœcimy stare zdania

        // Dodajemy nowe zdania do kolejki
        foreach (string line in lines)
        {
            sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        // Jeœli skoñczy³y siê zdania w kolejce
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue(); // Pobieramy kolejne zdanie
        dialogueText.text = sentence;
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false); // Chowamy okienko
    }
}