using System.Collections.Generic;
using UnityEngine;

public class BlastController
{
    private GridManager gridManager;

    public BlastController(GridManager gridManager)
    {
        this.gridManager = gridManager;
    }
    
    // Belirli bir pozisyonun etrafındaki grupları kontrol et
    public void CheckGroupsAroundPosition(Vector2Int position)
    {
        // Etraftaki pozisyonlar (kendisi + komşular)
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.zero,     // Kendisi
            Vector2Int.up,       // Yukarı
            Vector2Int.down,     // Aşağı
            Vector2Int.left,     // Sol
            Vector2Int.right,    // Sağ
            Vector2Int.up + Vector2Int.left,    // Sol Üst
            Vector2Int.up + Vector2Int.right,   // Sağ Üst
            Vector2Int.down + Vector2Int.left,  // Sol Alt
            Vector2Int.down + Vector2Int.right  // Sağ Alt
        };
        
        // Önce bu pozisyonlardaki tüm küpleri normal forma çevir
        foreach (Vector2Int dir in directions)
        {
            Vector2Int checkPos = position + dir;
            GridItem item = gridManager.GetGridItemAt(checkPos);
            if (item is Cube cube)
            {
                cube.SetNormalForm();
            }
        }
        
        // Şimdi etraftaki her pozisyon için DFS taraması yaparak 4+ eşleşmeleri bul
        bool[,] visited = new bool[gridManager.GridWidth, gridManager.GridHeight];
        
        foreach (Vector2Int dir in directions)
        {
            Vector2Int checkPos = position + dir;
            if (checkPos.x < 0 || checkPos.x >= gridManager.GridWidth || 
                checkPos.y < 0 || checkPos.y >= gridManager.GridHeight)
                continue;
                
            if (visited[checkPos.x, checkPos.y])
                continue;
                
            GridItem item = gridManager.GetGridItemAt(checkPos);
            if (item is Cube cube)
            {
                List<Cube> group = new List<Cube>();
                DFS(checkPos, cube.GetColor(), group, visited);
                
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

    public List<Cube> FindConnectedCubes(Cube startCube)
    {
        List<Cube> connectedCubes = new List<Cube>();
        bool[,] visited = new bool[gridManager.GridWidth, gridManager.GridHeight];

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
                if (np.x >= 0 && np.x < gridManager.GridWidth && np.y >= 0 && np.y < gridManager.GridHeight && !visited[np.x, np.y])
                {
                    GridItem gi = gridManager.GetGridItemAt(np);
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

    public void CheckAndHintGroups()
    {
        // Önce tüm Cube'ları normal forma döndür.
        for (int x = 0; x < gridManager.GridWidth; x++)
        {
            for (int y = 0; y < gridManager.GridHeight; y++)
            {
                GridItem gi = gridManager.GetGridItemAt(new Vector2Int(x, y));
                Cube cube = gi as Cube;
                if (cube != null)
                {
                    cube.SetNormalForm();
                }
            }
        }

        bool[,] visited = new bool[gridManager.GridWidth, gridManager.GridHeight];

        for (int x = 0; x < gridManager.GridWidth; x++)
        {
            for (int y = 0; y < gridManager.GridHeight; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (visited[x, y])
                    continue;

                GridItem gi = gridManager.GetGridItemAt(pos);
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
        if (pos.x < 0 || pos.x >= gridManager.GridWidth || pos.y < 0 || pos.y >= gridManager.GridHeight)
            return;
        if (visited[pos.x, pos.y])
            return;

        GridItem gi = gridManager.GetGridItemAt(pos);
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
}