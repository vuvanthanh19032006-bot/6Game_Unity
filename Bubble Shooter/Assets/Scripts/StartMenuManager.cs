using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject startMenu;
    public GameObject gameplayRoot;
    public GameObject youWinPanel;
    public GameObject gameOverPanel;

    private bool gameEnded = false;
    private static bool restartAndPlayImmediately = false;

    private void Start()
    {
        if (restartAndPlayImmediately)
        {
            restartAndPlayImmediately = false;
            PlayGame();
        }
        else
        {
            ShowStartMenu();
        }
    }

    private void ShowStartMenu()
    {
        gameEnded = false;

        if (startMenu != null)
            startMenu.SetActive(true);

        if (gameplayRoot != null)
            gameplayRoot.SetActive(false);

        if (youWinPanel != null)
            youWinPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void PlayGame()
    {
        gameEnded = false;

        if (startMenu != null)
            startMenu.SetActive(false);

        if (gameplayRoot != null)
            gameplayRoot.SetActive(true);

        if (youWinPanel != null)
            youWinPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void ShowYouWin()
    {
        if (gameEnded) return;

        gameEnded = true;

        if (youWinPanel != null)
            youWinPanel.SetActive(true);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (gameplayRoot != null)
            gameplayRoot.SetActive(false);

        Time.timeScale = 1f;
    }

    public void ShowGameOver()
    {
        if (gameEnded) return;

        gameEnded = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (youWinPanel != null)
            youWinPanel.SetActive(false);

        if (gameplayRoot != null)
            gameplayRoot.SetActive(false);

        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        // Restart xong vào thẳng gameplay
        restartAndPlayImmediately = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;

        // Về lại màn hình Play ban đầu
        restartAndPlayImmediately = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}