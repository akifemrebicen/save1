using UnityEngine;

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
        
        // Diğer kontrolcüleri başlat
        blastController = new BlastController(gridManager);
        gravityController = new GravityController(gridManager);
        
        // Kontrolcüler arası bağlantıları kur
        gravityController.SetBlastController(blastController);
        
        // Gravity Controller'ın event'ini dinle
        gravityController.OnCubeAnimationComplete += OnCubeAnimationComplete;
        
        // Başlangıçta ipuçlarını göster
        blastController.CheckAndHintGroups();
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
                    if (connectedCubes.Count >= 2)
                    {
                        foreach (var c in connectedCubes)
                        {
                            c.OnTapped();
                            gridManager.RemoveGridItemAt(c.GridPosition);
                        }
                        
                        // Yerçekimi uygula (GravityController kullanarak)
                        gravityController.ApplyGravity(gravityFallSpeed);
                        
                        // NOT: Artık burada CheckAndHintGroups çağrılmıyor.
                        // Küpler düştükten sonra otomatik olarak kontrol edilecek
                    }
                }
            }
        }
    }
    
    // Küp animasyonu tamamlandığında çağrılacak callback
    private void OnCubeAnimationComplete(GridItem item, Vector2Int position)
    {
        // Callback sadece kaydediliyor, gerekirse burada ek işlemler yapılabilir
    }
    
    private void OnDestroy()
    {
        // Event aboneliğini kaldır
        if (gravityController != null)
        {
            gravityController.OnCubeAnimationComplete -= OnCubeAnimationComplete;
            gravityController.CleanupTweens();
        }
    }
}