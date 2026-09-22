using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR
using UnityEditor; // Potrzebne, ¿eby Unity zapamiêta³o zmiany w edytorze
#endif

public enum InteractionType { Sign, Item, NPC, Shop, Trainer }

public class Interactable : MonoBehaviour
{
    [Header("Typ Interakcji")]
    public InteractionType type;

    [Header("Unikalne ID (Generuje siê samo)")]
    public string itemID; // <-- NOWOŒÆ: Do zapisu stanu zebrania

    [Header("Opis (Dla UI)")]
    public string objectName;

    [Header("Dla Znaków / NPC")]
    [TextArea] public string messageContent;

    [Header("Dla Przedmiotów")]
    public ItemData itemToGive;
    public PlayerData playerData;
    public AudioClip interactSound;

    [Header("Dla Sklepu")]
    public List<ItemData> shopInventory;

    [Header("Dla Trenerów")]
    public EnemyData enemy;
    public string battleSceneName = "BattleScene";

    private bool isPlayerInRange = false;
    private bool justInteracted = false;

    // --- NOWOŒÆ: Sprawdzanie przy starcie ---
    void Start()
    {
        // Jeœli to przedmiot i ju¿ go zebraliœmy -> zniszcz go od razu
        if (type == InteractionType.Item && playerData != null)
        {
            if (playerData.CheckIfCollected(itemID))
            {
                Destroy(gameObject);
            }
        }
    }

    void Update()
    {
        if (isPlayerInRange && !justInteracted && Input.GetKeyDown(KeyCode.E))
        {
            PerformAction();
        }
    }

    void PerformAction()
    {
        // 1. DŸwiêk
        if (interactSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(interactSound);
        }

        // 2. Logika
        switch (type)
        {
            case InteractionType.Sign:
                GameNotifications.instance.ShowMessage(messageContent, 4f);
                break;

            case InteractionType.Item:
                PickUpItem();
                break;

            case InteractionType.NPC:
                // Tu mo¿esz dodaæ logikê dialogu
                break;
            case InteractionType.Shop:
                if (ShopMenu.instance != null)
                {
                    ShopMenu.instance.OpenShop(shopInventory);
                }
                break;
            case InteractionType.Trainer:
                StartSpecificBattle();
                break;
        }
    }

    void PickUpItem()
    {
        if (itemToGive != null && playerData != null)
        {
            justInteracted = true;

            // Dodaj do plecaka
            playerData.AddItem(itemToGive);

            // --- NOWOŒÆ: Oznaczamy jako zebrane w PlayerData ---
            playerData.MarkAsCollected(itemID);

            // Wyœwietl komunikat
            GameNotifications.instance.ShowMessage($"Podniesiono: {itemToGive.itemName}", 3f);

            Destroy(gameObject);
        }
    }

    // --- FIZYKA ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            if (type == InteractionType.Sign)
            {
                PerformAction();
            }
            else
            {
                string prompt = "";

                switch (type)
                {
                    case InteractionType.Item:
                        // U¿ywamy objectName, który automatycznie pobra³ siê z ItemData
                        prompt = $"[E] Podnieœ: {objectName}";
                        break;
                    case InteractionType.NPC:
                        prompt = $"[E] Rozmawiaj: {objectName}";
                        break;
                    case InteractionType.Shop:
                        prompt = $"[E] Handluj: {objectName}";
                        break;
                    case InteractionType.Trainer:
                        prompt = $"[E] Walcz z {objectName}";
                        break;
                }

                if (InteractionUI.instance != null && !justInteracted)
                    InteractionUI.instance.ShowPrompt(prompt);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

            if (InteractionUI.instance != null && !justInteracted)
            {
                InteractionUI.instance.HidePrompt();
            }
        }
    }

    // --- MAGIA AUTOMATYZACJI (Dzia³a w Edytorze) ---

    private void OnValidate()
    {
        // 1. Automatyczne ID (jeœli puste)
        if (string.IsNullOrEmpty(itemID))
        {
            itemID = System.Guid.NewGuid().ToString();
            MarkDirty();
        }

        // 2. Automatyczna nazwa z ItemData
        // Jeœli mamy przypisany przedmiot, a nazwa w UI jest inna ni¿ nazwa przedmiotu -> Zaktualizuj
        if (type == InteractionType.Item && itemToGive != null)
        {
            if (objectName != itemToGive.itemName)
            {
                objectName = itemToGive.itemName;
                MarkDirty();
            }
        }
    }

    // Funkcja pomocnicza do zapisu zmian w edytorze
    private void MarkDirty()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }

    [ContextMenu("Wygeneruj Nowe ID")]
    private void GenerateNewID()
    {
        itemID = System.Guid.NewGuid().ToString();
        MarkDirty();
    }

    public void StartSpecificBattle()
    {
        // 1. ZABEZPIECZENIE: SprawdŸ czy przypisa³eœ wroga
        if (enemy == null)
        {
            Debug.LogError("Nie przypisano EnemyData w inspektorze!");
            return;
        }

        // 2. PRZYGOTOWANIE DANYCH (Wype³niamy "Most")
        // Przekazujemy wroga, z którym chcemy walczyæ
        BattleTransitionManager.enemyToFight = enemy;

        // Zapamiêtujemy pozycjê gracza, ¿eby wróci³ w to samo miejsce
        GameObject player = GameObject.FindGameObjectWithTag("Player"); // Upewnij siê, ¿e Gracz ma tag "Player"
        if (player != null)
        {
            BattleTransitionManager.lastPlayerPosition = player.transform.position;
        }

        // Ustawiamy flagi, ¿e wracamy z walki (¿eby nie zresetowa³o pozycji)
        BattleTransitionManager.isReturningFromBattle = true;
        // Opcjonalnie: Jeœli uciekniemy od Bossa, to mo¿e nie chcemy go odsuwaæ jak zwyk³ego stworka?
        // Ale na razie zostawmy standardowo:
        BattleTransitionManager.justRanAway = false;

        // 3. ZA£ADOWANIE SCENY
        SceneManager.LoadScene(battleSceneName);
    }
}