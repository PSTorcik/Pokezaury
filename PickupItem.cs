using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public ItemData item;         // Jaki przedmiot daje ten obiekt?
    public PlayerData playerData; // Gdzie go wrzuciæ? (Twój plik gracza)
    [Header("DŸwiêki")]
    public AudioClip pickupSound; // Tu wrzucisz fanfary

    // Zmienna prywatna - "Czy gracz stoi wystarczaj¹co blisko?"
    private bool isPlayerInRange = false;

    void Update()
    {
        // Sprawdzamy w ka¿dej klatce:
        // 1. Czy gracz jest blisko?
        // 2. Czy wcisn¹³ klawisz E?
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            PickUp();
        }
    }

    void PickUp()
    {
        // Logika dodawania do plecaka
        if (playerData != null && item != null)
        {
            playerData.AddItem(item);
            GameNotifications.instance.ShowMessage($"Podniesiono: {item.itemName}", 5f);
            if (pickupSound != null && AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX(pickupSound);
            }
        }

        // Niszczymy obiekt ze sceny
        Destroy(gameObject);
    }

    // --- FIZYKA (Wykrywanie obecnoœci) ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            GameNotifications.instance.ShowMessage($"{item.itemName}. Naciœnij [E], aby podnieœæ", 99f); // D³ugi czas, zniknie przy wyjœciu
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            GameNotifications.instance.ShowMessage("", 0.1f);
        }
    }
}