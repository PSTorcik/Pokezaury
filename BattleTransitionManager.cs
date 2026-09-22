using System.Collections.Generic;
using UnityEngine;

public static class BattleTransitionManager
{
    //WALKA
    public static EnemyData enemyToFight;

    // NOWE: Tu zapiszemy pozycjê gracza
    public static Vector3 lastPlayerPosition;

    // NOWE: Prze³¹cznik, czy wracamy z walki
    public static bool isReturningFromBattle = false;

    public static bool justRanAway = false; // Czy uciekliœmy z ostatniej walki?

    // NOWE: ID przeciwnika, z którym aktualnie walczymy
    public static string currentEnemyID;

    // --- NOWOŒÆ: Stan wysokoœci ---
    public static bool isOnElevation = false;

    // NOWE: Lista pokonanych przeciwników (baza danych "trupów")
    public static List<string> defeatedEnemies = new List<string>();

    //DRZWI
    // NOWE: Pozycja, w której gracz ma siê pojawiæ po zmianie sceny
    public static Vector3 targetSpawnPosition;

    public static bool spawnPositionActive = false;
}