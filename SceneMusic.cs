using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    [Header("Dane")]
    public PlayerData playerData; // WA¯NE: Przeci¹gnij tu plik HeroData!

    [Header("Muzyka dla tej sceny")]
    public AudioClip normalMusic;       // Muzyka domyœlna (np. las)
    public AudioClip propagandaMusic;   // Muzyka "specjalna" (np. Red Sun in the Sky)

    void Start()
    {
        // Bezpiecznik: jeœli nie ma Managera Audio, nie rób nic
        if (AudioManager.instance == null) return;

        AudioClip trackToPlay = normalMusic; // Domyœlnie wybieramy zwyk³¹

        // --- WARUNEK PROPAGANDY ---
        // 1. Czy mamy podpiête dane gracza?
        // 2. Czy gracz zaznaczy³ "ptaszka" w opcjach?
        // 3. Czy w ogóle przypisa³eœ muzykê propagandow¹ do tego pola?
        if (playerData != null && playerData.allowChinesePropaganda && propagandaMusic != null)
        {
            // Jeœli wszystko na TAK -> podmieniamy utwór
            trackToPlay = propagandaMusic;
        }

        // --- ODTWARZANIE ---
        if (trackToPlay != null)
        {
            AudioManager.instance.PlayMusic(trackToPlay);
        }
    }
}