using UnityEngine;

public class GridManager
{
    private GameObject[] cubePrefabs;
    private GameObject boxPrefab;
    private GameObject stonePrefab;
    private GameObject vasePrefab;
    private Transform gridParent;         // Container for the cubes (world space)
    private float cellSize;               // Size of each cell
    private float ySpacing;               // Extra space only along the Y axis
    private RectTransform backgroundRect; // Background UI RectTransform

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
        this.ySpacing = ySpacing;    // We'll only apply this spacing vertically
        this.backgroundRect = backgroundRect;
    }

    public void CreateGrid(LevelData data)
    {
        int width = data.grid_width;
        int height = data.grid_height;

        // Offsets to center the grid in the background.
        // X offset uses just cellSize (no extra spacing),
        // Y offset uses (cellSize + ySpacing).
        float offsetX = (width - 1) * 0.5f * cellSize;
        float offsetY = (height - 1) * 0.5f * (cellSize + ySpacing);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                string code = data.grid[index];

                // X uses cellSize alone, Y uses (cellSize + ySpacing).
                float posX = x * cellSize - offsetX;
                float posY = y * (cellSize + ySpacing) - offsetY;

                // Convert to local UI space vector.
                Vector2 localPos = new Vector2(posX, posY);

                // Convert from UI space to world space using backgroundRect.
                Vector3 worldPos = backgroundRect.TransformPoint(localPos);

                GameObject item = InstantiateItem(code, worldPos);
                if (item != null)
                {
                    item.transform.SetParent(gridParent, true);
                }
            }
        }
    }

    private GameObject InstantiateItem(string code, Vector3 pos)
    {
        GameObject prefabToInstantiate = null;
        switch (code)
        {
            case "r": prefabToInstantiate = cubePrefabs[0]; break;
            case "g": prefabToInstantiate = cubePrefabs[1]; break;
            case "b": prefabToInstantiate = cubePrefabs[2]; break;
            case "y": prefabToInstantiate = cubePrefabs[3]; break;
            case "rand":
                prefabToInstantiate = cubePrefabs[Random.Range(0, cubePrefabs.Length)];
                break;
            case "bo": prefabToInstantiate = boxPrefab; break;
            case "s": prefabToInstantiate = stonePrefab; break;
            case "v": prefabToInstantiate = vasePrefab; break;
            default: return null;
        }

        return Object.Instantiate(prefabToInstantiate, pos, Quaternion.identity);
    }
}
