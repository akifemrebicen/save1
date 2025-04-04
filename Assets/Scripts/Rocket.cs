using UnityEngine;
using System.Collections.Generic;

public class Rocket : GridItem
{
    public enum RocketDirection { Vertical, Horizontal }
    
    private RocketDirection direction;
    private GridManager gridManager;
    private Transform gridParent;
    private bool hasBeenTapped = false; // Tekrar tıklamayı engellemek için

    public override bool CanFall => true;

    public void Initialize(RocketDirection rocketDirection, GridManager manager, Transform parent)
    {
        direction = rocketDirection;
        gridManager = manager;
        gridParent = parent;
    }

    public override void OnTapped()
    {
        if (hasBeenTapped)
            return; // Zaten işlemdeyse tekrar işlem yapma
        
        hasBeenTapped = true;

        // Mevcut grid'deki tüm item'ları snapshot olarak alıyoruz.
        List<GridItem> currentItems = gridManager.GetAllItems();
        HashSet<GridItem> existingItems = new HashSet<GridItem>(currentItems);

        // Normal bölünmeyi gerçekleştiriyoruz (combo logic varsa onu buraya ekleyebilirsiniz).
        SplitRocket(existingItems);
    }

    private void SplitRocket(HashSet<GridItem> existingItems)
    {
        Vector2Int[] splitDirs = (direction == RocketDirection.Vertical)
            ? new Vector2Int[] { Vector2Int.up, Vector2Int.down }
            : new Vector2Int[] { Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int moveDir in splitDirs)
        {
            CreateRocketHalf(moveDir, existingItems);
        }

        // Grid'den bu rocket'i kaldır ve yok et.
        gridManager.RemoveGridItemAt(GridPosition);
        Destroy(gameObject);
    }

    private void CreateRocketHalf(Vector2Int moveDir, HashSet<GridItem> existingItems)
    {
        if (RocketManager.Instance != null)
        {
            RocketManager.Instance.CreateRocketHalf(GridPosition, moveDir, existingItems);
        }
    }

    public RocketDirection GetDirection() => direction;
}