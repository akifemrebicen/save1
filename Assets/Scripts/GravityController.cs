using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GravityController
{
    private GridManager gridManager;
    private Dictionary<GridItem, Sequence> activeTweens = new Dictionary<GridItem, Sequence>();
    private BlastController blastController;
    
    // Her küp düştüğünde çağrılacak delegate tanımı
    public delegate void CubeAnimationCompleteDelegate(GridItem item, Vector2Int position);
    public event CubeAnimationCompleteDelegate OnCubeAnimationComplete;

    public GravityController(GridManager gridManager)
    {
        this.gridManager = gridManager;
    }
    
    public void SetBlastController(BlastController blastController)
    {
        this.blastController = blastController;
    }

    public void ApplyGravity(float fallSpeed)
    {
        int gridWidth = gridManager.GridWidth;
        int gridHeight = gridManager.GridHeight;

        // Grid boyunca her sütunu ve satırı kontrol ediyoruz.
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Hedef pozisyonda boş hücre varsa
                if (gridManager.GetGridItemAt(new Vector2Int(x, y)) == null)
                {
                    // Yukarıdaki ilk hareket edebilen nesneyi buluyoruz.
                    for (int sourceY = y + 1; sourceY < gridHeight; sourceY++)
                    {
                        Vector2Int sourcePos = new Vector2Int(x, sourceY);
                        GridItem gi = gridManager.GetGridItemAt(sourcePos);
                        
                        // Make sure the item exists and can fall
                        if (gi != null && gi.CanFall && gi.gameObject != null)
                        {
                            // Kill any active tweens for this item
                            if (activeTweens.ContainsKey(gi))
                            {
                                activeTweens[gi].Kill();
                                activeTweens.Remove(gi);
                            }
                            
                            // Grid yapısında yer değiştirme:
                            gridManager.SetGridItemAt(new Vector2Int(x, sourceY), null);
                            gridManager.SetGridItemAt(new Vector2Int(x, y), gi);
                            gi.GridPosition = new Vector2Int(x, y);

                            // Hedef dünya pozisyonu
                            Vector3 targetPos = gridManager.GetWorldPosition(new Vector2Int(x, y));
                            // Şu anki pozisyon ile hedef arasındaki mesafe
                            float distance = Vector3.Distance(gi.transform.position, targetPos);
                            // Her küp için animasyon süresi; süre = mesafe / hız
                            float duration = distance / fallSpeed;

                            // Toon Blast tarzı sekme animasyonu için değerler:
                            float overshootDistance = 0.3f;
                            float bounceUpDistance = 0.2f;

                            Vector3 overshootPos = targetPos + new Vector3(0, -overshootDistance, 0);
                            Vector3 bounceUpPos = targetPos + new Vector3(0, bounceUpDistance, 0);

                            // Strong reference to the transform
                            Transform itemTransform = gi.transform;
                            
                            // Extra check to ensure transform is valid
                            if (itemTransform == null)
                            {
                                Debug.LogWarning("Transform is null for item at position: " + sourcePos);
                                continue;
                            }

                            // Store a reference to the GridItem for use in OnComplete
                            GridItem itemReference = gi;
                            
                            // Tween sırası: Overshoot → Bounce Up → Settle
                            Sequence seq = DOTween.Sequence();
                            
                            seq.Append(itemTransform.DOMove(overshootPos, duration * 0.6f)
                                .SetEase(Ease.InQuad));
                                
                            seq.Append(itemTransform.DOMove(bounceUpPos, duration * 0.2f)
                                .SetEase(Ease.OutQuad));
                                
                            seq.Append(itemTransform.DOMove(targetPos, duration * 0.2f)
                                .SetEase(Ease.InOutQuad));
                                
                            // Add safety check in OnComplete
                            seq.OnComplete(() => {
                                if (activeTweens.ContainsKey(itemReference))
                                {
                                    activeTweens.Remove(itemReference);
                                }
                                
                                // Animasyon tamamlandığında eventi tetikle
                                OnCubeAnimationComplete?.Invoke(itemReference, itemReference.GridPosition);
                                
                                // Kendi etrafında eşleşme kontrolü yapıyoruz
                                if (blastController != null)
                                {
                                    // Küpün etrafını kontrol et
                                    blastController.CheckGroupsAroundPosition(itemReference.GridPosition);
                                }
                            });
                            
                            // Add safety check in OnKill
                            seq.OnKill(() => {
                                if (activeTweens.ContainsKey(itemReference))
                                {
                                    activeTweens.Remove(itemReference);
                                }
                            });
                            
                            // Store the sequence
                            activeTweens[gi] = seq;

                            // Aynı sütun ve satırdaki diğer küpler için döngüden çık.
                            break;
                        }
                    }
                }
            }
        }
    }
    
    // Add this method to properly clean up when the game is reset or objects are destroyed
    public void CleanupTweens()
    {
        foreach (var tween in activeTweens.Values)
        {
            if (tween != null)
            {
                tween.Kill();
            }
        }
        activeTweens.Clear();
    }
}