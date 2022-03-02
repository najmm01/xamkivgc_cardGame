using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] private ScoreData scoreData;
    [SerializeField] private TMPro.TextMeshProUGUI info;

    private void Start()
    {
        info.text = $"Demon's Killed: {scoreData.killCount}\nScore: {scoreData.score}";
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
