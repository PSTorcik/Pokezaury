using UnityEngine;

[System.Serializable] // Musi tu byæ, ¿eby dzia³a³ zapis gry i widok w Inspektorze!
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

    // Konstruktor do ³apania
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
        xpToNextLevel = 100 * level;
    }

    // Logika awansu przeniesiona z PlayerData
    public bool AddXP(int amount)
    {
        currentXP += amount;
        bool hasLeveledUp = false;

        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
            hasLeveledUp = true;
        }

        return hasLeveledUp;
    }

    private void LevelUp()
    {
        level++;
        currentXP -= xpToNextLevel;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.5f);

        maxHP += 5;
        currentHP = maxHP; // Leczenie przy awansie
        damage += 2;
        defense += 1;

        Debug.Log($"{pokeName} awansuje na poziom {level}!");
    }
}