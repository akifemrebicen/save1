using System.Collections;
using UnityEngine;
using DG.Tweening; // DOTween kullanımı için

public class SpawnController
{
    // GridManager referansı
    private GridManager gridManager;
    // Spawnlanacak küp prefab'ları
    private GameObject[] cubePrefabs;
    // Spawn edilecek küplerin parent'ı
    private Transform gridParent;

    // Spawn ayarları
    private float spawnYOffset = 5f;   // Hedefin üstünde spawn offset
    private float fallSpeed = 1.0f;      // Küplerin hedefe düşme animasyon süresi

    // FallSpeed property - dışarıdan erişim için
    public float FallSpeed { get { return fallSpeed; } }

    // Constructor: gridManager, prefab dizisi ve gridParent parametrelerini alır.
    public SpawnController(GridManager grid, GameObject[] cubes, Transform gridParent)
    {
        gridManager = grid;
        cubePrefabs = cubes;
        this.gridParent = gridParent;
    }

    // Gridde boş kalan tüm hücreleri tarar, boşsa yeni küp spawnlar.
    public void SpawnNewCubes()
    {
        int gridWidth = gridManager.GridWidth;
        int gridHeight = gridManager.GridHeight;
        float cellSize = gridManager.CellSize;
        float ySpacing = gridManager.YSpacing;
        RectTransform backgroundRect = gridManager.BackgroundRect;

        // Gridi ortalamak için offset hesaplaması (CreateGrid metodundaki mantıkla aynı)
        float offsetX = (gridWidth - 1) * 0.5f * cellSize;
        float offsetY = (gridHeight - 1) * 0.5f * (cellSize + ySpacing);

        // Gridin tüm hücrelerini dolaş
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                // Eğer hücre boşsa
                if (gridManager.GetGridItemAt(gridPos) == null)
                {
                    // Pozisyon hesaplaması: her hücrenin local pozisyonunu alıp, backgroundRect.TransformPoint ile dünya koordinatına çeviriyoruz
                    float posX = x * cellSize - offsetX;
                    float posY = y * (cellSize + ySpacing) - offsetY;
                    Vector2 localPos = new Vector2(posX, posY);
                    Vector3 worldPos = backgroundRect.TransformPoint(localPos);

                    // Spawn pozisyonu: hedef pozisyonun üstüne spawnYOffset ekleyerek spawnlanma noktasını belirliyoruz
                    Vector3 spawnPos = worldPos + Vector3.up * spawnYOffset;

                    // Rastgele bir küp prefab'ı seç
                    GameObject prefab = GetRandomCubePrefab();
                    if(prefab == null)
                        continue;

                    // Yeni küpü instantiate et; parent olarak gridParent'ı veriyoruz
                    GameObject newCube = Object.Instantiate(prefab, spawnPos, Quaternion.identity, gridParent);

                    // Küpün GridItem component'ını al ve grid koordinatını ata
                    GridItem gridItem = newCube.GetComponent<GridItem>();
                    if (gridItem != null)
                    {
                        gridItem.GridPosition = gridPos;
                        gridManager.SetGridItemAt(gridPos, gridItem);
                    }

                    // Küpü, DOTween ile spawn pozisyonundan hedef worldPos'e doğru animasyonla indir
                    AnimateCubeFall(newCube, worldPos);
                }
            }
        }
    }

    // Rastgele bir küp prefab'ı döndürür
    private GameObject GetRandomCubePrefab()
    {
        if (cubePrefabs != null && cubePrefabs.Length > 0)
        {
            int index = Random.Range(0, cubePrefabs.Length);
            return cubePrefabs[index];
        }
        return null;
    }

    // Küpü DOTween ile hedef pozisyona doğru hareket ettirir
    private void AnimateCubeFall(GameObject cube, Vector3 targetPos)
    {
        cube.transform.DOMove(targetPos, fallSpeed).SetEase(Ease.InQuad).OnComplete(() =>
        {
            // Animasyon tamamlandığında isteğe bağlı ek işlemler yapılabilir.
        });
    }
}