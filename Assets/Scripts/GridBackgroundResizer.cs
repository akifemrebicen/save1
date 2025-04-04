using UnityEngine;

public class GridBackgroundResizer
{
    private RectTransform background;
    private float cellSize;
    private float padding;

    public GridBackgroundResizer(RectTransform bg, float cellSize = 100f, float padding = 20f)
    {
        this.background = bg;
        this.cellSize = cellSize;
        this.padding = padding;
    }

    public void Resize(int width, int height)
    {
        if (background == null) return;

        // Compute the desired size in UI units (pixels)
        float w = width * cellSize + padding - 7 ;
        float h = height * cellSize + padding * 2;
        background.sizeDelta = new Vector2(w, h);

        // Center the background (ensure its pivot is (0.5, 0.5))
        background.anchoredPosition = new Vector2(0,-150);
    }
}