using UnityEngine;
using System.Collections.Generic;

// --- DEFINICJE STATYSTYK I TYPÓW ---
public enum StatType { Strength, Defense, MaxHP, Speed }
public enum ItemType { HealHP, PermSTR, PermDEF, PermHP, Gold, Equipment, Misc, Consumable, CatchItem } // Uproœci³em typy do 3 g³ównych kategorii

[System.Serializable]
public class StatModifier
{
    public StatType statType;
    public int amount;
}

[CreateAssetMenu(fileName = "New Item", menuName = "Pokemon/Item")]
public class ItemData : ScriptableObject
{
    [Header("Wygl¹d i Opis")]
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Dzia³anie")]
    public ItemType itemType;

    public int price = 10;

    // --- DLA POTEK I Z£OTA (Prosta wartoœæ) ---
    [Tooltip("U¿yj tego dla iloœci leczenia lub iloœci z³ota w worku")]
    public int value;

    // --- DLA EKWIPUNKU (Lista bonusów) ---
    [Tooltip("U¿yj tego TYLKO dla przedmiotów typu Equipment")]
    public List<StatModifier> modifiers = new List<StatModifier>();
}