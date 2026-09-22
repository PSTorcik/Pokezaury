using UnityEngine;

public class GlobalControl : MonoBehaviour
{
    // Statyczna zmienna - "Wizytówka" jedynego prawowitego Essentials
    public static GlobalControl instance;

    void Awake()
    {
        // 1. Sprawdzamy, czy "Szef" ju¿ istnieje
        if (instance != null)
        {
            // Jeœli Szef ju¿ jest, a ja w³aœnie powsta³em -> jestem niepotrzebnym klonem.
            // Zniszcz mnie natychmiast, zanim zd¹¿ê cokolwiek zepsuæ/wyœwietliæ.
            Destroy(gameObject);
            return;
        }

        // 2. Jeœli Szefa nie ma -> Ja zostajê Szefem.
        instance = this;

        // 3. I nie pozwól mnie zniszczyæ przy zmianie sceny
        DontDestroyOnLoad(gameObject);
    }
}