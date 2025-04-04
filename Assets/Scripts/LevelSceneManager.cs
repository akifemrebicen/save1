using UnityEngine;
using System.Collections;

public class LevelSceneManager : MonoBehaviour
{
    [Header("Grid Setup")]
    [SerializeField] private GameObject[] cubePrefabs;
    [SerializeField] private GameObject boxPrefab;
    [SerializeField] private GameObject stonePrefab;
    [SerializeField] private GameObject vasePrefab;
    [SerializeField] private Transform gridParent;

    [Header("Background")]
    [SerializeField] private RectTransform backgroundRect;
    [SerializeField] private float cellSize = 100f;
    [SerializeField] private float cellSpacing = 5f;
    [SerializeField] private float padding = 20f;

    [Header("Settings")]
    [SerializeField] private float gravityFallSpeed = 5.0f;

    private GridManager gridManager;
    private GravityController gravityController;
    private BlastController blastController;
    private SpawnController spawnController;  // SpawnController referansı

    private int gridWidth;
    private int gridHeight;

    private void Start()
    {
        int level = PlayerPrefs.GetInt("LastPlayedLevel", 0) + 1;
        LevelData data = LevelLoader.LoadLevel(level);

        if (data == null)
        {
            Debug.LogError("❌ Level data could not be loaded.");
            return;
        }

        gridWidth = data.grid_width;
        gridHeight = data.grid_height;

        var resizer = new GridBackgroundResizer(backgroundRect, cellSize, padding);
        resizer.Resize(gridWidth, gridHeight);

        // GridManager'ı başlat
        gridManager = new GridManager(
            cubePrefabs,
            boxPrefab,
            stonePrefab,
            vasePrefab,
            gridParent,
            cellSize,
            cellSpacing,
            backgroundRect
        );
        gridManager.CreateGrid(data);

        // SpawnController'ı başlat
        spawnController = new SpawnController(gridManager, cubePrefabs, gridParent);

        // Diğer kontrolcüleri başlat
        blastController = new BlastController(gridManager);
        gravityController = new GravityController(gridManager);

        // Kontrolcüler arası bağlantıları kur
        gravityController.SetBlastController(blastController);
        // Başlangıçta ipuçlarını göster
        blastController.CheckAndHintGroups();
        
        // İlk grid durumunu logla
        //LogGridState("Initial Grid State:");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                Cube cube = hit.collider.GetComponent<Cube>();
                if (cube != null)
                {
                    // Bağlantılı küpleri bul (BlastController kullanarak)
                    var connectedCubes = blastController.FindConnectedCubes(cube);
                    var color = cube.GetColor();
                    Debug.Log($"Tapped color = {color}");
                    if (connectedCubes.Count >= 2)
                    {
                        
                        // Patlama öncesi grid durumunu logla
                        //LogGridState("Before Explosion:");

                        foreach (var c in connectedCubes)
                        {
                            var color2 = c.GetColor();
                            Debug.Log($"Connected colors = {color2}");
                            c.OnTapped();
                            gridManager.RemoveGridItemAt(c.GridPosition);
                        }

                        // Patlama sonrası grid durumunu logla
                        //LogGridState("After Explosion:");

                        // Yerçekimi uygula (GravityController kullanarak)
                        gravityController.ApplyGravity(gravityFallSpeed);
                        
                        // Hemen yeni küpleri spawn et - gravity bitmesini bekleme
                        spawnController.SpawnNewCubes();
                        
                        // Belirli bir süre sonra ipuçlarını yeniden kontrol et
                        StartCoroutine(CheckHintsAfterDelay());
                    }
                }
            }
        }
    }

    // Yeni bir coroutine ekleyin - bu sadece ipuçları için biraz gecikme ekler
    private IEnumerator CheckHintsAfterDelay()
    {
        // Tüm düşme animasyonlarının bitmesi için yeterli süre bekleyin
        // max(gravityFallSpeed, spawnController.fallSpeed) + küçük bir tampon
        yield return new WaitForSeconds(Mathf.Max(gravityFallSpeed, 1.0f) + 0.2f);
        
        // İpuçlarını kontrol et ve göster
        blastController.CheckAndHintGroups();
    }

    // Grid'in mevcut durumunu loglayan yardımcı metod
    private void LogGridState(string message)
    {
        Debug.Log(message);

        for (int y = 0; y < gridManager.GridHeight; y++)
        {
            string rowLog = "";
            for (int x = 0; x < gridManager.GridWidth; x++)
            {
                GridItem item = gridManager.GetGridItemAt(new Vector2Int(x, y));
                rowLog += item != null ? $"{item.gameObject.name} " : "Empty ";
            }
            Debug.Log($"Row {y}: {rowLog}");
        }
    }
}