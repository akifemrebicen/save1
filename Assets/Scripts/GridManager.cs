using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
    
    // Dictionary to keep track of active tweens
    private Dictionary<GridItem, Sequence> activeTweens = new Dictionary<GridItem, Sequence>();

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

    public List<Cube> FindConnectedCubes(Cube startCube)
    {
        List<Cube> connectedCubes = new List<Cube>();
        bool[,] visited = new bool[gridWidth, gridHeight];

        Stack<Cube> stack = new Stack<Cube>();
        stack.Push(startCube);
        visited[startCube.GridPosition.x, startCube.GridPosition.y] = true;

        Cube.ColorType targetColor = startCube.GetColor();

        while (stack.Count > 0)
        {
            Cube current = stack.Pop();
            connectedCubes.Add(current);

            Vector2Int pos = current.GridPosition;
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
            };

            foreach (var d in directions)
            {
                Vector2Int np = pos + d;
                if (np.x >= 0 && np.x < gridWidth && np.y >= 0 && np.y < gridHeight && !visited[np.x, np.y])
                {
                    GridItem gi = GetGridItemAt(np);
                    Cube neighbor = gi as Cube;
                    if (neighbor != null && neighbor.GetColor() == targetColor)
                    {
                        stack.Push(neighbor);
                        visited[np.x, np.y] = true;
                    }
                }
            }
        }
        return connectedCubes;
    }

    public void RemoveGridItemAt(Vector2Int pos)
    {
        if (pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight)
        {
            GridItem item = gridItems[pos.x, pos.y];
            
            // If there's an active tween for this item, kill it
            if (item != null && activeTweens.ContainsKey(item))
            {
                activeTweens[item].Kill();
                activeTweens.Remove(item);
            }
            
            gridItems[pos.x, pos.y] = null;
        }
    }

    private Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        float offsetX = (gridWidth - 1) * 0.5f * cellSize;
        float offsetY = (gridHeight - 1) * 0.5f * (cellSize + ySpacing);
        float posX = gridPos.x * cellSize - offsetX;
        float posY = gridPos.y * (cellSize + ySpacing) - offsetY;
        Vector2 localPos = new Vector2(posX, posY);
        return backgroundRect.TransformPoint(localPos);
    }

    public void ApplyGravity(float fallSpeed)
    {
        // Grid boyunca her sütunu ve satırı kontrol ediyoruz.
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Hedef pozisyonda boş hücre varsa
                if (GetGridItemAt(new Vector2Int(x, y)) == null)
                {
                    // Yukarıdaki ilk hareket edebilen nesneyi buluyoruz.
                    for (int sourceY = y + 1; sourceY < gridHeight; sourceY++)
                    {
                        Vector2Int sourcePos = new Vector2Int(x, sourceY);
                        GridItem gi = GetGridItemAt(sourcePos);
                        
                        // Make sure the item exists and can fall
                        if (gi != null && gi.CanFall && gi.gameObject != null)
                        {
                            // Kill any active tweens for this item
                            if (activeTweens.ContainsKey(gi))
                            {
                                activeTweens[gi].Kill();
                                activeTweens.Remove(gi);
                            }
                            
                            // Grid yapısında yer değiştirme:
                            gridItems[x, sourceY] = null;
                            gridItems[x, y] = gi;
                            gi.GridPosition = new Vector2Int(x, y);

                            // Hedef dünya pozisyonu
                            Vector3 targetPos = GetWorldPosition(new Vector2Int(x, y));
                            // Şu anki pozisyon ile hedef arasındaki mesafe
                            float distance = Vector3.Distance(gi.transform.position, targetPos);
                            // Her küp için animasyon süresi; süre = mesafe / hız
                            float duration = distance / fallSpeed;

                            // Toon Blast tarzı sekme animasyonu için değerler:
                            float overshootDistance = 0.3f;
                            float bounceUpDistance = 0.2f;

                            Vector3 overshootPos = targetPos + new Vector3(0, -overshootDistance, 0);
                            Vector3 bounceUpPos = targetPos + new Vector3(0, bounceUpDistance, 0);

                            // Strong reference to the transform
                            Transform itemTransform = gi.transform;
                            
                            // Extra check to ensure transform is valid
                            if (itemTransform == null)
                            {
                                Debug.LogWarning("Transform is null for item at position: " + sourcePos);
                                continue;
                            }

                            // Store a reference to the GridItem for use in OnComplete
                            GridItem itemReference = gi;
                            
                            // Tween sırası: Overshoot → Bounce Up → Settle
                            Sequence seq = DOTween.Sequence();
                            
                            seq.Append(itemTransform.DOMove(overshootPos, duration * 0.6f)
                                .SetEase(Ease.InQuad));
                                
                            seq.Append(itemTransform.DOMove(bounceUpPos, duration * 0.2f)
                                .SetEase(Ease.OutQuad));
                                
                            seq.Append(itemTransform.DOMove(targetPos, duration * 0.2f)
                                .SetEase(Ease.InOutQuad));
                                
                            // Add safety check in OnComplete
                            seq.OnComplete(() => {
                                if (activeTweens.ContainsKey(itemReference))
                                {
                                    activeTweens.Remove(itemReference);
                                }
                            });
                            
                            // Add safety check in OnKill
                            seq.OnKill(() => {
                                if (activeTweens.ContainsKey(itemReference))
                                {
                                    activeTweens.Remove(itemReference);
                                }
                            });
                            
                            // Store the sequence
                            activeTweens[gi] = seq;

                            // Aynı sütun ve satırdaki diğer küpler için döngüden çık.
                            break;
                        }
                    }
                }
            }
        }
    }

    public void CheckAndHintGroups()
    {
        // Önce tüm Cube'ları normal forma döndür.
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GridItem gi = GetGridItemAt(new Vector2Int(x, y));
                Cube cube = gi as Cube;
                if (cube != null)
                {
                    cube.SetNormalForm();
                }
            }
        }

        bool[,] visited = new bool[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (visited[x, y])
                    continue;

                GridItem gi = GetGridItemAt(pos);
                Cube cube = gi as Cube;
                if (cube == null)
                    continue;

                List<Cube> group = new List<Cube>();
                DFS(pos, cube.GetColor(), group, visited);

                if (group.Count >= 4)
                {
                    foreach (Cube c in group)
                    {
                        c.SetHintedForm();
                    }
                }
            }
        }
    }

    private void DFS(Vector2Int pos, Cube.ColorType targetColor, List<Cube> group, bool[,] visited)
    {
        if (pos.x < 0 || pos.x >= gridWidth || pos.y < 0 || pos.y >= gridHeight)
            return;
        if (visited[pos.x, pos.y])
            return;

        GridItem gi = GetGridItemAt(pos);
        Cube cube = gi as Cube;
        if (cube == null || cube.GetColor() != targetColor)
            return;

        visited[pos.x, pos.y] = true;
        group.Add(cube);

        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        foreach (Vector2Int d in directions)
        {
            DFS(pos + d, targetColor, group, visited);
        }
    }
    
    // Add this method to properly clean up when the game is reset or objects are destroyed
    public void CleanupTweens()
    {
        foreach (var tween in activeTweens.Values)
        {
            if (tween != null)
            {
                tween.Kill();
            }
        }
        activeTweens.Clear();
    }
}