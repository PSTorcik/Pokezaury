// --- PLIK: PauseMenu.cs ---
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance;
    public GameObject pauseMenuUI; // Przypisz ca³y Panel menu pauzy
    public static bool GameIsPaused = false;

    void Update()
    {
        // 1. Blokada pauzy w menu g³ównym i podczas walki (Twoja logika)
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "BattleScene" || currentScene == "MainMenu") return;

        // 2. Obs³uga klawisza ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ShopMenu.instance != null && ShopMenu.instance.isShopOpen)
            {
                ShopMenu.instance.CloseShop();
                return; 
            }

            if (StatsMenu.instance != null && StatsMenu.instance.isMenuOpen)
            {
                StatsMenu.instance.CloseMenu();
                return;
            }

            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    private void Awake()
    {
        // Ustawiamy instancjê przy starcie
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // Zabezpieczenie przed dublowaniem
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Odmra¿amy czas
        GameIsPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Zamra¿amy czas (zatrzymuje ruch, fizykê)
        GameIsPaused = true;
    }

    public void SaveGameButton()
    {
        SaveManager.instance.SaveGame();
    }

    public void LoadGameButton()
    {
        Time.timeScale = 1f; // Wa¿ne: OdmroŸ czas przed wczytaniem!
        SaveManager.instance.LoadGame();
    }

    public void QuitToMain()
    {
        Resume();

        SceneManager.LoadScene("MainMenu");
    }
}