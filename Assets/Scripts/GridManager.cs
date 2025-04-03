using System.Collections.Generic;
using UnityEngine;

public class GridManager
{
    private GameObject[] cubePrefabs;
    private GameObject boxPrefab;
    private GameObject stonePrefab;
    private GameObject vasePrefab;
    private Transform gridParent;
    private float cellSize;
    private float ySpacing;
    private RectTransform backgroundRect;

    // Tüm grid hücrelerini kapsayan 2D array (GridItem: Cube, box, vase, obstacle vs.)
    private GridItem[,] gridItems;
    private int gridWidth;
    private int gridHeight;

    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public GridItem[,] GridItems => gridItems;
    public float CellSize => cellSize;
    public float YSpacing => ySpacing;
    public RectTransform BackgroundRect => backgroundRect;
    
    public GridManager(
        GameObject[] cubes,
        GameObject box,
        GameObject stone,
        GameObject vase,
        Transform gridParent,
        float cellSize,
        float ySpacing,
        RectTransform backgroundRect
    )
    {
        cubePrefabs = cubes;
        boxPrefab = box;
        stonePrefab = stone;
        vasePrefab = vase;
        this.gridParent = gridParent;
        this.cellSize = cellSize;
        this.ySpacing = ySpacing;
        this.backgroundRect = backgroundRect;
    }

    public void CreateGrid(LevelData data)
    {
        gridWidth = data.grid_width;
        gridHeight = data.grid_height;
        gridItems = new GridItem[gridWidth, gridHeight];

        float offsetX = (gridWidth - 1) * 0.5f * cellSize;
        float offsetY = (gridHeight - 1) * 0.5f * (cellSize + ySpacing);

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                int index = y * gridWidth + x;
                string code = data.grid[index];

                float posX = x * cellSize - offsetX;
                float posY = y * (cellSize + ySpacing) - offsetY;
                Vector2 localPos = new Vector2(posX, posY);
                Vector3 worldPos = backgroundRect.TransformPoint(localPos);

                GameObject item = InstantiateItem(code, worldPos, new Vector2Int(x, y));
                if (item != null)
                {
                    item.transform.SetParent(gridParent, true);
                }
            }
        }
    }

    private GameObject InstantiateItem(string code, Vector3 pos, Vector2Int gridPos)
    {
        GameObject prefabToInstantiate = null;
        Cube.ColorType colorToAssign = Cube.ColorType.Red;

        switch (code)
        {
            case "r":
                prefabToInstantiate = cubePrefabs[0];
                colorToAssign = Cube.ColorType.Red;
                break;
            case "g":
                prefabToInstantiate = cubePrefabs[1];
                colorToAssign = Cube.ColorType.Green;
                break;
            case "b":
                prefabToInstantiate = cubePrefabs[2];
                colorToAssign = Cube.ColorType.Blue;
                break;
            case "y":
                prefabToInstantiate = cubePrefabs[3];
                colorToAssign = Cube.ColorType.Yellow;
                break;
            case "rand":
                int randIndex = Random.Range(0, cubePrefabs.Length);
                prefabToInstantiate = cubePrefabs[randIndex];
                colorToAssign = (Cube.ColorType)randIndex;
                break;
            case "bo":
                prefabToInstantiate = boxPrefab;
                break;
            case "s":
                prefabToInstantiate = stonePrefab;
                break;
            case "v":
                prefabToInstantiate = vasePrefab;
                break;
            default:
                return null;
        }

        GameObject obj = Object.Instantiate(prefabToInstantiate, pos, Quaternion.identity);
        GridItem gridItem = obj.GetComponent<GridItem>();
        if (gridItem != null)
        {
            gridItem.GridPosition = gridPos;
            Cube cube = gridItem as Cube;
            if (cube != null)
            {
                cube.SetColor(colorToAssign);
            }
            gridItems[gridPos.x, gridPos.y] = gridItem;
        }
        return obj;
    }

    public GridItem GetGridItemAt(Vector2Int pos)
    {
        if (pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight)
            return gridItems[pos.x, pos.y];
        return null;
    }

    public void RemoveGridItemAt(Vector2Int pos)
    {
        if (pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight)
        {
            gridItems[pos.x, pos.y] = null;
        }
    }

    public Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        float offsetX = (gridWidth - 1) * 0.5f * cellSize;
        float offsetY = (gridHeight - 1) * 0.5f * (cellSize + ySpacing);
        float posX = gridPos.x * cellSize - offsetX;
        float posY = gridPos.y * (cellSize + ySpacing) - offsetY;
        Vector2 localPos = new Vector2(posX, posY);
        return backgroundRect.TransformPoint(localPos);
    }
    
    public void SetGridItemAt(Vector2Int pos, GridItem item)
    {
        if (pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight)
        {
            gridItems[pos.x, pos.y] = item;
        }
    }
}