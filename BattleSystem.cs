using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Potrzebne do obs³ugi Slidera
using TMPro;          // Potrzebne do obs³ugi tekstów TextMeshPro
using UnityEngine.SceneManagement; // Potrzebne do zmiany scen

// Definicja stanów walki
public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

public class BattleSystem : MonoBehaviour
{
    public BattleState state;

    // Jednostki na scenie
    public BattleUnit playerUnit;
    public BattleUnit enemyUnit;

    // Interfejsy (HUD)
    public BattleHUD playerHUD;
    public BattleHUD enemyHUD;

    // ród³o danych gracza (ScriptableObject)
    public PlayerData playerDataSource;

    // Tekst dialogowy (np. "Atakujesz!")
    public TextMeshProUGUI dialogueText;

    [Header("Debug / Testowanie")]
    public EnemyData debugEnemy; // Tu wrzucisz wroga do testów

    [Header("DŸwiêki")]
    public AudioClip victorySound; // Tu wrzucisz fanfary
    public AudioClip defeatSound;  // Tu wrzucisz smutn¹ tr¹bkê
    public AudioClip hitSound;

    void Start()
    {
        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        // 1. Pobranie danych przeciwnika z Mened¿era Przejœcia
        EnemyData enemyData = BattleTransitionManager.enemyToFight;

        // ZABEZPIECZENIE: Jeœli nie ma wroga w Managerze (np. testujemy sam¹ scenê walki), u¿yj testowego
        if (enemyData == null)
        {
            if (debugEnemy != null)
            {
                Debug.LogWarning("TRYB TESTOWY: U¿ywam przeciwnika z pola DebugEnemy.");
                enemyData = debugEnemy;

                // KLUCZOWE: Przypisujemy go te¿ do Managera!
                // Dziêki temu funkcja EndBattle() nie wyrzuci b³êdu przy rozdawaniu XP.
                BattleTransitionManager.enemyToFight = debugEnemy;
            }
            else
            {
                Debug.LogError("B£¥D: Brak przeciwnika! Odpal grê od Mapy albo przypisz DebugEnemy.");
                dialogueText.text = "B³¹d: Brak danych wroga!";
                yield break;
            }
        }

        // 2. Konfiguracja jednostek na scenie
        enemyUnit.SetupEnemy(enemyData);



        //temp2
        // --- ZMIENIONY KOD: Szukamy pierwszego ¿ywego Pokezaura ---
        Pokezaur firstAlivePokezaur = null;

        if (playerDataSource.team.Count > 0)
        {
            foreach (Pokezaur poke in playerDataSource.team)
            {
                if (poke.currentHP > 0)
                {
                    firstAlivePokezaur = poke;
                    break; // ZnaleŸliœmy! Przerywamy pêtlê.
                }
            }

            if (firstAlivePokezaur != null)
            {
                // Wystawiamy znalezionego, ¿ywego Pokezaura
                playerUnit.SetupPlayer(firstAlivePokezaur);
            }
            else
            {
                // PRZEGRANA: Nie mamy ¿ywych stworków!
                dialogueText.text = "Nie masz Pokezaurów zdolnych do walki!";
                yield return WaitForInputOrTime(1.5f);

                state = BattleState.LOST;
                EndBattle(); // Wywo³ujemy standardow¹ procedurê przegranej
                yield break; // Przerywamy dalsze ³adowanie walki
            }
        }
        else
        {
            // PRZEGRANA: Nie mamy ¿adnych stworków!
            dialogueText.text = "Nie masz ¿adnych Pokezaurów zdolnych do walki!";
            yield return WaitForInputOrTime(1.5f);

            state = BattleState.LOST;
            EndBattle(); // Wywo³ujemy standardow¹ procedurê przegranej
            yield break; // Przerywamy dalsze ³adowanie walki
        }

            // 3. Konfiguracja pasków ¿ycia i tekstów
            enemyHUD.SetHUD(enemyUnit);
            playerHUD.SetHUD(playerUnit);

        // --- NOWOŒÆ: Wyœwietlamy Jajka w UI ---
        playerHUD.UpdateTeamUI(playerDataSource.team);

        // 4. Powitanie
        dialogueText.text = "Dzikie " + enemyUnit.unitName + " atakuje!";

            yield return WaitForInputOrTime(0.5f);

            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }

    void PlayerTurn()
    {
        dialogueText.text = "Wybierz akcjê:";
    }

    // --- FUNKCJE PRZYCISKÓW (Podepnij w Unity pod OnClick) ---

    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerAttack());
    }

    public void OnHealButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerHeal());
    }

    // Itemki
    // PODEPNIJ TO POD PRZYCISK "HEAL" lub "ITEM"
    public void OnPotionButton()
    {
        if (state != BattleState.PLAYERTURN) return;

        StartCoroutine(UseItemRoutine());
    }

    // FUNKCJA: UCIECZKA
    public void OnRunButton()
    {
        // Mo¿na uciekaæ tylko w swojej turze
        if (state != BattleState.PLAYERTURN) return;

        StartCoroutine(TryToRun());
    }

    IEnumerator TryToRun()
    {
        state = BattleState.ENEMYTURN;
        dialogueText.text = "Próbujesz uciec...";
        yield return WaitForInputOrTime(1f);

        bool success = Random.Range(0, 2) == 0;


        if (success)
        {
            dialogueText.text = "Uda³o siê uciec!";
            yield return WaitForInputOrTime(1f);

            playerDataSource.currentHP = playerUnit.currentHP;

            // Ustawianie flag
            BattleTransitionManager.isReturningFromBattle = true;
            BattleTransitionManager.justRanAway = true;

            SceneManager.LoadScene("MapScene");
        }
        else
        {
            // --- SCENARIUSZ: PORA¯KA ---
            dialogueText.text = "Nie uda³o siê uciec!";
            yield return WaitForInputOrTime(1f);

            dialogueText.text = "Tracisz turê!";
            yield return WaitForInputOrTime(1f);

            // Skoro ucieczka siê nie uda³a, nastêpuje tura przeciwnika
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator UseItemRoutine(ItemData itemToUse)
    {
        state = BattleState.ENEMYTURN; // Zu¿ywamy turê

        playerDataSource.RemoveItem(itemToUse);
        dialogueText.text = $"U¿yto: {itemToUse.itemName}!";
        yield return WaitForInputOrTime(1f);

        // --- MECHANIKA £APANIA ---
        if (itemToUse.itemType == ItemType.CatchItem)
        {
            dialogueText.text = "Rzucasz Jajem Pokezaura!";
            yield return WaitForInputOrTime(1f);

            // Losowanie 50%
            if (Random.value <= 0.5f)
            {
                // SUKCES
                dialogueText.text = "Uda³o siê! Pokezaur z³apany!";
                yield return WaitForInputOrTime(1.5f);

                // Sprawdzamy czy mamy miejsce w dru¿ynie
                if (playerDataSource.team.Count < 6)
                {
                    // Tworzymy nowego Pokezaura z danych wroga i dodajemy do dru¿yny
                    Pokezaur newPoke = new Pokezaur(BattleTransitionManager.enemyToFight);
                    playerDataSource.team.Add(newPoke);
                    dialogueText.text = $"{newPoke.pokeName} do³¹cza do dru¿yny!";
                }
                else
                {
                    dialogueText.text = "Brak miejsca w dru¿ynie! (System PC wkrótce)";
                }

                yield return WaitForInputOrTime(1.5f);

                // Koñczymy walkê jak przy wygranej (ale bez XP)
                BattleTransitionManager.justRanAway = false;
                BattleTransitionManager.defeatedEnemies.Add(BattleTransitionManager.currentEnemyID);
                SceneManager.LoadScene("MapScene");
                yield break;
            }
            else
            {
                // PORA¯KA
                dialogueText.text = "O nie! Pokezaur siê uwolni³!";
                yield return WaitForInputOrTime(1f);
            }
        }
        // --- MECHANIKA LECZENIA (Dla pierwszego stworka) ---
        else if (itemToUse.itemType == ItemType.HealHP)
        {
            playerUnit.Heal(itemToUse.value);
            playerHUD.SetHP(playerUnit.currentHP);

            // Zapisujemy od razu w strukturze dru¿yny
            playerDataSource.team[0].currentHP = playerUnit.currentHP;

            dialogueText.text = $"+{itemToUse.value} HP!";
            yield return WaitForInputOrTime(1f);
        }

        // Tura wroga
        StartCoroutine(EnemyTurn());
    }

    // --- LOGIKA WALKI ---

    IEnumerator PlayerAttack()
    {
        // Blokujemy przyciski
        state = BattleState.ENEMYTURN;

        // 1. Wylosowanie obra¿eñ (zgodnie z funkcj¹ w BattleUnit)
        int dmg = playerUnit.CalculateDamage();

        // 2. Zadanie obra¿eñ przeciwnikowi (funkcja zwraca true, jeœli wróg zgin¹³)
        // Uwaga: TakeDamage wewn¹trz BattleUnit uwzglêdnia ju¿ Obronê (defense)!
        bool isDead = enemyUnit.TakeDamage(dmg);

        // 3. Aktualizacja UI przeciwnika
        enemyHUD.SetHP(enemyUnit.currentHP);

        // 4. Informacja tekstowa
        dialogueText.text = "Zadajesz " + dmg + " pkt obra¿eñ!";

        if (hitSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(hitSound);
        }

        yield return WaitForInputOrTime(0.5f);

        // 5. Sprawdzenie wyniku
        if (isDead)
        {
            state = BattleState.WON;
            EndBattle();
        }
        else
        {
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator PlayerHeal()
    {
        state = BattleState.ENEMYTURN;

        // WZÓR: (2 * Poziom) + 6
        // Pobieramy level z jednostki gracza
        int healAmount = (playerUnit.unitLevel * 2) + 6;

        int minHeal = Mathf.FloorToInt(healAmount * 0.9f);
        int maxHeal = Mathf.FloorToInt(healAmount * 1.1f);

        // Zabezpieczenie: ¿eby nie zadaæ 0 lub ujemnych obra¿eñ
        if (minHeal < 1) minHeal = 1;

        // Random.Range dla int jest ekskluzywne dla górnej granicy, wiêc dajemy +1
        int healRandom = Random.Range(minHeal, maxHeal + 1);

        // Wykonujemy leczenie
        playerUnit.Heal(healRandom);

        // Aktualizujemy pasek ¿ycia i liczby
        playerHUD.SetHP(playerUnit.currentHP);

        // Wyœwietlamy graczowi konkretn¹ liczbê uleczonych punktów
        dialogueText.text = "Odzyskujesz " + healRandom + " HP!";

        yield return WaitForInputOrTime(0.5f);

        StartCoroutine(EnemyTurn());
    }

    IEnumerator EnemyTurn()
    {
        dialogueText.text = enemyUnit.unitName + " atakuje!";

        yield return WaitForInputOrTime(0.5f);

        // 1. Wróg losuje obra¿enia
        int dmg = enemyUnit.CalculateDamage();

        // 2. Gracz otrzymuje obra¿enia
        bool isDead = playerUnit.TakeDamage(dmg);

        // 3. Aktualizacja UI gracza
        playerHUD.SetHP(playerUnit.currentHP);

        dialogueText.text = enemyUnit.unitName + " zada³ Ci " + dmg + " obra¿eñ!";

        yield return WaitForInputOrTime(0.5f);

        if (isDead)
        {
            state = BattleState.LOST;
            EndBattle();
        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    // --- KONIEC WALKI ---

    void EndBattle()
    {
        StartCoroutine(EndBattleRoutine());
    }

    IEnumerator EndBattleRoutine()
    {
        if (state == BattleState.WON)
        {
            if (AudioManager.instance != null)
                AudioManager.instance.StopMusic();

            if (victorySound != null && AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX(victorySound);
            }
            dialogueText.text = "Wygra³eœ walkê!";
            yield return WaitForInputOrTime(1.5f);

            // --- SYSTEM NAGRÓD (POPRAWIONY) ---



            // 1. Pobieramy XP
            int xpReward = BattleTransitionManager.enemyToFight.xpReward;
            int goldReward = BattleTransitionManager.enemyToFight.goldReward;
            playerDataSource.AddGold(goldReward);

            dialogueText.text = "Zdobywasz " + goldReward + " Z³ota!";
            yield return WaitForInputOrTime(1.5f);
            // 2. Dodajemy XP do puli
            playerDataSource.currentXP += xpReward;
            dialogueText.text = "Zdobywasz " + xpReward + " XP!";
            yield return WaitForInputOrTime(1.5f);

            // 3. Sprawdzamy czy wbiliœmy poziom (Pêtla while - obs³uguje wbicie kilku lvli naraz)
            while (playerDataSource.currentXP >= playerDataSource.xpToNextLevel)
            {
                // Odejmujemy koszt poziomu
                playerDataSource.currentXP -= playerDataSource.xpToNextLevel;

                // Zwiêkszamy Level
                playerDataSource.unitLevel++;

                // --- TUTAJ JEST TO SKALOWANIE (x1.2) ---
                // Stary próg * 1.2. Np. 100 -> 120 -> 144...
                playerDataSource.xpToNextLevel = Mathf.RoundToInt(playerDataSource.xpToNextLevel * 2f);

                // Zwiêkszamy statystyki
                playerDataSource.maxHP += 5;
                playerDataSource.damage += 1;

                // Uleczamy gracza na full po awansie (Opcjonalnie)
                playerDataSource.currentHP = playerDataSource.maxHP;
                playerUnit.currentHP = playerDataSource.maxHP; // Aktualizujemy te¿ jednostkê na scenie
                playerHUD.SetHP(playerUnit.currentHP);

                // Info dla gracza
                dialogueText.text = "AWANS! Poziom " + playerDataSource.unitLevel + "!";
                yield return WaitForInputOrTime(1.5f);
                dialogueText.text = "Statystyki wzros³y!";
                yield return WaitForInputOrTime(1f);
            }

            // --- ZAPIS STANU ---
            // Zapisujemy aktualne HP gracza do ScriptableObject
            playerDataSource.currentHP = playerUnit.currentHP;

            // Resetujemy flagê ucieczki
            BattleTransitionManager.justRanAway = false;

            // Dodajemy wroga do listy pokonanych
            BattleTransitionManager.defeatedEnemies.Add(BattleTransitionManager.currentEnemyID);

            // Wracamy na mapê
            SceneManager.LoadScene("MapScene");
        }
        else if (state == BattleState.LOST)
        {
            if (AudioManager.instance != null)
                AudioManager.instance.StopMusic();

            if (defeatSound != null && AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX(defeatSound);
            }

            int lostGold = playerDataSource.LoseGoldOnDeath();
            dialogueText.text = "Zosta³eœ pokonany...";
            yield return WaitForInputOrTime(1f);
            dialogueText.text = $"Straci³eœ {lostGold} sztuk z³ota.";
            yield return WaitForInputOrTime(1f);

            BattleTransitionManager.isReturningFromBattle = false;

            // Uleczamy gracza przed restartem, ¿eby nie zaczyna³ gry martwy
            playerDataSource.currentHP = 1;

            SceneManager.LoadScene("MapScene");
        }
    }

    // Funkcja, która czeka X sekund, ALE przerywa natychmiast po klikniêciu mysz¹
    IEnumerator WaitForInputOrTime(float duration)
    {
        float timer = 0f;

        // Pêtla dzia³a dopóki czas nie min¹³
        while (timer < duration)
        {
            // Jeœli wciœniêto Lewy Przycisk Myszy (0) lub Spacjê
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                // Przerywamy czekanie (pomijamy resztê czasu)
                yield break;
            }

            // Dodajemy czas, jaki min¹³ od ostatniej klatki
            timer += Time.deltaTime;

            // Czekamy jedn¹ klatkê
            yield return null;
        }
    }
}