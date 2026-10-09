using UnityEngine;
using TMPro; // Do tekstów
using UnityEngine.UI; // Do obs³ugi obrazków
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class StatsMenu : MonoBehaviour
{
    public static StatsMenu instance;
    [Header("G³ówne")]
    public GameObject menuPanel; // Ca³e okno statystyk
    public PlayerData playerData; // Sk¹d bierzemy dane 

    [Header("Statystyki (Teksty)")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI AttackText;
    public TextMeshProUGUI DefText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI speedText;

    [Header("Ekwipunek")]
    public GameObject inventoryContainer; // Obiekt "Content" w ScrollView
    public GameObject itemSlotPrefab;     // Nasz pasek przedmiotu (Prefab)
   

    public bool isMenuOpen = false;

    void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "MainMenu" || currentScene == "BattleScene")
        {
            return;
        }

        if (PauseMenu.GameIsPaused)
        {
            return;
        }
        if (ShopMenu.instance != null && ShopMenu.instance.isShopOpen)
        {

            if (Input.GetKeyDown(KeyCode.B))
            {
                ShopMenu.instance.CloseShop();
            }
            return; 
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (isMenuOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }

    }
    private void Awake()
    {
        // Ustawiamy instancjê przy starcie gry
        if (instance == null)
        {
            instance = this;
        }
    }

    void OpenMenu()
    {
        menuPanel.SetActive(true);
        isMenuOpen = true;

        // Zatrzymanie czasu (opcjonalne, ¿eby nikt nas nie bi³ jak czytamy)
        Time.timeScale = 0f;

        UpdateStats();
        UpdateInventory();
    }

    public void CloseMenu()
    {
        menuPanel.SetActive(false);
        isMenuOpen = false;

        // Wznawiamy czas
        Time.timeScale = 1f;
    }

    void UpdateStats()
    {
        goldText.text = $"{playerData.currentGold}";

        // Sprawdzamy czy mamy jakiegoœ stworka
        if (playerData.team.Count > 0)
        {
            Pokezaur lead = playerData.team[0];
            nameText.text = lead.pokeName;
            levelText.text = "Poziom: " + lead.level;
            hpText.text = $"HP: {lead.currentHP} / {lead.maxHP}";
            xpText.text = $"XP: {lead.currentXP} / {lead.xpToNextLevel}";
            AttackText.text = $"Si³a: {lead.damage}";
            DefText.text = $"Obrona: {lead.defense}";
        }
        else
        {
            nameText.text = "Brak Pokezaurów";
            levelText.text = ""; hpText.text = ""; xpText.text = "";
            AttackText.text = ""; DefText.text = "";
        }
    }

    void UpdateInventory()
    {
        // 1. Czyœcimy star¹ listê
        foreach (Transform child in inventoryContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // 2. Tworzymy nowe paski dla ka¿dego przedmiotu
        foreach (InventorySlot slot in playerData.inventory)
        {
            GameObject newSlot = Instantiate(itemSlotPrefab, inventoryContainer.transform);

            // Ustawiamy Teksty (0 = Nazwa, 1 = Iloœæ)
            TextMeshProUGUI[] texts = newSlot.GetComponentsInChildren<TextMeshProUGUI>();

            if (texts.Length >= 1)
            {
                // SPRAWDZANIE CZY TO ZA£O¯ONE EQ
                if (playerData.currentEquippedItem == slot.item)
                {
                    texts[0].text = slot.item.itemName + " [Za³o¿ony]"; // Dopisek [E]
                    texts[0].color = Color.green; // Kolor zielony
                }
                else
                {
                    texts[0].text = slot.item.itemName;
                    texts[0].color = Color.black; // Kolor normalny
                }
            }

            if (texts.Length >= 2) texts[1].text = "x" + slot.amount;

            // 3. OBS£UGA KLIKNIÊCIA (NAJWA¯NIEJSZE)
            Button btn = newSlot.GetComponent<Button>();
            if (btn != null)
            {
                // Dodajemy "s³uchacza". To magiczne "() =>" pozwala przekazaæ konkretny item do funkcji.
                btn.onClick.AddListener(() => OnItemClicked(slot.item));
            }
        }
    }
    void OnItemClicked(ItemData item)
    {
        playerData.UseItem(item);
        UpdateStats();
        UpdateInventory();
    }
}