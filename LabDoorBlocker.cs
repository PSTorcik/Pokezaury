using UnityEngine;

public class LabDoorBlocker : MonoBehaviour
{
    public PlayerData playerData;

    void Update()
    {
        // Jeœli gracz ma ju¿ Pokezaura w dru¿ynie, blokada "wyparowuje"
        if (playerData.team.Count > 0)
        {
            gameObject.SetActive(false);
        }
    }

    // Blokada u¿ywa Collision, wiêc jest fizyczn¹ œcian¹
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && playerData.team.Count == 0)
        {
            if (GameNotifications.instance != null)
            {
                GameNotifications.instance.ShowMessage("Prof. Kasztan: Hej! Zanim wyruszysz, musisz wybraæ swojego partnera!", 4f);
            }
        }
    }
}