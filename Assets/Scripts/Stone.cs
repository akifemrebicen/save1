using UnityEngine;

public class Stone : Obstacle
{
    public override bool CanFall => false;

    protected override void Awake()
    {
        maxHealth = 1;
        base.Awake();
    }

    public override void TakeDamage(bool isFromRocket)
    {
        if (isFromRocket)
        {
            base.TakeDamage(true);
        }
        else
        {
            Debug.Log("Stone is immune to blast damage.");
        }
    }

    public override void OnTapped() { }
}