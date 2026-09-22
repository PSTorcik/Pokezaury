using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("èrÛd≥a DüwiÍku (Przypisz w Inspectorze)")]
    public AudioSource musicSource; // Kana≥ muzyki
    public AudioSource sfxSource;   // Kana≥ efektÛw

    // Zmienne do trzymania g≥oúnoúci
    private float musicVolume = 0.5f;
    private float sfxVolume = 0.5f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    // --- FUNKCJE DLA SUWAK”W ---

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        sfxSource.volume = sfxVolume;
    }

    // --- ODTWARZANIE MUZYKI (T£O) ---
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        // Sprytny warunek: Jeúli ta piosenka juø gra, nie resetuj jej!
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true; // Muzyka zawsze w pÍtli
        musicSource.volume = musicVolume; // Ustaw g≥oúnoúÊ suwaka
        musicSource.Play();
    }

    // --- ODTWARZANIE EFEKT”W (JEDNORAZOWE) ---
    // Tego uøywasz do krokÛw, atakÛw, klikniÍÊ
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        // PlayOneShot pozwala na nak≥adanie siÍ düwiÍkÛw (np. wybuch i krzyk naraz)
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }
}