using System.Collections;
using UnityEngine;
using DG.Tweening;

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
    private float fallSpeed = 5.0f;      // Küplerin hedefe düşme animasyon süresi

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

                    // Rastgele bir küp prefab'ı ve renk seç
                    int prefabIndex = Random.Range(0, cubePrefabs.Length);
                    GameObject prefab = cubePrefabs[prefabIndex];
                    Cube.ColorType colorToAssign = (Cube.ColorType)prefabIndex;
                    
                    if(prefab == null)
                        continue;

                    // Yeni küpü instantiate et; parent olarak gridParent'ı veriyoruz
                    GameObject newCube = Object.Instantiate(prefab, spawnPos, Quaternion.identity, gridParent);

                    // Küpün GridItem component'ını al ve grid koordinatını ata
                    GridItem gridItem = newCube.GetComponent<GridItem>();
                    if (gridItem != null)
                    {
                        gridItem.GridPosition = gridPos;
                        
                        // Cube tipinde ise renk ataması yap
                        Cube cube = gridItem as Cube;
                        if (cube != null)
                        {
                            cube.SetColor(colorToAssign);
                            Debug.Log($"Spawned new cube at {gridPos} with color: {colorToAssign}");
                        }
                        
                        gridManager.SetGridItemAt(gridPos, gridItem);
                    }

                    // Küpü, DOTween ile spawn pozisyonundan hedef worldPos'e doğru animasyonla indir
                    AnimateCubeFall(newCube, worldPos);
                }
            }
        }
    }

    // Küpü spawn pozisyonundan hedef pozisyona DOTween ile hareket ettirir (GravityController ile aynı animasyon)
    private void AnimateCubeFall(GameObject cube, Vector3 targetPos)
    {
        GridItem gridItem = cube.GetComponent<GridItem>();
        if (gridItem == null) return;
        
        // Şu anki pozisyon (spawn pozisyonu) ile hedef arasındaki mesafe
        Vector3 spawnPos = cube.transform.position; // Küp zaten spawn pozisyonunda oluşturuldu
        float distance = Vector3.Distance(spawnPos, targetPos);
        // Her küp için animasyon süresi; süre = mesafe / hız
        float duration = distance / fallSpeed;

        // Toon Blast tarzı sekme animasyonu için değerler:
        float overshootDistance = 0.3f;
        float bounceUpDistance = 0.2f;

        Vector3 overshootPos = targetPos + new Vector3(0, -overshootDistance, 0);
        Vector3 bounceUpPos = targetPos + new Vector3(0, bounceUpDistance, 0);

        // Strong reference to the transform
        Transform itemTransform = cube.transform;
        
        // Tween sırası: Spawn Pos → Overshoot → Bounce Up → Settle
        Sequence seq = DOTween.Sequence();
        
        // Spawn pozisyonundan overshoot pozisyonuna
        seq.Append(itemTransform.DOMove(overshootPos, duration * 0.6f)
            .SetEase(Ease.InQuad));
            
        seq.Append(itemTransform.DOMove(bounceUpPos, duration * 0.2f)
            .SetEase(Ease.OutQuad));
            
        seq.Append(itemTransform.DOMove(targetPos, duration * 0.2f)
            .SetEase(Ease.InOutQuad));
            
        // Animasyon tamamlandığında yapılacak işlemler
        seq.OnComplete(() => {
            // Animasyon tamamlandığında isteğe bağlı ek işlemler yapılabilir
        });
    }
}