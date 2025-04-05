using UnityEngine;
using DG.Tweening;
using System.Collections;

public class RocketHalf : GridItem
{
    private Vector2Int moveDirection;   // Örneğin (1,0) veya (0,1)
    private GridManager gridManager;
    private float moveSpeed = 10f;        // Hareket hızı

    // Discrete grid takibi için:
    private Vector2Int startGridPos;      // Oluşturulduğu anki grid pozisyonu
    private Vector3 startWorldPos;        // Oluşturulduğu anki world pozisyonu
    private Vector2 cellProgress;         // Hücre içerisindeki geçiş miktarı (birikimli)

    // Explosion sadece bir kez tetiklensin:
    private bool explosionTriggered = false;

    public override bool CanFall => true;

    /// <summary>
    /// Half rocket'ı başlatır.
    /// </summary>
    /// <param name="direction">Hareket yönü (örn: Vector2Int.up veya Vector2Int.right)</param>
    /// <param name="manager">GridManager referansı</param>
    /// <param name="initialGridPos">Half rocket'ın oluşturulduğu grid pozisyonu</param>
    public void Initialize(Vector2Int direction, GridManager manager, Vector2Int initialGridPos)
    {
        moveDirection = direction;
        gridManager = manager;
        startGridPos = initialGridPos;
        startWorldPos = transform.position;
        cellProgress = Vector2.zero;
        GridPosition = initialGridPos;
        explosionTriggered = false;
    }

    private void Update()
    {
        // Hareketi world space'de uygula.
        Vector3 moveVec = new Vector3(moveDirection.x, moveDirection.y, 0f) * (moveSpeed * Time.deltaTime);
        transform.Translate(moveVec);

        // Hücre bazında hareketi biriktir.
        float cellSize = gridManager.CellSize;
        cellProgress.x += moveVec.x / cellSize;
        cellProgress.y += moveVec.y / cellSize;
        Vector2Int deltaGrid = new Vector2Int(Mathf.FloorToInt(cellProgress.x), Mathf.FloorToInt(cellProgress.y));
        cellProgress.x -= deltaGrid.x;
        cellProgress.y -= deltaGrid.y;
        Vector2Int currentGridPos = startGridPos + deltaGrid;
        GridPosition = currentGridPos;

        // Explosion yalnızca bir kez tetiklensin:
        if (!explosionTriggered)
        {
            explosionTriggered = true;
            // Eğer horizontal ise, hem sola hem sağa blast; vertical ise, hem yukarı hem aşağı blast edelim.
            if (moveDirection.x != 0)
            {
                StartCoroutine(BlastRowFromCenter(currentGridPos));
            }
            else if (moveDirection.y != 0)
            {
                StartCoroutine(BlastColumnFromCenter(currentGridPos));
            }
            // Explosion tamamlandıktan sonra half rocket'ı kısa delay ile yok edelim.
            Destroy(gameObject, 0.5f);
        }

        CheckAndDestroyOutOfBounds();
    }

    // Belirtilen center hücresinden başlayarak, hem sola hem sağa doğru patlama yapar.
    private IEnumerator BlastRowFromCenter(Vector2Int center)
    {
        // Önce center hücresindeki öğeyi patlat.
        BlastGridItemAt(center);
        yield return new WaitForSeconds(0.05f);
        // Sola ve sağa doğru blast.
        int offset = 1;
        while ((center.x - offset >= 0) || (center.x + offset < gridManager.GridWidth))
        {
            if (center.x - offset >= 0)
            {
                BlastGridItemAt(new Vector2Int(center.x - offset, center.y));
            }
            if (center.x + offset < gridManager.GridWidth)
            {
                BlastGridItemAt(new Vector2Int(center.x + offset, center.y));
            }
            offset++;
            yield return new WaitForSeconds(0.05f);
        }
    }

    // Belirtilen center hücresinden başlayarak, hem yukarı hem aşağı blast yapar.
    private IEnumerator BlastColumnFromCenter(Vector2Int center)
    {
        BlastGridItemAt(center);
        yield return new WaitForSeconds(0.05f);
        int offset = 1;
        while ((center.y - offset >= 0) || (center.y + offset < gridManager.GridHeight))
        {
            if (center.y - offset >= 0)
            {
                BlastGridItemAt(new Vector2Int(center.x, center.y - offset));
            }
            if (center.y + offset < gridManager.GridHeight)
            {
                BlastGridItemAt(new Vector2Int(center.x, center.y + offset));
            }
            offset++;
            yield return new WaitForSeconds(0.05f);
        }
    }

    private void BlastGridItemAt(Vector2Int pos)
    {
        GridItem item = gridManager.GetGridItemAt(pos);
        if (item != null && !(item is Rocket))
        {
            // Uygun animasyonla patlat.
            item.transform.DOScale(0.8f, 0.1f).OnComplete(() =>
            {
                gridManager.RemoveGridItemAt(pos);
                Destroy(item.gameObject);
            });
        }
    }

    private void CheckAndDestroyOutOfBounds()
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
        {
            Destroy(gameObject);
        }
    }

    public override void OnTapped()
    {
        Destroy(gameObject);
    }
}
