using UnityEngine;

public class Cube : GridItem
{
    public enum ColorType { Red, Green, Blue, Yellow }

    private ColorType cubeColor;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hintedSprite;

    private SpriteRenderer spriteRenderer;
    private bool isHinted = false;

    public override bool CanFall => true;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetNormalForm();
    }

    public ColorType GetColor() => cubeColor;

    public void SetHintedForm()
    {
        isHinted = true;
        if (hintedSprite != null)
            spriteRenderer.sprite = hintedSprite;
    }

    public void SetNormalForm()
    {
        isHinted = false;
        if (normalSprite != null)
            spriteRenderer.sprite = normalSprite;
    }

    public override void OnTapped()
    {
        Debug.Log($"Tapped cube at {GridPosition} with color {cubeColor}");
        // Patlatma mantığı burada olacak
    }

    public override void TakeDamage(bool isFromRocket)
    {
        Destroy(gameObject);
    }
}