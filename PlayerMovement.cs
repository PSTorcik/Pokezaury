using UnityEngine;
using UnityEngine.SceneManagement; // Do wykrywania zmiany sceny
using UnityEngine.Tilemaps; // Do obs³ugi Tilemapy

public class PlayerMovement : MonoBehaviour
{
    [Header("Dane Gracza")]
    public PlayerData playerData;

    [Header("Ustawienia Ruchu")]
    public Rigidbody2D rb;
    public Animator anim;

    [Header("Ustawienia Walki")]
    [SerializeField] private Tilemap grassTilemap;
    public int encounterChance = 400;
    public EnemyData[] potentialEnemies;
    public string battleSceneName = "BattleScene";

    // --- NOWE ZMIENNE: System Kroków ---
    [Header("System Kroków")]
    public float stepLength = 1.5f; // Jak¹ odleg³oœæ trzeba przejœæ, ¿eby by³ "rzut kostk¹"
    private float currentStepTracker = 0f; // Licznik przebytego dystansu
    // -----------------------------------

    [Header("Audio")]
    public AudioClip stepSound;   // Tu przeci¹gniesz plik dŸwiêkowy
    public float stepRate = 0.4f; // Co ile sekund s³ychaæ krok (im mniej, tym szybciej tupie)
    private float nextStepTime = 0f; // Licznik wewnêtrzny

    private Vector2 movement;
    private bool isMoving;

    // --- NOWE: Automatyczne szukanie trawy po zmianie sceny ---
    private void OnEnable()
    {

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Ta funkcja odpala siê za ka¿dym razem, gdy wchodzisz na now¹ mapê
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. OCHRONA PRZED WALK¥: Jeœli to scena walki, gracz idzie spaæ
        if (scene.name == "BattleScene" || scene.name == "MainMenu") // Upewnij siê, ¿e nazwa sceny jest identyczna jak w Unity!
        {
            // Wy³¹czamy logikê, ¿eby nie psu³a flag
            GetComponent<SpriteRenderer>().enabled = false; // Zniknij
            if (rb != null) rb.simulated = false; // Wy³¹cz fizykê

            // WA¯NE: Przerywamy funkcjê (return). 
            // Dziêki temu kod poni¿ej siê NIE wykona w scenie walki.
            return;
        }
        else
        {
            // Jeœli to mapa - w³¹czamy gracza
            GetComponent<SpriteRenderer>().enabled = true;
            if (rb != null) rb.simulated = true;
        }

        // 2. LOGIKA MAPY (Wykonuje siê TYLKO jeœli to NIE jest BattleScene)
        GameObject grassObj = GameObject.Find("Enemy_Grass");
        if (grassObj != null)
        {
            grassTilemap = grassObj.GetComponent<Tilemap>();
        }

        if (BattleTransitionManager.isReturningFromBattle)
        {
            transform.position = BattleTransitionManager.lastPlayerPosition;

            // --- NOWOŒÆ: Przywracanie warstwy wizualnej ---
            if (BattleTransitionManager.isOnElevation)
            {
                GetComponent<SpriteRenderer>().sortingOrder = 10;
            }
            else
            {
                GetComponent<SpriteRenderer>().sortingOrder = 3; // lub Twoja domyœlna wartoœæ
            }
            // ----------------------------------------------

            // Logika ochrony
            if (BattleTransitionManager.justRanAway)
            {
                currentStepTracker = -6f;
            }

            // Resetujemy flagi DOPIERO PO powrocie na mapê
            BattleTransitionManager.justRanAway = false;
            BattleTransitionManager.isReturningFromBattle = false;
        }
    }


    void Update()
    {
        // 1. BLOKADA DIALOGU
        if (DialogueManager.instance != null && DialogueManager.instance.isDialogueActive)
        {
            rb.linearVelocity = Vector2.zero; // Zatrzymanie fizyki
            anim.SetFloat("Speed", 0);
            return; // Nic wiêcej nie robimy
        }

        // 2. ODCZYT KLAWISZY
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 3. SPRAWDZENIE CZY SIÊ RUSZAMY
        if (movement.sqrMagnitude > 0.01f)
        {
            isMoving = true;

            // Animacje
            anim.SetFloat("Horizontal", movement.x);
            anim.SetFloat("Vertical", movement.y);
            anim.SetFloat("Speed", 1);

            // --- A. LICZENIE KROKÓW DO WALKI (Z now¹ prêdkoœci¹!) ---
            // TU JEST ZMIANA: Mno¿ymy przez playerData.moveSpeed zamiast lokalnego moveSpeed
            currentStepTracker += playerData.moveSpeed * Time.deltaTime;

            if (currentStepTracker >= stepLength)
            {
                currentStepTracker = 0; // Reset licznika
                CheckForEncounters();   // SprawdŸ czy walka
            }

            // --- B. DWIÊK KROKÓW (TUPANIE) ---
            if (Time.time >= nextStepTime)
            {
                if (stepSound != null && AudioManager.instance != null)
                {
                    AudioManager.instance.PlaySFX(stepSound);

                    // Im szybciej biegniesz, tym czêœciej tupiesz? 
                    // Mo¿esz zostawiæ sta³e stepRate albo zrobiæ dynamiczne: stepRate / (playerData.moveSpeed / 5f)
                    nextStepTime = Time.time + stepRate;
                }
            }
        }
        else
        {
            // Jeœli stoimy w miejscu
            isMoving = false;
            anim.SetFloat("Speed", 0);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movement.normalized * playerData.moveSpeed;
    }

    void CheckForEncounters()
    {
        if (grassTilemap == null) return;

        Vector3Int gridPosition = grassTilemap.WorldToCell(transform.position);

        if (grassTilemap.HasTile(gridPosition))
        {
            int roll = Random.Range(0, 1001);

            if (roll <= encounterChance)
            {
                BattleTransitionManager.lastPlayerPosition = transform.position;
                BattleTransitionManager.isReturningFromBattle = true;

                if (potentialEnemies.Length > 0)
                {
                    int randomIndex = Random.Range(0, potentialEnemies.Length);
                    BattleTransitionManager.enemyToFight = potentialEnemies[randomIndex];

                    movement = Vector2.zero;
                    if (rb != null) rb.linearVelocity = Vector2.zero;
                    SceneManager.LoadScene(battleSceneName);
                }
            }
        }
    }
}