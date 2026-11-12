using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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

    // Tekst dialogowy
    public TextMeshProUGUI dialogueText;

    [Header("Panel zmiany Pokezaura w walce")]
    public GameObject partySwitchPanel;       // Panel z 6 przyciskami dru¿yny
    public Button[] partySlotButtons;         // 6 przycisków slotów
    public TextMeshProUGUI[] partySlotTexts;  // Teksty na 6 przyciskach (nazwa + HP)
    public Image[] partySlotIcons;            // NOWE: 6 obrazków (Icon) wewn¹trz przycisków
    public GameObject cancelSwitchButton;     // Przycisk "Anuluj" (ukrywany, gdy zmiana jest wymuszona po œmierci)

    [Header("Debug / Testowanie")]
    public EnemyData debugEnemy;

    [Header("DŸwiêki")]
    public AudioClip victorySound;
    public AudioClip defeatSound;
    public AudioClip hitSound;

    // Œledzimy dok³adnie tego Pokezaura, który aktualnie stoi na arenie
    private Pokezaur activePokezaur;
    private bool isForcedSwitch = false;

    void Start()
    {
        if (partySwitchPanel != null)
            partySwitchPanel.SetActive(false);

        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        EnemyData enemyData = BattleTransitionManager.enemyToFight;

        if (enemyData == null)
        {
            if (debugEnemy != null)
            {
                Debug.LogWarning("TRYB TESTOWY: U¿ywam przeciwnika z pola DebugEnemy.");
                enemyData = debugEnemy;
                BattleTransitionManager.enemyToFight = debugEnemy;
            }
            else
            {
                Debug.LogError("B£¥D: Brak przeciwnika! Odpal grê od Mapy albo przypisz DebugEnemy.");
                dialogueText.text = "B³¹d: Brak danych wroga!";
                yield break;
            }
        }

        enemyUnit.SetupEnemy(enemyData);

        // Szukamy pierwszego ¿ywego Pokezaura i zapisujemy go w polu klasy activePokezaur
        activePokezaur = GetFirstAlivePokezaur();

        if (activePokezaur != null)
        {
            playerUnit.SetupPlayer(activePokezaur);
        }
        else
        {
            dialogueText.text = "Nie masz Pokezaurów zdolnych do walki!";
            yield return WaitForInputOrTime(1.5f);

            state = BattleState.LOST;
            EndBattle();
            yield break;
        }

        enemyHUD.SetHUD(enemyUnit);
        playerHUD.SetHUD(playerUnit);
        playerHUD.UpdateTeamUI(playerDataSource.team);

        dialogueText.text = "Dzikie " + enemyUnit.unitName + " atakuje!";
        yield return WaitForInputOrTime(0.5f);

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    private Pokezaur GetFirstAlivePokezaur()
    {
        foreach (Pokezaur poke in playerDataSource.team)
        {
            if (poke.currentHP > 0)
                return poke;
        }
        return null;
    }

    void PlayerTurn()
    {
        dialogueText.text = "Wybierz akcjê:";
    }

    // --- SYSTEM ZMIANY POKEZAURA W WALCE ---

    // Podepnij pod przycisk "DRU¯YNA / ZMIEÑ" w menu walki
    public void OnOpenPartyButton()
    {
        if (state != BattleState.PLAYERTURN) return;

        isForcedSwitch = false;
        OpenPartySwitchPanel();
    }

    private void OpenPartySwitchPanel()
    {
        if (partySwitchPanel == null) return;

        partySwitchPanel.SetActive(true);

        // Jeœli zmiana jest wymuszona (po omdleniu), nie pozwalamy zamkn¹æ okna przyciskiem "Anuluj"
        if (cancelSwitchButton != null)
            cancelSwitchButton.SetActive(!isForcedSwitch);

        // Odœwie¿amy wygl¹d 6 przycisków
        for (int i = 0; i < partySlotButtons.Length; i++)
        {
            if (i < playerDataSource.team.Count)
            {
                Pokezaur poke = playerDataSource.team[i];
                partySlotButtons[i].gameObject.SetActive(true);

                int displayHP = (poke == activePokezaur) ? playerUnit.currentHP : poke.currentHP;
                string statusTag = (poke == activePokezaur) ? " [WALCZY]" : (displayHP <= 0 ? " [ZEMDLA£]" : "");

                // Wyœwietlamy Nazwê, Poziom, HP oraz statystyki ATK i DEF
                partySlotTexts[i].text = $"<b>{poke.pokeName}</b> (Lv.{poke.level}){statusTag}\n" +
                                         $"HP: {displayHP}/{poke.maxHP} | Si³a: {poke.damage} | Obr: {poke.defense}";

                // Wyœwietlamy obrazek (przód) Pokezaura na przycisku
                if (partySlotIcons != null && i < partySlotIcons.Length && partySlotIcons[i] != null)
                {
                    partySlotIcons[i].enabled = (poke.sprite != null);
                    partySlotIcons[i].sprite = poke.sprite;
                }

                partySlotButtons[i].interactable = (displayHP > 0 && poke != activePokezaur);
            }
            else
            {
                partySlotButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // Podepnij pod przycisk "Anuluj" w panelu dru¿yny
    public void OnClosePartyButton()
    {
        if (isForcedSwitch) return; // Nie mo¿na anulowaæ, gdy obecny Pokezaur zemdla³
        if (partySwitchPanel != null)
            partySwitchPanel.SetActive(false);
    }

    // Podepnij pod 6 przycisków slotów (z argumentem int od 0 do 5)
    public void OnSelectPartyMember(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= playerDataSource.team.Count) return;

        Pokezaur chosenPoke = playerDataSource.team[slotIndex];
        if (chosenPoke.currentHP <= 0 || chosenPoke == activePokezaur) return;

        if (partySwitchPanel != null)
            partySwitchPanel.SetActive(false);

        StartCoroutine(SwitchPokezaurRoutine(chosenPoke));
    }

    IEnumerator SwitchPokezaurRoutine(Pokezaur newPokezaur)
    {
        state = BattleState.ENEMYTURN; // Blokujemy przyciski na czas animacji/komunikatu

        // 1. Zapisujemy HP dotychczasowego Pokezaura (jeœli jeszcze ¿yje)
        if (activePokezaur != null)
        {
            activePokezaur.currentHP = playerUnit.currentHP;
        }

        // 2. Ustawiamy nowego Pokezaura jako aktywnego
        activePokezaur = newPokezaur;
        playerUnit.SetupPlayer(activePokezaur);
        playerHUD.SetHUD(playerUnit);
        playerHUD.UpdateTeamUI(playerDataSource.team);

        dialogueText.text = $"Do boju, {activePokezaur.pokeName}!";
        yield return WaitForInputOrTime(1f);

        // 3. Jeœli to by³a zmiana wymuszona po œmierci, gracz dostaje swoj¹ turê.
        // Jeœli dobrowolna w trakcie tury — przechodzimy do tury wroga.
        if (isForcedSwitch)
        {
            isForcedSwitch = false;
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
        else
        {
            StartCoroutine(EnemyTurn());
        }
    }

    // --- FUNKCJE PRZYCISKÓW AKCJI ---

    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN) return;
        StartCoroutine(PlayerAttack());
    }

    public void OnHealButton()
    {
        if (state != BattleState.PLAYERTURN) return;
        StartCoroutine(PlayerHeal());
    }

    public void OnPotionButton()
    {
        if (state != BattleState.PLAYERTURN) return;

        ItemData potionToUse = null;
        foreach (InventorySlot slot in playerDataSource.inventory)
        {
            if (slot.item.itemType == ItemType.HealHP)
            {
                potionToUse = slot.item;
                break;
            }
        }

        if (potionToUse != null)
        {
            StartCoroutine(UseItemRoutine(potionToUse));
        }
        else
        {
            dialogueText.text = "Nie masz ¿adnych mikstur w plecaku!";
        }
    }

    public void OnRunButton()
    {
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

            // POPRAWKA: Zapisujemy HP dok³adnie tego Pokezaura, który aktualnie walczy³
            if (activePokezaur != null)
            {
                activePokezaur.currentHP = playerUnit.currentHP;
            }

            BattleTransitionManager.isReturningFromBattle = true;
            BattleTransitionManager.justRanAway = true;

            SceneManager.LoadScene("MapScene");
        }
        else
        {
            dialogueText.text = "Nie uda³o siê uciec!";
            yield return WaitForInputOrTime(1f);

            dialogueText.text = "Tracisz turê!";
            yield return WaitForInputOrTime(1f);

            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator UseItemRoutine(ItemData itemToUse)
    {
        state = BattleState.ENEMYTURN;

        playerDataSource.RemoveItem(itemToUse);
        dialogueText.text = $"U¿yto: {itemToUse.itemName}!";
        yield return WaitForInputOrTime(1f);

        if (itemToUse.itemType == ItemType.CatchItem)
        {
            dialogueText.text = "Rzucasz Jajem Pokezaura!";
            yield return WaitForInputOrTime(1f);

            if (Random.value <= 0.5f)
            {
                dialogueText.text = "Uda³o siê! Pokezaur z³apany!";
                yield return WaitForInputOrTime(1.5f);

                // POPRAWKA: Zapisujemy aktualne HP naszego walcz¹cego Pokezaura przed wyjœciem na mapê!
                if (activePokezaur != null)
                {
                    activePokezaur.currentHP = playerUnit.currentHP;
                }

                if (playerDataSource.team.Count < 6)
                {
                    Pokezaur newPoke = new Pokezaur(BattleTransitionManager.enemyToFight);
                    playerDataSource.team.Add(newPoke);
                    dialogueText.text = $"{newPoke.pokeName} do³¹cza do dru¿yny!";
                }
                else
                {
                    dialogueText.text = "Brak miejsca w dru¿ynie! (System PC wkrótce)";
                }

                yield return WaitForInputOrTime(1.5f);

                BattleTransitionManager.justRanAway = false;
                BattleTransitionManager.defeatedEnemies.Add(BattleTransitionManager.currentEnemyID);
                SceneManager.LoadScene("MapScene");
                yield break;
            }
            else
            {
                dialogueText.text = "O nie! Pokezaur siê uwolni³!";
                yield return WaitForInputOrTime(1f);
            }
        }
        else if (itemToUse.itemType == ItemType.HealHP)
        {
            playerUnit.Heal(itemToUse.value);
            playerHUD.SetHP(playerUnit.currentHP);

            // POPRAWKA: Zapisujemy uleczone HP do aktywnie walcz¹cego Pokezaura (a nie sztywno do team[0])
            if (activePokezaur != null)
            {
                activePokezaur.currentHP = playerUnit.currentHP;
            }

            dialogueText.text = $"+{itemToUse.value} HP!";
            yield return WaitForInputOrTime(1f);
        }

        StartCoroutine(EnemyTurn());
    }

    // --- LOGIKA WALKI ---

    IEnumerator PlayerAttack()
    {
        state = BattleState.ENEMYTURN;

        int dmg = playerUnit.CalculateDamage();
        bool isDead = enemyUnit.TakeDamage(dmg);
        enemyHUD.SetHP(enemyUnit.currentHP);

        dialogueText.text = "Zadajesz " + dmg + " pkt obra¿eñ!";

        if (hitSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(hitSound);
        }

        yield return WaitForInputOrTime(0.5f);

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

        int healAmount = (playerUnit.unitLevel * 2) + 6;
        int minHeal = Mathf.FloorToInt(healAmount * 0.9f);
        int maxHeal = Mathf.FloorToInt(healAmount * 1.1f);
        if (minHeal < 1) minHeal = 1;

        int healRandom = Random.Range(minHeal, maxHeal + 1);

        playerUnit.Heal(healRandom);
        playerHUD.SetHP(playerUnit.currentHP);

        // POPRAWKA: Synchronizujemy HP z obiektem aktywnego Pokezaura
        if (activePokezaur != null)
        {
            activePokezaur.currentHP = playerUnit.currentHP;
        }

        dialogueText.text = "Odzyskujesz " + healRandom + " HP!";
        yield return WaitForInputOrTime(0.5f);

        StartCoroutine(EnemyTurn());
    }

    IEnumerator EnemyTurn()
    {
        dialogueText.text = enemyUnit.unitName + " atakuje!";
        yield return WaitForInputOrTime(0.5f);

        int dmg = enemyUnit.CalculateDamage();
        bool isDead = playerUnit.TakeDamage(dmg);
        playerHUD.SetHP(playerUnit.currentHP);

        // Zapisujemy aktualne HP w obiekcie naszego Pokezaura
        if (activePokezaur != null)
        {
            activePokezaur.currentHP = playerUnit.currentHP;
        }

        dialogueText.text = enemyUnit.unitName + " zada³ Ci " + dmg + " obra¿eñ!";
        yield return WaitForInputOrTime(0.5f);

        if (isDead)
        {
            // Odœwie¿amy ikonki jajek (jedno jajko staje siê szare/martwe)
            playerHUD.UpdateTeamUI(playerDataSource.team);

            dialogueText.text = $"{activePokezaur.pokeName} pada nieprzytomny!";
            yield return WaitForInputOrTime(1.5f);

            // Sprawdzamy, czy mamy jeszcze jakiegokolwiek ¿ywego Pokezaura w dru¿ynie
            Pokezaur nextAlive = GetFirstAlivePokezaur();

            if (nextAlive != null)
            {
                // Jeœli mamy podpiêty panel wyboru dru¿yny, otwieramy go w trybie wymuszonym
                if (partySwitchPanel != null)
                {
                    dialogueText.text = "Wybierz kolejnego Pokezaura!";
                    isForcedSwitch = true;
                    OpenPartySwitchPanel();
                }
                else
                {
                    // Zabezpieczenie: jeœli nie zbudowa³eœ jeszcze panelu UI, automatycznie wypuszcza kolejnego ¿ywego!
                    isForcedSwitch = true;
                    yield return StartCoroutine(SwitchPokezaurRoutine(nextAlive));
                }
            }
            else
            {
                // Dopiero gdy WSZYSTKIE Pokezaury w dru¿ynie maj¹ 0 HP -> przegrywamy walkê
                state = BattleState.LOST;
                EndBattle();
            }
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
                AudioManager.instance.PlaySFX(victorySound);

            dialogueText.text = "Wygra³eœ walkê!";
            yield return WaitForInputOrTime(1.5f);

            int xpReward = BattleTransitionManager.enemyToFight.xpReward;
            int goldReward = BattleTransitionManager.enemyToFight.goldReward;
            playerDataSource.AddGold(goldReward);

            dialogueText.text = "Zdobywasz " + goldReward + " Z³ota!";
            yield return WaitForInputOrTime(1.5f);

            if (activePokezaur != null)
            {
                // Najpierw zapisujemy HP, z jakim skoñczy³ walkê
                activePokezaur.currentHP = playerUnit.currentHP;

                dialogueText.text = activePokezaur.pokeName + " zdobywa " + xpReward + " XP!";
                yield return WaitForInputOrTime(1.5f);

                bool leveledUp = activePokezaur.AddXP(xpReward);

                if (leveledUp)
                {
                    // POPRAWKA: Odœwie¿amy ca³e playerUnit i HUD (nowe maxHP, nowy level i pe³ne HP)
                    playerUnit.SetupPlayer(activePokezaur);
                    playerHUD.SetHUD(playerUnit);

                    dialogueText.text = "AWANS! Poziom " + activePokezaur.level + "!";
                    yield return WaitForInputOrTime(1.5f);
                    dialogueText.text = "Statystyki wzros³y!";
                    yield return WaitForInputOrTime(1f);
                }
            }

            BattleTransitionManager.justRanAway = false;
            BattleTransitionManager.defeatedEnemies.Add(BattleTransitionManager.currentEnemyID);
            SceneManager.LoadScene("MapScene");
        }
        else if (state == BattleState.LOST)
        {
            if (AudioManager.instance != null)
                AudioManager.instance.StopMusic();

            if (defeatSound != null && AudioManager.instance != null)
                AudioManager.instance.PlaySFX(defeatSound);

            int lostGold = playerDataSource.LoseGoldOnDeath();
            dialogueText.text = "Zosta³eœ pokonany...";
            yield return WaitForInputOrTime(1f);
            dialogueText.text = $"Straci³eœ {lostGold} sztuk z³ota.";
            yield return WaitForInputOrTime(1f);

            BattleTransitionManager.isReturningFromBattle = false;

            if (playerDataSource.team.Count > 0)
            {
                foreach (Pokezaur p in playerDataSource.team)
                {
                    p.currentHP = p.maxHP;
                }
            }

            SceneManager.LoadScene("MapScene");
        }
    }

    IEnumerator WaitForInputOrTime(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }
}