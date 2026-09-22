using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Pokemon/Enemy")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int maxHealth;
    public int attackPower;
    public int unitLevel;

    public int defense; // Obrona: O tyle mniej obra¿eñ otrzyma
    public int xpReward; // Nagroda: Tyle XP dostanie gracz za wygran¹
    public int goldReward;

    public Sprite battleSprite;
}