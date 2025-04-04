using UnityEngine;
using System.Collections.Generic;

public class RocketManager
{
    // Static instance for easy access from Rocket.cs.
    public static RocketManager Instance { get; private set; }

    // Full rocket prefabs.
    private GameObject verticalRocketPrefab;
    private GameObject horizontalRocketPrefab;

    // Rocket half prefabs.
    private GameObject verticalUpHalfPrefab;
    private GameObject verticalDownHalfPrefab;
    private GameObject horizontalLeftHalfPrefab;
    private GameObject horizontalRightHalfPrefab;

    private GridManager gridManager;
    private Transform gridParent;

    // Constructor: All prefab references and dependencies are provided from LevelSceneManager.
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

        // Set the static instance for access from other classes.
        Instance = this;
    }

    // Creates a full rocket at a given grid position with the specified direction.
    public void CreateRocket(Vector2Int gridPos, Rocket.RocketDirection direction)
    {
        GameObject prefab = (direction == Rocket.RocketDirection.Vertical)
            ? verticalRocketPrefab
            : horizontalRocketPrefab;

        Vector3 worldPos = gridManager.GetWorldPosition(gridPos);
        GameObject rocketGO = Object.Instantiate(prefab, worldPos, Quaternion.identity, gridParent);
        Rocket rocket = rocketGO.GetComponent<Rocket>();
        if (rocket != null)
        {
            rocket.Initialize(direction, gridManager, gridParent);
            gridManager.SetGridItemAt(gridPos, rocket);
        }
    }

    // Creates a rocket half at a given grid position and movement direction,
    // and passes the snapshot of existing items to the half.
    public void CreateRocketHalf(Vector2Int gridPos, Vector2Int moveDirection, HashSet<GridItem> existingItems)
    {
        GameObject prefab = GetRocketHalfPrefab(moveDirection);
        if (prefab == null)
            return;

        Vector3 worldPos = gridManager.GetWorldPosition(gridPos);
        GameObject halfGO = Object.Instantiate(prefab, worldPos, Quaternion.identity, gridParent);
        RocketHalf half = halfGO.GetComponent<RocketHalf>();
        if (half != null)
        {
            half.Initialize(moveDirection, gridManager, existingItems);
        }
    }

    // Returns the correct half prefab based on the move direction.
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
