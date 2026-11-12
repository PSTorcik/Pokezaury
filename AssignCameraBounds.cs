using UnityEngine;
using Unity.Cinemachine; // Lub 'using Cinemachine;' w zale¿noœci od wersji

public class AssignCameraBounds : MonoBehaviour
{
    void Start()
    {
        // Szukamy komponentu granicz¹cego w za³adowanej kamerze
        CinemachineConfiner2D confiner = FindObjectOfType<CinemachineConfiner2D>();

        if (confiner != null)
        {
            // Przypinamy ten konkretny Polygon Collider do kamery
            PolygonCollider2D myCollider = GetComponent<PolygonCollider2D>();
            confiner.BoundingShape2D = myCollider;

            // Odœwie¿amy cache, ¿eby kamera nie pamiêta³a starej mapy
            confiner.InvalidateBoundingShapeCache();
        }
    }
}