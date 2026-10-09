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

    void Awake()
    {
        if (instance == null) instance = this;
        panel.SetActive(false); // Ukrywamy na start
    }

    // Ta funkcja jest wywo³ywana przez jajko po wciœniêciu 'E'
    public void OpenPanel(EnemyData starterData, string description)
    {
        currentStarterData = starterData;
        pokeImage.sprite = starterData.battleSprite;
        descriptionText.text = description;

        panel.SetActive(true);
        Time.timeScale = 0f; // Zatrzymujemy grê, ¿eby gracz nie chodzi³ z otwartym oknem
    }

    // Funkcja podpinana pod przycisk "TAK"
    public void ChooseYes()
    {
        // 1. Tworzymy nowego Pokezaura z danych i dodajemy do dru¿yny gracza
        Pokezaur newPoke = new Pokezaur(currentStarterData);
        playerData.team.Add(newPoke);

        // 2. Wyœwietlamy powiadomienie
        if (GameNotifications.instance != null)
        {
            GameNotifications.instance.ShowMessage($"Wybra³eœ {newPoke.pokeName}!", 5f);
        }

        // 3. Usuwamy wszystkie jajka ze sceny, ¿eby nie wzi¹æ reszty
        GameObject[] eggs = GameObject.FindGameObjectsWithTag("StarterEgg");
        foreach (GameObject egg in eggs)
        {
            Destroy(egg);
        }

        // 4. Zamykamy panel i wznawiamy czas
        ClosePanel();
    }

    // Funkcja podpinana pod przycisk "NIE"
    public void ChooseNo()
    {
        ClosePanel();
    }

    private void ClosePanel()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
    }
}