using UnityEngine;

public abstract class Obstacle : GridItem
{
    protected int maxHealth = 1;
    protected int currentHealth;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public override void TakeDamage(bool isFromRocket)
    {
        currentHealth--;

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}