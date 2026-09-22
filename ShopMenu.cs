using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopMenu : MonoBehaviour
{
    public static ShopMenu instance;
    public bool isShopOpen = false;

    [Header("UI")]
    public GameObject shopPanel;
    public Transform shopContainer;   // Kontener po lewej (Towar NPC)
    public Transform playerContainer; // Kontener po prawej (Twój plecak)
    public GameObject itemSlotPrefab; // Ten sam prefab co w StatsMenu!
    public TextMeshProUGUI goldText;  // Wyœwietlanie z³ota gracza

    [Header("Dane")]
    public PlayerData playerData;
    public AudioClip shopSound;

    // Lista przedmiotów, które ma AKTUALNY sklepikarz (zmienia siê zale¿nie od NPC)
    private List<ItemData> currentShopStock;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Update()
    {
        // Zamykanie sklepu klawiszem ESC lub B
        if (shopPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.B))
            {
                CloseShop();
            }
        }
    }

    // --- OTWIERANIE I ZAMYKANIE ---

    public void OpenShop(List<ItemData> itemsForSale)
    {
        currentShopStock = itemsForSale;
        shopPanel.SetActive(true);
        isShopOpen = true;
        Time.timeScale = 0f; // Pauza gry

        // Zamykamy inne menu jeœli s¹ otwarte
        if (StatsMenu.instance != null) StatsMenu.instance.CloseMenu();

        RefreshUI();
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        isShopOpen = false;
        Time.timeScale = 1f;
    }

    // --- ODŒWIE¯ANIE WIDOKU ---

    void RefreshUI()
    {
        // 1. Aktualizacja z³ota
        if (goldText != null)
            goldText.text = playerData.currentGold.ToString();

        // 2. Czyœcimy stare sloty
        foreach (Transform child in shopContainer) Destroy(child.gameObject);
        foreach (Transform child in playerContainer) Destroy(child.gameObject);

        // 3. Rysujemy Sklep (LEWA STRONA) - Kupowanie
        if (currentShopStock != null)
        {
            foreach (ItemData item in currentShopStock)
            {
                GameObject slot = Instantiate(itemSlotPrefab, shopContainer);
                SetupSlot(slot, item, true);
            }
        }

        // 4. Rysujemy Gracza (PRAWA STRONA) - Sprzedawanie
        foreach (InventorySlot slot in playerData.inventory)
        {
            GameObject newSlot = Instantiate(itemSlotPrefab, playerContainer);

            // SetupSlot ustawia ju¿ cenê ORAZ iloœæ ("x5 (50 G)")
            SetupSlot(newSlot, slot.item, false);

        }
    }

    // Funkcja pomocnicza do ustawiania wygl¹du i przycisku
    void SetupSlot(GameObject slotObj, ItemData item, bool isBuyingMode)
    {
        TextMeshProUGUI[] texts = slotObj.GetComponentsInChildren<TextMeshProUGUI>();

        if (isBuyingMode)
        {
            // Widok sklepu: Poka¿ cenê
            texts[0].text = item.itemName;
            if (texts.Length >= 2)
            {
                texts[1].text = $"<color=yellow>{item.price} G</color>";
                // Upewnij siê, ¿e czcionka jest wystarczaj¹co ma³a, np. 14-16pkt
            }
        }
        else
        {
            // Widok gracza: Poka¿ cenê sprzeda¿y (np. po³owa)
            texts[0].text = item.itemName;
            if (texts.Length >= 2)
            {
                // ZnajdŸ iloœæ tego przedmiotu w plecaku
                InventorySlot slot = playerData.inventory.Find(x => x.item == item);
                int amount = slot != null ? slot.amount : 0;
                int sellPrice = item.price / 2;

                // Np.: "x5 (50 G)"
                texts[1].text = $"x{amount} <color=green>{sellPrice} G</color>";
            }
        }

        // Obrazek (jeœli masz ikonki w itemData)
        // Image icon = slotObj.transform.Find("Icon").GetComponent<Image>();
        // icon.sprite = item.icon;

        // --- INTERAKCJA ---
        Button btn = slotObj.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();

        if (isBuyingMode)
        {
            btn.onClick.AddListener(() => BuyItem(item));
        }
        else
        {
            btn.onClick.AddListener(() => SellItem(item));
        }
    }

    // --- LOGIKA TRANSAKCJI ---

    void BuyItem(ItemData item)
    {
        if (playerData.currentGold >= item.price)
        {
            playerData.currentGold -= item.price;
            playerData.AddItem(item);

            if (shopSound != null && AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX(shopSound);
            }
            GameNotifications.instance.ShowMessage($"Kupiono: {item.itemName}", 3f);

            RefreshUI();
        }
        else
        {
            GameNotifications.instance.ShowMessage("Brak z³ota!", 1.5f);
        }
    }

    void SellItem(ItemData item)
    {
        // ZnajdŸ slot z przedmiotem
        InventorySlot slot = playerData.inventory.Find(x => x.item == item);

        // --- NOWOŒÆ: ZABEZPIECZENIE PRZED SPRZEDA¯¥ ZA£O¯ONEGO ---
        if (playerData.currentEquippedItem == item)
        {
            // Jeœli to ten przedmiot i mamy go tylko 1 sztukê (czyli tê na sobie)
            if (slot != null && slot.amount <= 1)
            {
                // Wyœwietl komunikat (jeœli masz system powiadomieñ, jak nie to Debug.Log)
                if (GameNotifications.instance != null)
                    GameNotifications.instance.ShowMessage("Zdejmij przedmiot przed sprzeda¿¹!", 2f);
                else
                    Debug.Log("Nie mo¿esz sprzedaæ za³o¿onego przedmiotu!");

                return; // STOP! Przerywamy funkcjê, nic nie sprzedajemy.
            }
        }
        // -----------------------------------------------------------

        if (slot != null)
        {
            int sellPrice = item.price / 2;
            playerData.currentGold += sellPrice;

            playerData.RemoveItem(item);

            if (shopSound != null && AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX(shopSound);
            }
            GameNotifications.instance.ShowMessage($"Sprzedano: {item.itemName}", 3f);
            RefreshUI();
        }
    }
}