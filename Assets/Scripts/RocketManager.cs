using UnityEngine;
using System.Collections.Generic;

public class RocketManager
{
    public static RocketManager Instance { get; private set; }

    private GameObject verticalRocketPrefab;
    private GameObject horizontalRocketPrefab;

    private GameObject verticalUpHalfPrefab;
    private GameObject verticalDownHalfPrefab;
    private GameObject horizontalLeftHalfPrefab;
    private GameObject horizontalRightHalfPrefab;

    private GridManager gridManager;
    private Transform gridParent;

    public RocketManager(
        GameObject verticalRocketPrefab,
        GameObject horizontalRocketPrefab,
        GameObject verticalUpHalfPrefab,
        GameObject verticalDownHalfPrefab,
        GameObject horizontalLeftHalfPrefab,
        GameObject horizontalRightHalfPrefab,
        GridManager gridManager,
        Transform gridParent)
    {
        this.verticalRocketPrefab = verticalRocketPrefab;
        this.horizontalRocketPrefab = horizontalRocketPrefab;
        this.verticalUpHalfPrefab = verticalUpHalfPrefab;
        this.verticalDownHalfPrefab = verticalDownHalfPrefab;
        this.horizontalLeftHalfPrefab = horizontalLeftHalfPrefab;
        this.horizontalRightHalfPrefab = horizontalRightHalfPrefab;
        this.gridManager = gridManager;
        this.gridParent = gridParent;

        Instance = this;
    }

    public void CreateRocket(Vector2Int gridPos, Rocket.RocketDirection direction)
    {
        Debug.Log($"Created rocket at {gridPos}");
        GameObject prefab = (direction == Rocket.RocketDirection.Vertical)
            ? verticalRocketPrefab
            : horizontalRocketPrefab;

        Vector3 worldPos = gridManager.GetWorldPosition(gridPos);
        GameObject rocketGO = Object.Instantiate(prefab, worldPos, Quaternion.identity, gridParent);
        Rocket rocket = rocketGO.GetComponent<Rocket>();
        if (rocket != null)
        {
            rocket.Initialize(direction, gridManager, gridParent, gridPos);
            gridManager.SetGridItemAt(gridPos, rocket);
        }
    }

    public void CreateRocketHalf(Vector2Int gridPos, Vector2Int moveDirection, HashSet<GridItem> existingItems)
    {
        Debug.Log($"Created rocket half at {gridPos} moving {moveDirection}");
        GameObject prefab = GetRocketHalfPrefab(moveDirection);
        if (prefab == null)
            return;

        Vector3 worldPos = gridManager.GetWorldPosition(gridPos);
        GameObject halfGO = Object.Instantiate(prefab, worldPos, Quaternion.identity, gridParent);
        RocketHalf half = halfGO.GetComponent<RocketHalf>();
        if (half != null)
        {
            half.Initialize(moveDirection, gridManager, gridPos);
        }
    }

    private GameObject GetRocketHalfPrefab(Vector2Int moveDirection)
    {
        if (moveDirection == Vector2Int.up)
            return verticalUpHalfPrefab;
        if (moveDirection == Vector2Int.down)
            return verticalDownHalfPrefab;
        if (moveDirection == Vector2Int.left)
            return horizontalLeftHalfPrefab;
        if (moveDirection == Vector2Int.right)
            return horizontalRightHalfPrefab;
        return null;
    }
}
