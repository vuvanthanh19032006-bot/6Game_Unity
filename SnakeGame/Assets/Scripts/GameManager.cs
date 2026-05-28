using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public SnakeController snake;
    public FoodSpawner foodSpawner;

    [Header("In Game UI")]
    public GameObject scorePanel;
    public TMP_Text scoreText;

    [Header("Game Over UI")]
    public TMP_Text gameOverScoreText;
    public TMP_Text gameOverBestText;

    [Header("Panels")]
    public GameObject startPanel;
    public GameObject howToPlayPanel;
    public GameObject gameOverPanel;
    public GameObject pausePanel;

    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }

    private int score;
    private int bestScore;

    private const string BestScoreKey = "Snake_BestScore";

    private void Awake()
    {
        if (snake == null)
            snake = FindObjectOfType<SnakeController>();

        if (foodSpawner == null)
            foodSpawner = FindObjectOfType<FoodSpawner>();

        bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);

        UpdateUI();
    }

    private void Start()
    {
        ShowStartMenu();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && IsGameOver)
        {
            RestartGame();
        }

        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void ShowStartMenu()
    {
        Time.timeScale = 0f;

        IsGameOver = false;
        IsPaused = false;
        score = 0;

        if (startPanel != null)
            startPanel.SetActive(true);

        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (scorePanel != null)
            scorePanel.SetActive(false);

        UpdateUI();
    }

    public void StartGame()
    {
        Time.timeScale = 1f;

        IsGameOver = false;
        IsPaused = false;
        score = 0;

        if (startPanel != null)
            startPanel.SetActive(false);

        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (scorePanel != null)
            scorePanel.SetActive(true);

        UpdateUI();

        if (snake != null)
            snake.ResetSnake();

        if (foodSpawner != null)
            foodSpawner.SpawnFood();
    }

    public void ShowHowToPlay()
    {
        Time.timeScale = 0f;

        if (startPanel != null)
            startPanel.SetActive(false);

        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(true);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (scorePanel != null)
            scorePanel.SetActive(false);
    }

    public void BackToStartMenu()
    {
        Time.timeScale = 0f;

        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);

        if (startPanel != null)
            startPanel.SetActive(true);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (scorePanel != null)
            scorePanel.SetActive(false);
    }

    public void RestartGame()
    {
        StartGame();
    }

    public void BackToMenu()
    {
        ShowStartMenu();
    }

    public void AddScore(int amount)
    {
        score += 1;

        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt(BestScoreKey, bestScore);
            PlayerPrefs.Save();
        }

        UpdateUI();
    }

    public void GameOver()
    {
        IsGameOver = true;
        IsPaused = false;

        Time.timeScale = 0f;

        if (startPanel != null)
            startPanel.SetActive(false);

        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (scorePanel != null)
            scorePanel.SetActive(false);

        UpdateUI();
    }

    public void WinGame()
    {
        GameOver();
    }

    public void TogglePause()
    {
        if (IsGameOver) return;
        if (startPanel != null && startPanel.activeSelf) return;
        if (howToPlayPanel != null && howToPlayPanel.activeSelf) return;

        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;

        if (pausePanel != null)
            pausePanel.SetActive(IsPaused);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = score.ToString();

        if (gameOverScoreText != null)
            gameOverScoreText.text = score.ToString();

        if (gameOverBestText != null)
            gameOverBestText.text = bestScore.ToString();
    }

    [ContextMenu("Reset Best Score")]
    public void ResetBestScore()
    {
        PlayerPrefs.DeleteKey(BestScoreKey);
        bestScore = 0;
        UpdateUI();
    }
}