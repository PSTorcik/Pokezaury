using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StarterSelectionUI : MonoBehaviour
{
    public static StarterSelectionUI instance;

    [Header("Elementy UI")]
    public GameObject panel;
    public Image pokeImage;
    public TextMeshProUGUI descriptionText;

    [Header("Dane")]
    public PlayerData playerData;

    private EnemyData currentStarterData;

    // Zmienna przechowuj¹ca skrypt ruchu gracza, by móc go wy³¹czyæ
    private PlayerMovement playerMovementRef;

    void Awake()
    {
        // Klasyczny Singleton – upewnia siê, ¿e to okno jest tylko jedno
        if (instance == null) instance = this;
        panel.SetActive(false);
    }

    void Update()
    {
        // Jeœli okno jest widoczne, zamykamy je TYLKO klawiszem Escape (lub przyciskiem na ekranie)
        if (panel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ChooseNo();
            }
        }
    }

    public void OpenPanel(EnemyData starterData, string description)
    {
        currentStarterData = starterData;
        pokeImage.sprite = starterData.battleSprite;
        descriptionText.text = description;

        panel.SetActive(true);

        // Znajdujemy gracza po tagu i unieruchamiamy go.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovementRef = player.GetComponent<PlayerMovement>();
            if (playerMovementRef != null)
            {
                // To fizycznie dezaktywuje skrypt. Gracz "zamarza".
                playerMovementRef.enabled = false;

                // Na wypadek, gdyby gracz by³ akurat w trakcie chodzenia 
                // i zamar³by w pozycji biegu, zatrzymujemy go.
                Animator anim = player.GetComponent<Animator>();
                if (anim != null) anim.SetFloat("Speed", 0);

                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
        }
    }

    public void ChooseYes()
    {
        // Dodanie Pokezaura do dru¿yny (struktura listowa).
        Pokezaur newPoke = new Pokezaur(currentStarterData);
        playerData.team.Add(newPoke);

        // Opcjonalne powiadomienie.
        if (GameNotifications.instance != null)
        {
            GameNotifications.instance.ShowMessage($"Wybra³eœ {newPoke.pokeName}!", 5f);
        }

        // Sprz¹tanie po stole – usuwamy wszystkie jajka oznaczone nowym tagiem.
        GameObject[] eggs = GameObject.FindGameObjectsWithTag("StarterEgg");
        foreach (GameObject egg in eggs)
        {
            Destroy(egg);
        }

        // Usuniêcie blokady ucieczki, jeœli taka jest przed drzwiami
        GameObject doorBlocker = GameObject.Find("DoorBlocker"); //lub odpowiednia nazwa
        if (doorBlocker != null)
        {
            doorBlocker.SetActive(false);
        }


        ClosePanel();
    }

    public void ChooseNo()
    {
        ClosePanel();
    }

    private void ClosePanel()
    {
        panel.SetActive(false);

        // Zwracamy graczowi mo¿liwoœæ ruchu!
        if (playerMovementRef != null)
        {
            playerMovementRef.enabled = true;
        }
    }
}