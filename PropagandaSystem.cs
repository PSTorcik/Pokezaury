using UnityEngine;

public class PropagandaSystem : MonoBehaviour
{
    public PlayerData playerData;
    public float minTime = 10f;
    public float maxTime = 30f;
    private float timer;
    public AudioClip gongSound;
    public AudioClip happySound;

    // Lista "dobrych" wiadomoœci
    string[] goodMessages = {
        "+15 Social Credit! Dobra praca Obywatelu!",
        "+50 Social Credit! Chwa³a Partii!",
        "Wielki Brat patrzy z uznaniem. (+20 SC)",
        "Twoja lojalnoœæ zosta³a odnotowana.",
        "Nic siê nie wydarzy³o na Placu Tiananmen w 1989."
    };

    // Lista "z³ych" wiadomoœci (rzadsze)
    string[] badMessages = {
        "-30 Social Credit! Podejrzane zachowanie.",
        "-100 Social Credit! Deportacja do Korei Pó³nocnej nieunikniona.",
        "Zakaz chodzenia w butach",
        "Zakaz Kubusia Puchatka!",
        "Minus jeden talerz ry¿u",
        "Podobno utrzymujesz kontakty z niejakim The Wokiem"
    };

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        // Jeœli opcja jest wy³¹czona - nic nie rób
        if (!playerData.allowChinesePropaganda) return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            TriggerPropaganda();
            ResetTimer();
        }
    }

    void ResetTimer()
    {
        timer = Random.Range(minTime, maxTime);
    }

    void TriggerPropaganda()
    {
        bool isGood = Random.value > 0.3f;
        string message = "";

        if (isGood)
        {
            message = goodMessages[Random.Range(0, goodMessages.Length)];
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX(happySound);
            if (GameNotifications.instance != null)
            {
                GameNotifications.instance.ShowMessage($"{message}", 3f);
            }
        }

        else
        {
            message = badMessages[Random.Range(0, badMessages.Length)];
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX(gongSound);
            if (GameNotifications.instance != null)
            {
                GameNotifications.instance.ShowMessage($"<color=red>{message}</color>", 3f);
            }
        }
    }
}