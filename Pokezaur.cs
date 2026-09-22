using UnityEngine;

[System.Serializable] // Bardzo wa¿ne! Pozwala widzieæ to w Inspektorze i zapisywaæ do pliku
public class Pokezaur
{
    public string pokeName;
    public int level;
    public int currentHP;
    public int maxHP;
    public int damage;
    public int defense;
    public int currentXP;
    public int xpToNextLevel;
    public Sprite sprite;

    // Konstruktor do tworzenia nowego Pokezaura (np. przy ³apaniu dzikiego)
    public Pokezaur(EnemyData wildEnemy)
    {
        pokeName = wildEnemy.enemyName;
        level = wildEnemy.unitLevel;
        maxHP = wildEnemy.maxHealth;
        currentHP = wildEnemy.maxHealth;
        damage = wildEnemy.attackPower;
        defense = wildEnemy.defense;
        sprite = wildEnemy.battleSprite;

        currentXP = 0;
        xpToNextLevel = 100 * level; // Proste skalowanie XP
    }
}