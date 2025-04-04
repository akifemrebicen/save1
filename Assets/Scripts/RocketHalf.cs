using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class RocketHalf : GridItem
{
    private Vector2Int moveDirection;
    private GridManager gridManager;
    private float moveSpeed = 10f; // Adjust speed as needed

    // The set of grid items that existed at the time of the rocket split.
    private HashSet<GridItem> existingItemsAtSplit;

    public override bool CanFall => true;

    // Initialize with the movement direction, gridManager, and snapshot set.
    public void Initialize(Vector2Int direction, GridManager manager, HashSet<GridItem> existingItems)
    {
        moveDirection = direction;
        gridManager = manager;
        existingItemsAtSplit = existingItems;
    }

    private void Update()
    {
        // Convert moveDirection (Vector2Int) to Vector3 and move.
        Vector3 moveVec = new Vector3(moveDirection.x, moveDirection.y, 0f) * (moveSpeed * Time.deltaTime);
        transform.Translate(moveVec);

        // Check if there is an existing grid item at our current location and blast it.
        TryBlastGridItem();

        // Destroy if the half rocket goes off-screen.
        CheckAndDestroyOutOfBounds();
    }

    private void TryBlastGridItem()
    {
        Vector2Int currentGridPos = GetGridPositionFromWorld(transform.position);
        GridItem item = gridManager.GetGridItemAt(currentGridPos);

        // Only blast if:
        // 1. There is an item.
        // 2. The item existed at the moment of splitting.
        // 3. The item is not another full rocket.
        if (item != null && existingItemsAtSplit.Contains(item) && !(item is Rocket))
        {
            BlastGridItem(item);
        }
    }

    private void BlastGridItem(GridItem item)
    {
        // Apply a subtle shrink animation (to 0.8 scale) before destruction.
        item.transform.DOScale(0.8f, 0.1f).OnComplete(() =>
        {
            gridManager.RemoveGridItemAt(item.GridPosition);
            Destroy(item.gameObject);
        });
    }

    // Converts world position to grid coordinates. Adjust as necessary for your grid.
    private Vector2Int GetGridPositionFromWorld(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x);
        int y = Mathf.RoundToInt(worldPos.y);
        return new Vector2Int(x, y);
    }

    private void CheckAndDestroyOutOfBounds()
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
        {
            Destroy(gameObject);
        }
    }

    public override void OnTapped()
    {
        // Rocket halves typically aren't tapped, but if they are, simply destroy them.
        Destroy(gameObject);
    }
}
