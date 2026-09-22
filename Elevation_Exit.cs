using UnityEngine;

public class Elevation_Exit : MonoBehaviour
{
    public Collider2D[] mountainColliders;
    public Collider2D[] boundaryColliders;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // NOWO��: Zapisujemy, �e zeszli�my na d�
            BattleTransitionManager.isOnElevation = false;

            foreach (Collider2D collider in mountainColliders)
            {
                collider.enabled = true; // W��czamy �ciany z powrotem
            }
            foreach (Collider2D boundary in boundaryColliders)
            {
                boundary.enabled = false; // Wy��czamy barierki
            }

            // Resetujemy warstw� wizualn�
            collision.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 3;
        }
    }
}
