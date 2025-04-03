using UnityEngine;

public static class LevelLoader
{
    public static LevelData LoadLevel(int levelNumber)
    {
        // Format as two digits: Level_01, Level_02, ..., Level_10
        string path = $"Levels/Level_{levelNumber:00}";
        TextAsset json = Resources.Load<TextAsset>(path);

        if (json == null)
        {
            Debug.LogError($"Level JSON not found at Resources/{path}.json");
            return null;
        }

        return JsonUtility.FromJson<LevelData>(json.text);
    }
}