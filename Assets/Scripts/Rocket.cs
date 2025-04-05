using UnityEngine;
using System.Collections.Generic;

public class Rocket : GridItem
{
    public enum RocketDirection { Vertical, Horizontal }
    
    private RocketDirection direction;
    private GridManager gridManager;
    private Transform gridParent;

    public override bool CanFall => true;

    // Grid pozisyonunu da alıyoruz.
    public void Initialize(RocketDirection rocketDirection, GridManager manager, Transform parent, Vector2Int gridPos)
    {
        direction = rocketDirection;
        gridManager = manager;
        gridParent = parent;
        GridPosition = gridPos; // Roketin grid pozisyonunu atıyoruz.
    }

    public override void OnTapped()
    {
        Debug.Log($"Rocket tapped at grid position: {GridPosition}");
        List<GridItem> currentItems = gridManager.GetAllItems();
        HashSet<GridItem> existingItems = new HashSet<GridItem>(currentItems);

        SplitRocket(existingItems);

        // Patlama sonrası işlemleri, 0.5 sn delay ile tetikleyelim.
        LevelSceneManager.Instance.TriggerPostExplosionDelayed(0.5f);
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

        gridManager.RemoveGridItemAt(GridPosition);
        Destroy(gameObject, 0.1f);
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