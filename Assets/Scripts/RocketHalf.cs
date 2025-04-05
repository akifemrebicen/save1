using System.Collections;
using UnityEngine;
using DG.Tweening;

public class RocketHalf : GridItem
{
    // Grid yönünde hareket (örneğin, yukarı için Vector2Int.up, aşağı için Vector2Int.down)
    private Vector2Int moveDirection;
    private GridManager gridManager;
    private float stepDuration = 0.1f; // Her hücreye geçiş süresi (ayarlanabilir)

    // Mevcut grid pozisyonu
    private Vector2Int currentGridPos;

    /// <summary>
    /// Roket yarısını başlatır.
    /// </summary>
    /// <param name="direction">Grid yönünde hareket (örneğin, Vector2Int.up)</param>
    /// <param name="manager">GridManager referansı</param>
    /// <param name="initialGridPos">Başlangıç grid pozisyonu</param>
    public void Initialize(Vector2Int direction, GridManager manager, Vector2Int initialGridPos)
    {
        moveDirection = direction;
        gridManager = manager;
        currentGridPos = initialGridPos;
        GridPosition = initialGridPos;
        // Başlangıç world pozisyonunu grid pozisyonundan hesapla
        transform.position = gridManager.GetWorldPosition(initialGridPos);
        // Grid adımlı hareketi başlatan coroutine
        StartCoroutine(GridMovementRoutine());
    }

    private IEnumerator GridMovementRoutine()
    {
        while (true)
        {
            // Bir sonraki grid hücresini hesapla
            Vector2Int nextGridPos = currentGridPos + moveDirection;

            // Eğer grid sınırlarının dışına çıktıysa roketi yok et (bu kontrol isteğe bağlı)
            if (nextGridPos.x < 0 || nextGridPos.x >= gridManager.GridWidth ||
                nextGridPos.y < 0 || nextGridPos.y >= gridManager.GridHeight)
            {
                // Alternatif olarak, ekran dışı kontrolü aşağıda yapılacak
                break;
            }

            // Hedef world pozisyonu, grid hücresinden hesaplanır
            Vector3 targetWorldPos = gridManager.GetWorldPosition(nextGridPos);

            // Mevcut world pozisyonundan hedefe animasyonlu geçiş yap
            yield return transform.DOMove(targetWorldPos, stepDuration).WaitForCompletion();

            // Yeni grid pozisyonuna geçtiğimizde güncelle
            currentGridPos = nextGridPos;
            GridPosition = nextGridPos;

            // Eğer bu grid hücresinde bir Cube varsa, explosion animasyonunu tetikle
            GridItem item = gridManager.GetGridItemAt(nextGridPos);
            if (item != null && item is Cube)
            {
                BlastGridItemAt(nextGridPos);
            }

            // Ekran dışı kontrolü: Roket parçasının world konumunu kameranın viewport’una çeviriyoruz.
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
            if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
            {
                // Ekrandan çıkıyorsa, kendisini patlatacak animasyon oynat ve destroy et
                Sequence selfExplosionSeq = DOTween.Sequence();
                selfExplosionSeq.Append(transform.DOScale(1.2f, 0.1f).SetEase(Ease.OutBounce));
                selfExplosionSeq.Append(transform.DOScale(0f, 0.1f).SetEase(Ease.InBack));
                selfExplosionSeq.OnComplete(() => { Destroy(gameObject); });
                yield break;
            }

            // Her adım arasında kısa bir gecikme (isteğe bağlı)
            yield return new WaitForSeconds(0.05f);
        }

        // Eğer grid sınırlarının dışına çıkarsa da ekran kontrolü ile aynı işlemi yapalım:
        Vector3 finalViewportPos = Camera.main.WorldToViewportPoint(transform.position);
        if (finalViewportPos.x < 0 || finalViewportPos.x > 1 || finalViewportPos.y < 0 || finalViewportPos.y > 1)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOScale(1.2f, 0.1f).SetEase(Ease.OutBounce));
            seq.Append(transform.DOScale(0f, 0.1f).SetEase(Ease.InBack));
            seq.OnComplete(() => { Destroy(gameObject); });
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Belirtilen grid hücresindeki Cube için explosion animasyonu uygular.
    /// </summary>
    /// <param name="pos">Grid pozisyonu</param>
    private void BlastGridItemAt(Vector2Int pos)
    {
        GridItem item = gridManager.GetGridItemAt(pos);
        if (item != null && item is Cube)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(item.transform.DOScale(1.2f, 0.1f).SetEase(Ease.OutBounce));
            seq.Append(item.transform.DOScale(0f, 0.1f).SetEase(Ease.InBack));
            seq.OnComplete(() =>
            {
                gridManager.RemoveGridItemAt(pos);
                Destroy(item.gameObject);
            });
        }
    }

    public override bool CanFall => true;

    public override void OnTapped()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (LevelSceneManager.Instance != null)
            LevelSceneManager.Instance.RocketHalfDestroyed();
    }
}
