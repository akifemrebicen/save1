using UnityEngine;

public class Box : Obstacle
{
    public override bool CanFall => false;

    protected override void Awake()
    {
        maxHealth = 1;
        base.Awake();
    }

    public override void OnTapped()
    {
        // Tıklamayla değil, patlamayla etkileşir
    }
}