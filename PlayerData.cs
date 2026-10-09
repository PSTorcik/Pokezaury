using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
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

    [Header("Dru¿yna (Max 6)")]
    public List<Pokezaur> team = new List<Pokezaur>();

    [Header("Statystyki Trenera")]
    public float moveSpeed = 5f;
    public int currentGold = 0;

    [Header("Audio")]
    public AudioClip EQSound;
    public AudioClip potionSound;
    public AudioClip consumableSound;

    [Header("Zapis Gry")]
    public List<string> collectedItemIDs = new List<string>();

    public List<InventorySlot> inventory = new List<InventorySlot>();
    public ItemData currentEquippedItem; // Zostawiamy dla pierwszego stworka

    public bool CheckIfCollected(string id) { return collectedItemIDs.Contains(id); }
    public void MarkAsCollected(string id) { if (!collectedItemIDs.Contains(id)) collectedItemIDs.Add(id); }

    public void AddItem(ItemData itemToAdd)
    {
        foreach (InventorySlot slot in inventory)
        {
            if (slot.item == itemToAdd)
            {
                slot.amount++;
                return;
            }
        }
        inventory.Add(new InventorySlot(itemToAdd, 1));
    }

    public void RemoveItem(ItemData itemToRemove)
    {
        foreach (InventorySlot slot in inventory)
        {
            if (slot.item == itemToRemove)
            {
                slot.amount--;
                if (slot.amount <= 0) inventory.Remove(slot);
                return;
            }
        }
    }

    public void AddGold(int amount) { currentGold += amount; }

    public bool SpendGold(int amount)
    {
        if (currentGold >= amount) { currentGold -= amount; return true; }
        return false;
    }

    public int LoseGoldOnDeath()
    {
        int goldToLose = Mathf.FloorToInt(currentGold * 0.25f);
        currentGold -= goldToLose;
        if (currentGold < 0) currentGold = 0;
        return goldToLose;
    }

    // --- U¯YWANIE PRZEDMIOTÓW (NA LIDERZE DRU¯YNY) ---
    public void UseItem(ItemData item)
    {
        if (team.Count == 0 && item.itemType != ItemType.Gold)
        {
            Debug.LogWarning("Brak Pokezaurów w dru¿ynie!");
            return;
        }

        Pokezaur leadPoke = (team.Count > 0) ? team[0] : null;

        switch (item.itemType)
        {
            case ItemType.HealHP:
            case ItemType.Consumable:
                if (leadPoke.currentHP >= leadPoke.maxHP) return;
                leadPoke.currentHP += item.value;
                if (leadPoke.currentHP > leadPoke.maxHP) leadPoke.currentHP = leadPoke.maxHP;
                RemoveItem(item);
                if (item.itemType == ItemType.HealHP) PlaySound(potionSound);
                else PlaySound(consumableSound);
                break;

            case ItemType.Gold:
                AddGold(item.value);
                RemoveItem(item);
                break;

            case ItemType.PermSTR:
                leadPoke.damage += item.value;
                RemoveItem(item);
                PlaySound(potionSound);
                break;

            case ItemType.PermDEF:
                leadPoke.defense += item.value;
                RemoveItem(item);
                PlaySound(potionSound);
                break;

            case ItemType.PermHP:
                leadPoke.maxHP += item.value;
                leadPoke.currentHP += item.value;
                RemoveItem(item);
                PlaySound(potionSound);
                break;

            case ItemType.Equipment:
                HandleEquipment(item, leadPoke);
                PlaySound(EQSound);
                break;
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && AudioManager.instance != null) AudioManager.instance.PlaySFX(clip);
    }

    void HandleEquipment(ItemData item, Pokezaur poke)
    {
        if (currentEquippedItem == item) UnequipItem(poke);
        else
        {
            if (currentEquippedItem != null) UnequipItem(poke);
            EquipItem(item, poke);
        }
    }

    void EquipItem(ItemData item, Pokezaur poke)
    {
        currentEquippedItem = item;
        foreach (StatModifier mod in item.modifiers)
        {
            switch (mod.statType)
            {
                case StatType.Strength: poke.damage += mod.amount; break;
                case StatType.Defense: poke.defense += mod.amount; break;
                case StatType.MaxHP: poke.maxHP += mod.amount; poke.currentHP += mod.amount; break;
                case StatType.Speed: moveSpeed += mod.amount; break; // Speed zostaje dla trenera
            }
        }
    }

    void UnequipItem(Pokezaur poke)
    {
        if (currentEquippedItem != null)
        {
            foreach (StatModifier mod in currentEquippedItem.modifiers)
            {
                switch (mod.statType)
                {
                    case StatType.Strength: poke.damage -= mod.amount; break;
                    case StatType.Defense: poke.defense -= mod.amount; break;
                    case StatType.MaxHP:
                        poke.maxHP -= mod.amount;
                        if (poke.currentHP > poke.maxHP) poke.currentHP = poke.maxHP;
                        break;
                    case StatType.Speed: moveSpeed -= mod.amount; break;
                }
            }
            currentEquippedItem = null;
        }
    }
}