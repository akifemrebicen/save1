using UnityEngine;
using System.Collections;
using DG.Tweening;

public class LevelSceneManager : MonoBehaviour
{
    public static LevelSceneManager Instance { get; private set; }

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
        Instance = this;

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

        spawnController = new SpawnController(gridManager, cubePrefabs, gridParent);

        blastController = new BlastController(gridManager);
        gravityController = new GravityController(gridManager);
        gravityController.SetBlastController(blastController);

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
                Cube tappedCube = hit.collider.GetComponent<Cube>();
                if (tappedCube != null)
                {
                    var connectedCubes = blastController.FindConnectedCubes(tappedCube);
                    if (connectedCubes.Count >= 4)
                    {
                        StartCoroutine(AnimateGroupAndCreateRocket(connectedCubes, tappedCube.GridPosition));
                    }
                    else if (connectedCubes.Count >= 2)
                    {
                        foreach (var c in connectedCubes)
                        {
                            c.OnTapped();
                            gridManager.RemoveGridItemAt(c.GridPosition);
                        }

                        gravityController.ApplyGravity(gravityFallSpeed);
                        spawnController.SpawnNewCubes();
                        StartCoroutine(CheckHintsAfterDelay());
                    }
                    else
                    {
                        // Küçük grup için işlem yapılmıyor.
                    }
                }
                else
                {
                    Rocket rocket = hit.collider.GetComponent<Rocket>();
                    if (rocket != null)
                    {
                        Debug.Log("Rocket tapped!");
                        rocket.OnTapped();
                        gridManager.RemoveGridItemAt(rocket.GridPosition);
                    }
                }
            }
        }
    }

    public void TriggerPostExplosion()
    {
        gravityController.ApplyGravity(gravityFallSpeed);
        spawnController.SpawnNewCubes();
        StartCoroutine(CheckHintsAfterDelay());
    }

    public void TriggerPostExplosionDelayed(float delay)
    {
        StartCoroutine(DelayedExplosionRoutine(delay));
    }

    private IEnumerator DelayedExplosionRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        TriggerPostExplosion();
    }

    private IEnumerator CheckHintsAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);
        blastController.CheckAndHintGroups();
    }

    private IEnumerator AnimateGroupAndCreateRocket(System.Collections.Generic.List<Cube> group, Vector2Int rocketPos)
    {
        Vector3 rocketWorldPos = gridManager.GetWorldPosition(rocketPos);
        Sequence seq = DOTween.Sequence();

        foreach (var cube in group)
        {
            seq.Join(
                cube.transform.DOMove(rocketWorldPos, 0.3f)
                              .SetEase(Ease.InQuad)
            );
        }

        seq.AppendInterval(0.1f);

        foreach (var cube in group)
        {
            seq.Join(
                cube.transform.DOScale(0.8f, 0.1f)
            );
        }

        seq.OnComplete(() =>
        {
            foreach (var c in group)
            {
                gridManager.RemoveGridItemAt(c.GridPosition);
                Destroy(c.gameObject);
            }
        });

        yield return seq.WaitForCompletion();

        Rocket.RocketDirection direction = (Random.value < 0.5f)
            ? Rocket.RocketDirection.Vertical
            : Rocket.RocketDirection.Horizontal;
        rocketManager.CreateRocket(rocketPos, direction);
        Debug.Log($"Rocket created at {rocketPos}");

        TriggerPostExplosion();
    }

    // Yarım roket (RocketHalf) yok edildiğinde çağrılacak metot.
    public void RocketHalfDestroyed()
    {
        Debug.Log("A rocket half has been destroyed.");
        // İhtiyaca göre ek işlemler yapılabilir.
    }
}
