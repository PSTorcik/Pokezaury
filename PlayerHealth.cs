using UnityEngine;


public class PlayerHealth:MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 100;

    public PlayerHealth(int currentHealth, int maxHealth)
    {
        this.currentHealth = currentHealth;
        this.maxHealth = maxHealth;
    }


    public void ChangeHealth(int amount)
    {
        currentHealth += amount;

        if (currentHealth <= 0)
        {
            Die();
            gameObject.SetActive(false);
        }
    }

    public void Die()
    {
        currentHealth = 0;
        Debug.Log("Player has died.");
    }
}
