using UnityEngine;
using DG.Tweening; // DOTween kullanımı için

public class Cube : GridItem
{
    public enum ColorType { Red, Green, Blue, Yellow }

    private ColorType cubeColor;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hintedSprite;
    [SerializeField] private GameObject crackEffect;
    [SerializeField] private GameObject lightEffect;

    private SpriteRenderer spriteRenderer;

    public override bool CanFall => true;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetNormalForm();
    }

    public ColorType GetColor() => cubeColor;

    public void SetColor(ColorType color)
    {
        cubeColor = color;
    }

    public void SetHintedForm()
    {
        if (hintedSprite != null)
            spriteRenderer.sprite = hintedSprite;
    }

    public void SetNormalForm()
    {
        if (normalSprite != null)
            spriteRenderer.sprite = normalSprite;
    }

    public override void OnTapped()
    {
        // Tweenleri temizleyelim ki, yok edilen nesnelerde tween kalmasın.
        transform.DOKill();
        Debug.Log($"Tapped cube at {GridPosition} with color {cubeColor}");
        if (crackEffect != null)
        {
            Instantiate(crackEffect, transform.position, Quaternion.identity);
        }
        if (lightEffect != null)
        {
            Instantiate(lightEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    public override void TakeDamage(bool isFromRocket)
    {
        transform.DOKill();
        Destroy(gameObject);
    }
}