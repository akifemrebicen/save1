using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button levelButton;       // Assign the Button in the Inspector
    [SerializeField] private Text levelButtonText;     // Assign the Text child of the Button here

    private void Start()
    {
        int lastPlayedLevel = PlayerPrefs.GetInt("LastPlayedLevel", 0);

        if (lastPlayedLevel == 10)
        {
            levelButtonText.text = "Level Completed";
            levelButton.interactable = false;
        }
        else
        {
            levelButtonText.text = $"Level {lastPlayedLevel + 1}";
            levelButton.onClick.AddListener(LoadLevelScene);
        }
    }

    private void LoadLevelScene()
    {
        int currentLevel = PlayerPrefs.GetInt("LastPlayedLevel", 1);
        SceneManager.LoadScene("LevelScene");
    }
}