using System.Collections.Generic; // WA¯NE: Potrzebne do obs³ugi Listy
using UnityEngine;

// --- NOWA KLASA POMOCNICZA (Slot w plecaku) ---
[System.Serializable] // Dziêki temu zobaczymy listê w Inspektorze
public class InventorySlot
{
    public ItemData item;
    public int amount;

    public InventorySlot(ItemData _item, int _amount)
    {
        item = _item;
        amount = _amount;
    }
}

[CreateAssetMenu(fileName = "New Player", menuName = "Pokemon/Player")]
public class PlayerData : ScriptableObject
{

    public string unitName;
    // --- USUWAMY STARE STATYSTYKI GRACZA (HP, Damage, Level itp.) ---
    // Zastêpujemy je list¹ Pokezaurów:

    [Header("Dru¿yna (Max 6)")]
    public List<Pokezaur> team = new List<Pokezaur>();

    [Header("Inne")]
    public float moveSpeed = 5f;
    public int currentGold = 0;

    // ... (Zostaw wszystkie funkcje AddItem, RemoveItem, UseItem, AddGold itp.)

    // UWAGA: Musisz usun¹æ z UseItem odniesienia do currentHP i maxHP gracza, 
    // poniewa¿ leczenie bêdzie teraz dotyczy³o Pokezaura, a nie trenera.


    public ItemData currentEquippedItem;
    public AudioClip EQSound;
    public AudioClip potionSound;
    public AudioClip consumableSound;
    [Header("Ustawienia")]
    public bool allowChinesePropaganda = false; 
    [Header("Zapis Gry")]
    public List<string> collectedItemIDs = new List<string>();

    // Funkcja sprawdzaj¹ca czy przedmiot o danym ID zosta³ ju¿ zebrany
    public bool CheckIfCollected(string id)
    {
        return collectedItemIDs.Contains(id);
    }

    // Funkcja dodaj¹ca przedmiot do listy zebranych
    public void MarkAsCollected(string id)
    {
        if (!collectedItemIDs.Contains(id))
        {
            collectedItemIDs.Add(id);
        }
    }

    // --- PLECAK ---
    public List<InventorySlot> inventory = new List<InventorySlot>();
    // Funkcja dodaj¹ca przedmiot (u¿yjemy jej przy podnoszeniu i kupowaniu)
    public void AddItem(ItemData itemToAdd)
    {
        // 1. Sprawdzamy czy ju¿ mamy taki przedmiot, ¿eby go "sztackowaæ"
        foreach (InventorySlot slot in inventory)
        {
            if (slot.item == itemToAdd)
            {
                slot.amount++;
                Debug.Log($"Dodano {itemToAdd.itemName}. Masz teraz: {slot.amount}");
                return;
            }
        }

        // 2. Jeœli nie mamy, tworzymy nowy slot
        inventory.Add(new InventorySlot(itemToAdd, 1));
    }

    // Funkcja usuwaj¹ca przedmiot po u¿yciu
    public void RemoveItem(ItemData itemToRemove)
    {
        foreach (InventorySlot slot in inventory)
        {
            if (slot.item == itemToRemove)
            {
                slot.amount--;
                if (slot.amount <= 0)
                {
                    inventory.Remove(slot); // Usuwamy slot, jeœli iloœæ to 0
                }
                return;
            }
        }
    }
    public void UseItem(ItemData item)
    {
        switch (item.itemType)
        {
            // PRZYPADEK 1: MIKSTURY I JEDZENIE (U¿ywamy pola 'value')
            case ItemType.HealHP:
                if (currentHP >= maxHP)
                {
                    GameNotifications.instance.ShowMessage("Masz ju¿ pe³ne zdrowie!", 5f);
                    return;
                }

                currentHP += item.value;
                if (currentHP > maxHP) currentHP = maxHP;
                GameNotifications.instance.ShowMessage($"Wypito {item.itemName}. Uleczono {item.value} HP.", 5f);
                RemoveItem(item); // Zu¿ywamy przedmiot
                PlayPotionSound();
                break;

            case ItemType.Consumable:
                if (currentHP >= maxHP)
                {
                    GameNotifications.instance.ShowMessage("Masz ju¿ pe³ne zdrowie!", 5f);
                    return;
                }

                currentHP += item.value;
                if (currentHP > maxHP) currentHP = maxHP;
                GameNotifications.instance.ShowMessage($"Zjedzono {item.itemName}. Uleczono {item.value} HP.", 5f);
                RemoveItem(item);
                if (consumableSound != null && AudioManager.instance != null)
                {
                    AudioManager.instance.PlaySFX(consumableSound);
                }
                break;


            case ItemType.Gold:
                AddGold(item.value); // Dodajemy z³oto do portfela
                GameNotifications.instance.ShowMessage($"Otwarto {item.itemName}. Zyskano {item.value} z³ota.", 5f);
                RemoveItem(item); // Worek znika
                break;

            case ItemType.Misc:
                GameNotifications.instance.ShowMessage($"Nie mo¿esz nic z tym zrobiæ", 5f);
                break;


            case ItemType.PermSTR:
                damage += item.value;
                GameNotifications.instance.ShowMessage($"Wypito {item.itemName}. Zyskano {item.value} ataku.", 5f);
                RemoveItem(item);
                PlayPotionSound();
                break;

            case ItemType.PermDEF:
                defense += item.value; 
                GameNotifications.instance.ShowMessage($"Wypito {item.itemName}. Zyskano {item.value} obrony.", 5f);
                RemoveItem(item);
                PlayPotionSound();
                break;

            case ItemType.PermHP:
                maxHP += item.value; // Dodajemy z³oto do portfela
                GameNotifications.instance.ShowMessage($"Wypito {item.itemName}. Zyskano {item.value} maksymalnego ¿ycia.", 5f);
                RemoveItem(item);
                PlayPotionSound();
                break;



            // PRZYPADEK 3: EKWIPUNEK (U¿ywamy listy 'modifiers')
            case ItemType.Equipment:
                HandleEquipment(item);
                if (EQSound != null && AudioManager.instance != null)
                {
                    AudioManager.instance.PlaySFX(EQSound);
                }
                break;
        }
    }
    void PlayPotionSound() {
        if (potionSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(potionSound);
        }
    }

    void HandleEquipment(ItemData item)
    {
        if (currentEquippedItem == item)
        {
            UnequipItem(); // Zdejmij, jeœli to samo
        }
        else
        {
            if (currentEquippedItem != null) UnequipItem(); // Zdejmij stare
            EquipItem(item); // Za³ó¿ nowe
        }
    }

    void EquipItem(ItemData item)
    {
        currentEquippedItem = item;

        // PÊTLA PO WSZYSTKICH CECHACH PRZEDMIOTU
        foreach (StatModifier mod in item.modifiers)
        {
            switch (mod.statType)
            {
                case StatType.Strength:
                    damage += mod.amount;
                    break;
                case StatType.Defense:
                    defense += mod.amount;
                    break;
                case StatType.MaxHP:
                    maxHP += mod.amount;
                    currentHP += mod.amount;
                    break;
                case StatType.Speed:
                    moveSpeed += mod.amount;
                    break;
            }
        }

        GameNotifications.instance.ShowMessage($"Za³o¿ono: {item.itemName}.", 5f);
    }

    void UnequipItem()
    {
        if (currentEquippedItem != null)
        {
            // PÊTLA ODWROTNA - ODEJMUJEMY STATYSTYKI
            foreach (StatModifier mod in currentEquippedItem.modifiers)
            {
                switch (mod.statType)
                {
                    case StatType.Strength:
                        damage -= mod.amount;
                        break;
                    case StatType.Defense:
                        defense -= mod.amount;
                        break;
                    case StatType.MaxHP:
                        maxHP -= mod.amount;
                        // Wa¿ne zabezpieczenie:
                        // Jeœli zdejmiemy zbrojê i MaxHP spadnie poni¿ej obecnego HP, musimy przyci¹æ obecne HP.
                        if (currentHP > mod.amount) currentHP -= mod.amount;
                        else currentHP = 1; // Zostawiamy przynajmniej 1 HP
                        break;
                    case StatType.Speed:
                        moveSpeed -= mod.amount;
                        break;
                }
            }
            GameNotifications.instance.ShowMessage($"Zdjêto: {currentEquippedItem.itemName}.", 5f);
            currentEquippedItem = null;
        }
    }




    // Funkcja, któr¹ wywo³a BattleSystem po walce
    public bool AddXP(int amount)
    {
        currentXP += amount;
        bool hasLeveledUp = false; // Flaga sprawdzaj¹ca, czy chocia¿ raz awansowaliœmy

        // Zmieniamy 'if' na 'while' - pêtla wykonuje siê tak d³ugo, 
        // jak d³ugo mamy wystarczaj¹co du¿o XP na awans
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
            hasLeveledUp = true;
        }

        return hasLeveledUp; // Zwracamy prawdê tylko jeœli poziom wzrós³
    }

    void LevelUp()
    {
        unitLevel++;
        currentXP -= xpToNextLevel; // Przenosimy nadmiar XP na kolejny poziom

        // Zwiêkszamy wymagania na kolejny poziom (np. o 50%)
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.5f);

        // ZWIÊKSZANIE STATYSTYK (Prosta matematyka)
        maxHP += 5;
        currentHP = maxHP; // Leczymy gracza przy awansie
        damage += 2;
        defense += 1;
    }
    public void AddGold(int amount)
    {
        currentGold += amount;
    }
    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            return true; // Transakcja udana
        }
        else
        {
            return false; // Transakcja odrzucona
        }
    }
    public int LoseGoldOnDeath()
    {

        int goldToLose = Mathf.FloorToInt(currentGold * 0.25f);

        // 2. Zabieramy z³oto
        currentGold -= goldToLose;

        // 3. Zabezpieczenie (¿eby nie wyjœæ na minus, choæ tu matematycznie nie powinno)
        if (currentGold < 0) currentGold = 0;

        // 4. Zwracamy kwotê, któr¹ straciliœmy (¿eby wyœwietliæ w UI)
        return goldToLose;
    }
}