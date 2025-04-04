using UnityEngine;
using System.Collections;
using DG.Tweening;  // Make sure you have DOTween imported

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

    [Header("Rocket Prefabs")]
    [SerializeField] private GameObject verticalRocketPrefab;
    [SerializeField] private GameObject horizontalRocketPrefab;
    [SerializeField] private GameObject verticalUpHalfPrefab;
    [SerializeField] private GameObject verticalDownHalfPrefab;
    [SerializeField] private GameObject horizontalLeftHalfPrefab;
    [SerializeField] private GameObject horizontalRightHalfPrefab;

    private GridManager gridManager;
    private GravityController gravityController;
    private BlastController blastController;
    private SpawnController spawnController;
    private RocketManager rocketManager;

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

        // Resize the background
        var resizer = new GridBackgroundResizer(backgroundRect, cellSize, padding);
        resizer.Resize(gridWidth, gridHeight);

        // Initialize the GridManager and create the grid
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

        // Initialize the SpawnController
        spawnController = new SpawnController(gridManager, cubePrefabs, gridParent);

        // Initialize the other controllers
        blastController = new BlastController(gridManager);
        gravityController = new GravityController(gridManager);
        gravityController.SetBlastController(blastController);

        // Initialize the RocketManager with rocket prefab references
        rocketManager = new RocketManager(
            verticalRocketPrefab,
            horizontalRocketPrefab,
            verticalUpHalfPrefab,
            verticalDownHalfPrefab,
            horizontalLeftHalfPrefab,
            horizontalRightHalfPrefab,
            gridManager,
            gridParent
        );

        // Show initial hints (highlight groups of 4+ cubes)
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
                // Check if we tapped a Cube
                Cube tappedCube = hit.collider.GetComponent<Cube>();
                if (tappedCube != null)
                {
                    var connectedCubes = blastController.FindConnectedCubes(tappedCube);
                    // If we have at least 4 cubes in the group, animate them into a rocket
                    if (connectedCubes.Count >= 4)
                    {
                        // Start a coroutine that:
                        // 1) Animates all cubes to the tapped cell
                        // 2) Destroys them
                        // 3) Spawns a rocket at that cell
                        // 4) Applies gravity & spawns new cubes
                        StartCoroutine(AnimateGroupAndCreateRocket(connectedCubes, tappedCube.GridPosition));
                    }
                    else
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
                else
                {
                    // If it's not a cube, check if it's a rocket
                    Rocket rocket = hit.collider.GetComponent<Rocket>();
                    if (rocket != null)
                    {
                        Debug.Log("Rocket tapped!");
                        rocket.OnTapped();  // Rocket splits
                        gridManager.RemoveGridItemAt(rocket.GridPosition);
                    }
                }
            }
        }
    }
    private IEnumerator CheckHintsAfterDelay()
    {
        // Tüm düşme animasyonlarının bitmesi için yeterli süre bekleyin
        // max(gravityFallSpeed, spawnController.fallSpeed) + küçük bir tampon
        yield return new WaitForSeconds(Mathf.Max(gravityFallSpeed, 1.0f) + 0.2f);
        
        // İpuçlarını kontrol et ve göster
        blastController.CheckAndHintGroups();
    }

    /// <summary>
    /// Coroutine that:
    /// 1) Animates all cubes in 'group' to the 'rocketPos' cell
    /// 2) Scales them slightly and destroys them
    /// 3) Creates a rocket in that cell
    /// 4) Applies gravity
    /// 5) Spawns new cubes
    /// 6) Checks for hints after a short delay
    /// </summary>
    private IEnumerator AnimateGroupAndCreateRocket(System.Collections.Generic.List<Cube> group, Vector2Int rocketPos)
    {
        // Convert rocketPos (grid coords) to world coords
        Vector3 rocketWorldPos = gridManager.GetWorldPosition(rocketPos);

        // We can animate all cubes simultaneously in a single DOTween Sequence
        Sequence seq = DOTween.Sequence();

        // 1) Animate each cube to the rocketWorldPos
        foreach (var cube in group)
        {
            // Join each cube's movement to the main sequence
            seq.Join(
                cube.transform.DOMove(rocketWorldPos, 0.3f)
                              .SetEase(Ease.InQuad)
            );
        }

        // 2) After they've arrived, scale them down a bit before destroying
        //    We can add a small AppendInterval to ensure they've reached the rocketWorldPos
        seq.AppendInterval(0.1f);

        // Now scale them all to 0.8 in 0.1s
        foreach (var cube in group)
        {
            seq.Join(
                cube.transform.DOScale(0.8f, 0.1f)
            );
        }

        // 3) After the scale, destroy them
        seq.OnComplete(() =>
        {
            // Remove cubes from the grid & destroy
            foreach (var c in group)
            {
                gridManager.RemoveGridItemAt(c.GridPosition);
                Destroy(c.gameObject);
            }
        });

        // Wait for sequence to finish
        yield return seq.WaitForCompletion();

        // 4) Create a rocket in that cell. Random direction:
        Rocket.RocketDirection direction = (Random.value < 0.5f)
            ? Rocket.RocketDirection.Vertical
            : Rocket.RocketDirection.Horizontal;
        rocketManager.CreateRocket(rocketPos, direction);

        // 5) Apply gravity
        gravityController.ApplyGravity(gravityFallSpeed);

        // 6) Spawn new cubes
        spawnController.SpawnNewCubes();

        // 7) Check hints after a short delay
        yield return new WaitForSeconds(Mathf.Max(gravityFallSpeed, 1.0f) + 0.2f);
        blastController.CheckAndHintGroups();
    }

    // (Optional) Log the grid state for debugging
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
