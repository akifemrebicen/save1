using UnityEngine;

public class LevelSceneManager : MonoBehaviour
{
    [Header("Grid Setup")]
    [SerializeField] private GameObject[] cubePrefabs;  // Cube prefabs with SpriteRenderer
    [SerializeField] private GameObject boxPrefab;
    [SerializeField] private GameObject stonePrefab;
    [SerializeField] private GameObject vasePrefab;
    [SerializeField] private Transform gridParent;        // An empty GameObject in world space

    [Header("Background")]
    [SerializeField] private RectTransform backgroundRect; // The UI Image's RectTransform
    [SerializeField] private float cellSize = 100f;         // Constant cell size in UI units (e.g., pixels)
    [SerializeField] private float cellSpacing = 5f;
    [SerializeField] private float padding = 20f;            // Extra UI units padding around the grid

    private void Start()
    {
        // Load level data.
        int level = PlayerPrefs.GetInt("LastPlayedLevel", 0) + 1;
        LevelData data = LevelLoader.LoadLevel(level);
        if (data == null)
        {
            Debug.LogError("❌ Level data could not be loaded.");
            return;
        }

        // Resize the background image (UI) to fit the grid dimensions.
        // Background size = (grid_width * cellSize + 2*padding, grid_height * cellSize + 2*padding)
        var resizer = new GridBackgroundResizer(backgroundRect, cellSize, padding);
        resizer.Resize(data.grid_width, data.grid_height);

        // Create the grid. Pass the backgroundRect for coordinate conversion.
        var gridManager = new GridManager(cubePrefabs, boxPrefab, stonePrefab, vasePrefab, gridParent, cellSize, cellSpacing, backgroundRect);
        gridManager.CreateGrid(data);
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
                    cube.OnTapped();
                }
            }
        }
    }
}