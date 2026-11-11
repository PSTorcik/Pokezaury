using UnityEngine;

public class StarterEgg : MonoBehaviour
{
    [Header("Konfiguracja Startera")]
    public EnemyData starterData;
    [TextArea(3, 5)]
    public string professorDescription;

    [Header("Ustawienia Interakcji")]
    public float interactionDistance = 1.5f;

    // Zmieniliœmy typ zmiennej na Twój konkretny skrypt gracza
    private PlayerMovement player;

    void Update()
    {
        // Pancerne szukanie gracza w ka¿dej klatce, dopóki go nie znajdzie
        if (player == null)
        {
            player = FindObjectOfType<PlayerMovement>();

            // Jeœli gracza nadal nie ma w tej klatce, przerywamy Update i unikamy b³êdu
            if (player == null) return;
        }

        // Obliczamy dystans. 'player.transform.position' jest w 100% bezpieczne.
        float distance = Vector2.Distance(transform.position, player.transform.position);
        Debug.Log($"[{gameObject.name}] Dystans do gracza: {distance}");


        if (distance <= interactionDistance)
        {
            if (GameNotifications.instance != null)
            {
                GameNotifications.instance.ShowMessage($"[E] Zbadaj jajko: {starterData.enemyName}", 0.5f);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (StarterSelectionUI.instance != null && !StarterSelectionUI.instance.panel.activeSelf)
                {
                    StarterSelectionUI.instance.OpenPanel(starterData, professorDescription);
                }
            }
        }
    }
}