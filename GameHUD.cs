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
        // Poziom
        levelText.text = "LVL " + playerData.unitLevel;

        // Pasek HP
        hpSlider.maxValue = playerData.maxHP;
        hpSlider.value = playerData.currentHP;
        hpText.text = $"{playerData.currentHP} / {playerData.maxHP}";

        // Pasek XP (Jeœli doda³eœ slider)
        if (xpSlider != null)
        {
            xpSlider.maxValue = playerData.xpToNextLevel;
            xpSlider.value = playerData.currentXP;
        }
        if (xpText != null)
        {
            // Wyœwietli np.: "45 / 100 XP"
            xpText.text = $"{playerData.currentXP} / {playerData.xpToNextLevel} XP";
        }
    }
    // --- DODAJ TO DO GameHUD.cs (poza funkcj¹ Update) ---

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