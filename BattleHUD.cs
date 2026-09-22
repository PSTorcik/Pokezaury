using UnityEngine;
using UnityEngine.UI; // WA¯NE: Wymagane do obs³ugi Text i Slider
using TMPro;
using System.Collections.Generic; //lista

public class BattleHUD : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public Slider hpSlider;

    // 1. NOWE: Pole na tekst z liczbami HP
    public TextMeshProUGUI hpText;

    // 2. NOWE: Zmienna prywatna do zapamiêtania Max HP
    private int _maxHP;

    [Header("Dru¿yna (Jajka)")]
    public Image[] teamEggs; // Tablica 6 obrazków reprezentuj¹cych jajka
    public Color aliveColor = new Color(1f, 1f, 1f, 1f); // Zwyk³y (Bia³y)
    public Color faintedColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Ciemny szary (Zemdla³)

    // Ta funkcja ustawi UI na samym pocz¹tku walki
    public void SetHUD(BattleUnit unit)
    {
        nameText.text = unit.unitName;
        levelText.text = "Lvl " + unit.unitLevel;

        hpSlider.maxValue = unit.maxHP;
        hpSlider.value = unit.currentHP;

        // 3. NOWE: Zapamiêtujemy MaxHP i ustawiamy tekst startowy
        _maxHP = unit.maxHP;
        hpText.text = unit.currentHP + " / " + _maxHP;
    }

    public void SetHP(int hp)
    {
        hpSlider.value = hp;

        // 4. NOWE: Aktualizujemy tekst u¿ywaj¹c nowej wartoœci i zapamiêtanego Max
        hpText.text = hp + " / " + _maxHP;
    }

    // --- NOWA FUNKCJA: Aktualizacja Ikon Jajek ---
    public void UpdateTeamUI(List<Pokezaur> team)
    {
        // Sprawdzamy wszystkie 6 miejsc na jajka
        for (int i = 0; i < teamEggs.Length; i++)
        {
            if (i < team.Count)
            {
                // Mamy Pokezaura na tym slocie
                teamEggs[i].gameObject.SetActive(true); // Pokazujemy jajko

                if (team[i].currentHP > 0)
                {
                    teamEggs[i].color = aliveColor; // ¯yje = jasne
                }
                else
                {
                    teamEggs[i].color = faintedColor; // Nie ¿yje = ciemne
                }
            }
            else
            {
                // Nie mamy Pokezaura na tym slocie (np. mamy tylko 3, a to jest slot 4)
                teamEggs[i].gameObject.SetActive(false); // Ukrywamy jajko
            }
        }
    }


}