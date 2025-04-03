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

    [Header("Gravity Settings")]
    [SerializeField] private float gravityFallSpeed = 5.0f;

    private GridManager gridManager;

    private void Start()
    {
        int level = PlayerPrefs.GetInt("LastPlayedLevel", 0) + 1;
        LevelData data = LevelLoader.LoadLevel(level);
        if (data == null)
        {
            Debug.LogError("❌ Level data could not be loaded.");
            return;
        }

        var resizer = new GridBackgroundResizer(backgroundRect, cellSize, padding);
        resizer.Resize(data.grid_width, data.grid_height);

        gridManager = new GridManager(
            cubePrefabs, boxPrefab, stonePrefab, vasePrefab,
            gridParent, cellSize, cellSpacing, backgroundRect
        );
        gridManager.CreateGrid(data);
        gridManager.CheckAndHintGroups();
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
                    var connectedCubes = gridManager.FindConnectedCubes(cube);
                    if (connectedCubes.Count >= 2)
                    {
                        foreach (var c in connectedCubes)
                        {
                            c.OnTapped();
                            gridManager.RemoveGridItemAt(c.GridPosition);
                        }
                    }
                    gridManager.ApplyGravity(gravityFallSpeed);
                    gridManager.CheckAndHintGroups();
                }
            }
        }
    }
}
