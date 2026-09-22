using UnityEngine;

public class EssentialsLoader : MonoBehaviour
{
    public GameObject essentialsPrefab;

    void Awake()
    {
        // Sprawdzamy czy Szef (GlobalControl) ju¿ istnieje w grze
        if (GlobalControl.instance == null)
        {
            // Jeœli nie ma -> Tworzymy go
            Instantiate(essentialsPrefab);
            // Nie musimy tu pisaæ DontDestroyOnLoad, bo GlobalControl sam to zrobi w swoim Awake()
        }
    }
}