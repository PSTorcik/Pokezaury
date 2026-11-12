using UnityEngine;

public class StarterEgg : MonoBehaviour
{
    [Header("Konfiguracja Startera")]
    public EnemyData starterData;
    [TextArea(3, 5)]
    public string professorDescription;

    [Header("Ustawienia Interakcji")]
    public float interactionDistance = 1.5f;

    private PlayerMovement player;

    void Update()
    {
        // 1. NAJPIERW szukamy gracza, jeœli zmienna jest pusta
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerMovement>();

            // Jeœli gracza nadal nie ma w tej klatce, przerywamy Update i unikamy b³êdu
            if (player == null) return;
        }

        // 2. DOPIERO GDY MAMY GRACZA: sprawdzamy, czy ma ju¿ startera w dru¿ynie
        if (player.playerData != null && player.playerData.team.Count > 0)
        {
            Destroy(gameObject);
            return;
        }

        // 3. Obliczamy dystans
        float distance = Vector2.Distance(transform.position, player.transform.position);

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