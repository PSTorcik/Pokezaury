using UnityEngine;
using UnityEngine.UI; // Jeœli u¿ywasz UI nad g³owami

public class BattleUnit : MonoBehaviour
{
    // Pola na statystyki lokalne w czasie walki
    public string unitName;
    public int damage;
    public int maxHP;
    public int defense;
    public int unitLevel;
    public int currentHP;

    // Odniesienie do SpriteRenderer, ¿eby zmieniæ obrazek
    public SpriteRenderer mySpriteRenderer;

    // Funkcja do ustawiania przeciwnika (z danych EnemyData)
    public void SetupEnemy(EnemyData enemyData)
    {
        unitName = enemyData.enemyName;
        maxHP = enemyData.maxHealth;
        currentHP = enemyData.maxHealth; // Przeciwnik zawsze zaczyna z pe³nym ¿yciem
        damage = enemyData.attackPower;
        unitLevel = enemyData.unitLevel;
        defense = enemyData.defense;

        // Ustawiamy grafikê potwora
        mySpriteRenderer.sprite = enemyData.battleSprite;
    }

    // Funkcja do ustawiania gracza (z danych PlayerData)
    public void SetupPlayer(Pokezaur myPokezaur)
    {
        unitName = myPokezaur.pokeName;
        unitLevel = myPokezaur.level;
        maxHP = myPokezaur.maxHP;
        damage = myPokezaur.damage;
        defense = myPokezaur.defense;
        currentHP = myPokezaur.currentHP;

        mySpriteRenderer.sprite = myPokezaur.sprite;
    }

    // Metoda zwraca 'true', jeœli jednostka umar³a
    public bool TakeDamage(int rawDamage)
    {
        // Redukujemy obra¿enia o obronê
        int actualDamage = rawDamage - defense/2;

        // Zabezpieczenie: ¯eby atak nie leczy³ (jeœli obrona jest wy¿sza ni¿ atak)
        // Zawsze zadajemy minimum 1 obra¿enia (chyba ¿e chcesz ca³kowit¹ odpornoœæ)
        if (actualDamage < 1) actualDamage = 1;

        currentHP -= actualDamage;

        if (currentHP <= 0)
        {
            currentHP = 0;
            return true;
        }
        return false;
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP)
        {
            currentHP = maxHP; // Nie mo¿na mieæ wiêcej ni¿ max ¿ycia
        }
    }

    // Nowa funkcja do losowania si³y ataku
    public int CalculateDamage()
    {
        // Przyk³ad: Obra¿enia mog¹ byæ o 2 mniejsze lub o 2 wiêksze od bazy
        // Jeœli damage = 10, wylosuje liczbê z przedzia³u 8-12
        int minDmg = Mathf.FloorToInt(damage * 0.9f);
        int maxDmg = Mathf.FloorToInt(damage * 1.1f);

        // Zabezpieczenie: ¿eby nie zadaæ 0 lub ujemnych obra¿eñ
        if (minDmg < 1) minDmg = 1;

        // Random.Range dla int jest ekskluzywne dla górnej granicy, wiêc dajemy +1
        return Random.Range(minDmg, maxDmg + 1);
    }
}