using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class StatsMenu : MonoBehaviour
{
    public static StatsMenu instance;

    [Header("G³ówne")]
    public GameObject menuPanel;
    public PlayerData playerData;

    [Header("Statystyki (Teksty)")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI AttackText;
    public TextMeshProUGUI DefText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI speedText;

    [Header("Podgl¹d Dru¿yny (Nowe)")]
    public Image pokeImage;                  // Obrazek aktualnie wybranego Pokezaura
    public TextMeshProUGUI slotIndexText;    // Np. "Pokezaur 1 / 3"
    public Button setLeaderButton;           // Przycisk "Ustaw jako lidera"

    [Header("Ekwipunek")]
    public GameObject inventoryContainer;
    public GameObject itemSlotPrefab;

    public bool isMenuOpen = false;
    private int selectedPokeIndex = 0; // Którego stworka z dru¿yny aktualnie ogl¹damy (0 - 5)

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
        if (instance == null)
        {
            instance = this;
        }
    }

    void OpenMenu()
    {
        menuPanel.SetActive(true);
        isMenuOpen = true;
        selectedPokeIndex = 0; // Po otwarciu domyœlnie pokazujemy lidera (slot 0)

        Time.timeScale = 0f;

        UpdateStats();
        UpdateInventory();
    }

    public void CloseMenu()
    {
        menuPanel.SetActive(false);
        isMenuOpen = false;

        Time.timeScale = 1f;
    }

    // Podepnij pod przycisk ">" (Nastêpny Pokezaur) w oknie StatsMenu
    public void NextPokezaur()
    {
        if (playerData.team.Count <= 1) return;
        selectedPokeIndex = (selectedPokeIndex + 1) % playerData.team.Count;
        UpdateStats();
    }

    // Podepnij pod przycisk "<" (Poprzedni Pokezaur) w oknie StatsMenu
    public void PreviousPokezaur()
    {
        if (playerData.team.Count <= 1) return;
        selectedPokeIndex--;
        if (selectedPokeIndex < 0) selectedPokeIndex = playerData.team.Count - 1;
        UpdateStats();
    }

    // Podepnij pod przycisk "Ustaw jako lidera" w oknie StatsMenu
    public void SetSelectedAsLeader()
    {
        if (selectedPokeIndex <= 0 || selectedPokeIndex >= playerData.team.Count) return;

        // Bezpieczna zamiana lidera (przek³ada te¿ za³o¿ony ekwipunek na nowego lidera)
        playerData.SwapLeader(selectedPokeIndex);

        selectedPokeIndex = 0; // Teraz nasz wybrany stworek jest na pozycji 0
        UpdateStats();
    }

    void UpdateStats()
    {
        if (goldText != null) goldText.text = $"{playerData.currentGold}";
        if (speedText != null) speedText.text = $"Szybkoœæ: {playerData.moveSpeed}";

        if (playerData.team.Count > 0)
        {
            // Zabezpieczenie zakresu indeksu
            if (selectedPokeIndex >= playerData.team.Count)
                selectedPokeIndex = 0;

            Pokezaur current = playerData.team[selectedPokeIndex];
            string leaderTag = (selectedPokeIndex == 0) ? " [LIDER]" : "";

            nameText.text = current.pokeName + leaderTag;
            levelText.text = "Poziom: " + current.level;
            hpText.text = $"HP: {current.currentHP} / {current.maxHP}";
            xpText.text = $"XP: {current.currentXP} / {current.xpToNextLevel}";
            AttackText.text = $"Si³a: {current.damage}";
            DefText.text = $"Obrona: {current.defense}";

            if (pokeImage != null && current.sprite != null)
            {
                pokeImage.enabled = true;
                pokeImage.sprite = current.sprite;
            }

            if (slotIndexText != null)
            {
                slotIndexText.text = $"{selectedPokeIndex + 1} / {playerData.team.Count}";
            }

            if (setLeaderButton != null)
            {
                // Przycisk "Ustaw jako lidera" jest aktywny tylko, gdy nie ogl¹damy ju¿ lidera
                setLeaderButton.interactable = (selectedPokeIndex != 0);
            }
        }
        else
        {
            nameText.text = "Brak Pokezaurów";
            levelText.text = ""; hpText.text = ""; xpText.text = "";
            AttackText.text = ""; DefText.text = "";
            if (pokeImage != null) pokeImage.enabled = false;
            if (slotIndexText != null) slotIndexText.text = "0 / 0";
            if (setLeaderButton != null) setLeaderButton.interactable = false;
        }
    }

    void UpdateInventory()
    {
        foreach (Transform child in inventoryContainer.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (InventorySlot slot in playerData.inventory)
        {
            GameObject newSlot = Instantiate(itemSlotPrefab, inventoryContainer.transform);
            TextMeshProUGUI[] texts = newSlot.GetComponentsInChildren<TextMeshProUGUI>();

            if (texts.Length >= 1)
            {
                if (playerData.currentEquippedItem == slot.item)
                {
                    texts[0].text = slot.item.itemName + " [Za³o¿ony]";
                    texts[0].color = Color.green;
                }
                else
                {
                    texts[0].text = slot.item.itemName;
                    texts[0].color = Color.black;
                }
            }

            if (texts.Length >= 2) texts[1].text = "x" + slot.amount;

            Button btn = newSlot.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnItemClicked(slot.item));
            }
        }
    }

    void OnItemClicked(ItemData item)
    {
        // U¿ywamy przedmiotu na aktualnie przegl¹danym Pokezaurze (selectedPokeIndex)
        playerData.UseItem(item, selectedPokeIndex);
        UpdateStats();
        UpdateInventory();
    }
}