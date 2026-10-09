using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public string firstLevelName = "MapScene"; // Nazwa pierwszej sceny gry
    public PlayerData playerData; // Przypisz tutaj ScriptableObject gracza

    // Dane startowe (do resetu przy Nowej Grze)
    [Header("Wartoœci domyœlne dla Nowej Gry")]
    public int startLevel = 1;
    public int startMaxHP = 20;
    public int startDamage = 5;
    public int startDefense = 2;

    [Header("Panele UI")]
    public GameObject mainButtonsPanel;
    public GameObject authorsPanel;
    public GameObject optionsPanel;

    [Header("Opcje Audio")]
    public Slider musicSlider; 
    public Slider sfxSlider;

    [Header("Referencje")]
    public Toggle propagandaToggle;

    public void NewGame()
    {
        // 1. Reset statystyk (Bez zmian)
        playerData.team.Clear(); // WA¯NE: Pusta dru¿yna na start
        playerData.inventory.Clear();
        playerData.collectedItemIDs.Clear();
        playerData.currentGold = 0;
        playerData.currentEquippedItem = null;
        playerData.moveSpeed = 5f;

        // 2. Reset managerów (Bez zmian)
        BattleTransitionManager.defeatedEnemies.Clear();
        BattleTransitionManager.isReturningFromBattle = false; // To wa¿ne! Nie wracamy z walki.


        BattleTransitionManager.targetSpawnPosition = new Vector3(0f, 0f, 0f);

        // W³¹czamy flagê: "Hej, po za³adowaniu mapy przesuñ gracza na pozycjê targetSpawnPosition"
        BattleTransitionManager.spawnPositionActive = true;
        // ----------------------------------------

        // 3. £aduj mapê
        SceneManager.LoadScene(firstLevelName);
    }

    public void LoadGame()
    {

        SaveManager manager = FindFirstObjectByType<SaveManager>();
        if (manager != null)
        {
            manager.LoadGame();
        }
        else
        {
            Debug.LogError("Nie znaleziono SaveManagera! Upewnij siê, ¿e obiekt Essentials jest na scenie.");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Wychodzenie z gry...");
        Application.Quit();
    }

    public void OpenAuthors()
    {
        mainButtonsPanel.SetActive(false);
        authorsPanel.SetActive(true);
    }

    public void OpenOptions()
    {
        mainButtonsPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void BackToMenu()
    {
        authorsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        mainButtonsPanel.SetActive(true);
    }

    void Start()
    {
        BackToMenu();
        // 1. Sprawdzamy czy Manager w ogóle istnieje (bezpiecznik)
        if (AudioManager.instance != null)
        {
            // --- OBS£UGA MUZYKI ---
            if (musicSlider != null)
            {
                // ZMIANA: Pobieramy zapamiêtan¹ wartoœæ z Managera
                musicSlider.value = AudioManager.instance.GetMusicVolume();

                // Podpinamy nas³uchiwanie zmian
                musicSlider.onValueChanged.AddListener((val) =>
                {
                    AudioManager.instance.SetMusicVolume(val);
                });
            }

            // --- OBS£UGA EFEKTÓW ---
            if (sfxSlider != null)
            {
                // ZMIANA: Pobieramy zapamiêtan¹ wartoœæ z Managera
                sfxSlider.value = AudioManager.instance.GetSFXVolume();

                // Podpinamy nas³uchiwanie zmian
                sfxSlider.onValueChanged.AddListener((val) =>
                {
                    AudioManager.instance.SetSFXVolume(val);
                });
            }
        }
        bool isPropagandaActive = PlayerPrefs.GetInt("SocialCreditOption", 0) == 1;

        // Aktualizujemy PlayerData
        playerData.allowChinesePropaganda = isPropagandaActive;

        // Aktualizujemy wizualnie Toggle (¿eby ptaszek siê zgadza³)
        if (propagandaToggle != null)
        {
            propagandaToggle.isOn = isPropagandaActive;
        }
    }
    public void SetPropaganda(bool isSwitched)
    {
        // 1. Zmieniamy wartoœæ w danych gry
        playerData.allowChinesePropaganda = isSwitched;

        // 2. Zapisujemy to na dysku (¿eby po restarcie gry pamiêta³o)
        // PlayerPrefs nie obs³uguje bool, wiêc zamieniamy na int (1 = prawda, 0 = fa³sz)
        PlayerPrefs.SetInt("SocialCreditOption", isSwitched ? 1 : 0);
        PlayerPrefs.Save();

        // Opcjonalnie: Œmieszny log
        if (isSwitched) Debug.Log("Obywatel podj¹³ s³uszn¹ decyzjê.");
    }
}