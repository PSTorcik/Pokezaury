using UnityEngine;

public class Elevation_Entry : MonoBehaviour
{
    public Collider2D[] mountainColliders;
    public Collider2D[] boundaryColliders;


    // TO JEST NOWO��:
    // Funkcja Start uruchamia si� przy ka�dym za�adowaniu sceny (r�wnie� po walce)
    private void Start()
    {
        // Je�li Manager m�wi, �e jeste�my na g�rze...
        if (BattleTransitionManager.isOnElevation)
        {
            SetElevationState(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Zapisujemy stan w Managerze
            BattleTransitionManager.isOnElevation = true;

            SetElevationState(true);

            // Ustawiamy warstw� wizualn� gracza (�eby sta� "nad" traw�)
            collision.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 10;
        }
    }

    // Pomocnicza funkcja, �eby nie pisa� tego samego dwa razy
    void SetElevationState(bool isUp)
    {
        // Je�li jeste�my na g�rze (isUp = true), wy��czamy �ciany (false)
        // Je�li jeste�my na dole (isUp = false), w��czamy �ciany (true)
        foreach (Collider2D collider in mountainColliders)
        {
            collider.enabled = !isUp;
        }

        // Barierki dzia�aj� odwrotnie do �cian
        foreach (Collider2D boundary in boundaryColliders)
        {
            boundary.enabled = isUp;
        }
    }
}