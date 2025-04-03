using UnityEngine;

public class Vase : Obstacle
{
    public override bool CanFall => true;

    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite crackedSprite;

    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        maxHealth = 2;
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetNormalVisual();
    }

    public override void TakeDamage(bool isFromRocket)
    {
        base.TakeDamage(isFromRocket);

        if (currentHealth == 1)
        {
            SetCrackedVisual();
        }
    }

    private void SetNormalVisual()
    {
        if (spriteRenderer != null && normalSprite != null)
            spriteRenderer.sprite = normalSprite;
    }

    private void SetCrackedVisual()
    {
        if (spriteRenderer != null && crackedSprite != null)
            spriteRenderer.sprite = crackedSprite;
    }

    public override void OnTapped() { }
}