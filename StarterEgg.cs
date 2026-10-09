using UnityEngine;

public class StarterEgg : MonoBehaviour
{
    public EnemyData starterData; // Wrzuæ tu np. dane Ognistego, Wodnego lub Trawiastego stworka

    [TextArea]
    public string professorDescription; // Np. "Prof. Kasztan: To Ogniozaur. Jest porywczy. Chcesz go?"

    private bool isPlayerClose;

    void Update()
    {
        // Jeœli gracz jest blisko, nacisn¹³ E, a panel wyboru jeszcze nie jest otwarty
        if (isPlayerClose && Input.GetKeyDown(KeyCode.E) && !StarterSelectionUI.instance.panel.activeSelf)
        {
            StarterSelectionUI.instance.OpenPanel(starterData, professorDescription);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerClose = true;
            // Opcjonalnie: Poka¿ powiadomienie "[E] Zbadaj Jajo"
            if (InteractionUI.instance != null) InteractionUI.instance.ShowPrompt($"[E] Zbadaj jajko: {starterData.enemyName}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerClose = false;
            if (InteractionUI.instance != null) InteractionUI.instance.HidePrompt();
        }
    }
}