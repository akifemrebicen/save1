using UnityEngine;

public abstract class GridItem : MonoBehaviour
{
    public Vector2Int GridPosition { get; set; }

    public virtual bool CanFall => false;

    public abstract void OnTapped();

    public virtual void TakeDamage(bool isFromRocket) { }
}