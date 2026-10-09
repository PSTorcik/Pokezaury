using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameHUD : MonoBehaviour
{
    [Header("ród³o Danych")]
    public PlayerData playerData; // Przeci¹gnij tu HeroData

    [Header("Elementy UI")]
    public GameObject hudContainer; // Ca³y panel (¿eby go ukrywaæ)

    public TextMeshProUGUI levelText;
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    [Header("Pasek XP")]
    public Slider xpSlider;
    public TextMeshProUGUI xpText; // 1. NOWE: Miejsce na tekst XP

    void Update()
    {
        // 1. Ukrywanie HUD w Menu i Walce
        string sceneName = SceneManager.GetActiveScene().name;

        // Jeœli jesteœmy w Menu lub w Walce -> Ukryj HUD
        if (sceneName == "MainMenu" || sceneName == "BattleScene")
        {
            if (hudContainer.activeSelf) hudContainer.SetActive(false);
            return; // Nie aktualizuj reszty, szkoda mocy
        }
        else
        {
            // Jesteœmy na mapie -> Poka¿ HUD
            if (!hudContainer.activeSelf) hudContainer.SetActive(true);
        }

        // 2. Aktualizacja Wartoœci (Live)
        UpdateStats();
    }

    void UpdateStats()
    {
        // POPRAWKA: Pobieramy dane z pierwszego Pokezaura w dru¿ynie, jeœli istnieje
        if (playerData != null && playerData.team != null && playerData.team.Count > 0)
        {
            Pokezaur leadPoke = playerData.team[0];

            if (levelText != null) levelText.text = "LVL " + leadPoke.level;

            if (hpSlider != null)
            {
                hpSlider.maxValue = leadPoke.maxHP;
                hpSlider.value = leadPoke.currentHP;
            }
            if (hpText != null) hpText.text = $"{leadPoke.currentHP} / {leadPoke.maxHP}";

            if (xpSlider != null)
            {
                xpSlider.maxValue = leadPoke.xpToNextLevel;
                xpSlider.value = leadPoke.currentXP;
            }
            if (xpText != null)
            {
                xpText.text = $"{leadPoke.currentXP} / {leadPoke.xpToNextLevel} XP";
            }
        }
        else
        {
            // Jeœli nie ma dru¿yny, czyœcimy HUD lub ustawiamy wartoœci domyœlne
            if (levelText != null) levelText.text = "LVL 0";
            if (hpSlider != null) { hpSlider.value = 0; hpSlider.maxValue = 1; }
            if (hpText != null) hpText.text = "0 / 0";
            if (xpSlider != null) { xpSlider.value = 0; xpSlider.maxValue = 1; }
            if (xpText != null) xpText.text = "0 / 0 XP";
        }
    }

    private void OnEnable()
    {
        // Nas³uchuj zmiany sceny
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Przestañ nas³uchiwaæ (¿eby nie by³o b³êdów przy wy³¹czaniu gry)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Ta funkcja odpali siê AUTOMATYCZNIE raz, od razu po wejœciu do nowej sceny
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sprawdzamy nazwê (upewnij siê co do pisowni!)
        if (scene.name == "MainMenu" || scene.name == "BattleScene")
        {
            if (hudContainer != null)
                hudContainer.SetActive(false); // WY£¥CZ GO NATYCHMIAST
        }
        else
        {
            if (hudContainer != null)
                hudContainer.SetActive(true); // W£¥CZ NA MAPIE
        }
    }
}