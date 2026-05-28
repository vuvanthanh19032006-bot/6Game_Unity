using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameObject[] tetrominoes;
    public Transform spawnPoint;
    public Transform[] previewSlots;
    public Transform holdSlot;
    public GameObject miniBlockPrefab;
    public float previewScale = 9f;

    [Header("Visual Theme")]
    public Camera targetCamera;
    public Color cameraBackgroundColor = new Color(0.05f, 0.07f, 0.12f);

    public bool useThemeColors = true;

    [Header("Piece Mapping")]
    public int tPieceIndex = 2;

    public Color[] themeColors = new Color[]
    {
        new Color(0.24f, 0.82f, 0.96f),
        new Color(0.98f, 0.87f, 0.24f),
        new Color(0.66f, 0.51f, 0.95f),
        new Color(0.35f, 0.90f, 0.43f),
        new Color(0.97f, 0.34f, 0.34f),
        new Color(0.9568628f, 0.5372549f, 0.9568628f),
        new Color(0.30f, 0.51f, 0.98f)
    };

    [Header("Score")]
    public int score = 0;
    public Text scoreText;
    public Text linesText;
    public Text levelText;
    public Text nextPieceText;

    [Header("Speed")]
    public float baseFallTime = 1f;
    public float minFallTime = 0.1f;
    public float speedUpPerLevel = 0.08f;

    [Header("UI")]
    public GameObject mainMenuUI;
    public GameObject gameUI;
    public GameObject gameOverUI;

    public bool isGameOver = false;

    int totalLines = 0;
    int level = 1;
    int heldPieceIndex = -1;
    bool holdUsedThisTurn = false;
    int comboCount = -1;
    bool backToBackDifficult = false;

    readonly Queue<int> nextQueue = new Queue<int>();

    void Start()
    {
        EnsureEventSystem();

        if (mainMenuUI != null)
            mainMenuUI.SetActive(true);

        if (gameUI != null)
            gameUI.SetActive(false);

        SetGameOverUIState(false);

        ApplyVisualTheme();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuMusic();
    }

    public void StartGame()
    {
        if (mainMenuUI != null)
            mainMenuUI.SetActive(false);

        if (gameUI != null)
            gameUI.SetActive(true);

        ResetAndStartRun();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameMusic();
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void ApplyVisualTheme()
    {
        Camera cam = targetCamera != null ? targetCamera : Camera.main;

        if (cam != null)
            cam.backgroundColor = cameraBackgroundColor;
    }

    public void SpawnNew()
    {
        if (isGameOver) return;
        if (tetrominoes == null || tetrominoes.Length == 0) return;

        int index = GetNextTetrominoIndex();

        SpawnPiece(index);
        UpdateNextPieceUI();

        holdUsedThisTurn = false;
    }

    void SpawnPiece(int index)
    {
        GameObject piece = Instantiate(tetrominoes[index], spawnPoint.position, Quaternion.identity);

        Tetromino tetromino = piece.GetComponent<Tetromino>();

        if (tetromino != null)
            tetromino.SetPieceIndex(index);

        if (!Board.IsValidPosition(piece.transform))
        {
            Destroy(piece);
            GameOver();
        }
    }

    public void AddScore(int lines)
    {
        RegisterLineClear(lines, false);
    }

    public void RegisterLineClear(int lines, bool isTSpin)
    {
        if (lines <= 0)
        {
            comboCount = -1;
            return;
        }

        totalLines += lines;
        level = Mathf.Max(1, (totalLines / 10) + 1);

        int baseScore = 0;

        if (isTSpin)
        {
            switch (lines)
            {
                case 1:
                    baseScore = 800;
                    break;

                case 2:
                    baseScore = 1200;
                    break;

                case 3:
                    baseScore = 1600;
                    break;
            }
        }
        else
        {
            switch (lines)
            {
                case 1:
                    baseScore = 100;
                    break;

                case 2:
                    baseScore = 300;
                    break;

                case 3:
                    baseScore = 500;
                    break;

                case 4:
                    baseScore = 800;
                    break;
            }
        }

        bool difficult = isTSpin || lines == 4;

        if (difficult && backToBackDifficult)
            baseScore = Mathf.RoundToInt(baseScore * 1.5f);

        backToBackDifficult = difficult;

        comboCount++;

        int comboBonus = comboCount > 0 ? comboCount * 50 : 0;

        score += (baseScore * level) + (comboBonus * level);

        UpdateScoreUI();
    }

    public void AddDropScore(int cells, bool hardDrop)
    {
        // Không cộng điểm khi bấm S, phím xuống hoặc hard drop.
        // Điểm chỉ được cộng khi xóa hàng trong RegisterLineClear().
    }

    public float GetFallTime()
    {
        float adjusted = baseFallTime - ((level - 1) * speedUpPerLevel);
        return Mathf.Max(minFallTime, adjusted);
    }

    int GetNextTetrominoIndex()
    {
        if (nextQueue.Count == 0)
            RefillBag();

        int index = nextQueue.Dequeue();

        if (nextQueue.Count == 0)
            RefillBag();

        return index;
    }

    void RefillBag()
    {
        List<int> bag = new List<int>();

        for (int i = 0; i < tetrominoes.Length; i++)
            bag.Add(i);

        for (int i = 0; i < bag.Count; i++)
        {
            int randomIndex = Random.Range(i, bag.Count);

            int temp = bag[i];
            bag[i] = bag[randomIndex];
            bag[randomIndex] = temp;
        }

        for (int i = 0; i < bag.Count; i++)
            nextQueue.Enqueue(bag[i]);
    }

    void UpdateNextPieceUI()
    {
        if (previewSlots == null || previewSlots.Length == 0) return;

        List<int> previewList = new List<int>(nextQueue);

        for (int i = 0; i < previewSlots.Length; i++)
        {
            Transform slot = previewSlots[i];

            ClearGeneratedPreview(slot);

            if (i >= previewList.Count) continue;

            int pieceIndex = previewList[i];

            DrawPreviewPiece(pieceIndex, slot);
        }

        UpdateHoldUI();
    }

    void DrawPreviewPiece(int index, Transform parent)
    {
        if (tetrominoes == null) return;
        if (index < 0 || index >= tetrominoes.Length) return;

        GameObject prefab = tetrominoes[index];
        bool canUseMiniPrefab = miniBlockPrefab != null && !HasMissingMonoScripts(miniBlockPrefab);

        float spacing = 1f;

        foreach (Transform block in prefab.transform)
        {
            GameObject mini;

            if (canUseMiniPrefab)
            {
                mini = Instantiate(miniBlockPrefab, parent);
            }
            else
            {
                mini = new GameObject("MiniBlock");
                mini.transform.SetParent(parent, false);

                SpriteRenderer src = block.GetComponent<SpriteRenderer>();
                SpriteRenderer dst = mini.AddComponent<SpriteRenderer>();

                if (src != null)
                    dst.sprite = src.sprite;
            }

            mini.name = "PreviewBlock";

            Vector2 pos = block.localPosition;
            mini.transform.localPosition = pos * spacing;

            SpriteRenderer sr = mini.GetComponent<SpriteRenderer>();

            if (sr != null)
                sr.color = GetPieceColor(index);
        }
    }

    bool HasMissingMonoScripts(GameObject obj)
    {
        if (obj == null) return false;

        MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();

        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null)
                return true;
        }

        return false;
    }

    void UpdateHoldUI()
    {
        if (holdSlot == null) return;

        ClearGeneratedPreview(holdSlot);

        if (heldPieceIndex >= 0)
            DrawPreviewPiece(heldPieceIndex, holdSlot);
    }

    void ClearGeneratedPreview(Transform parent)
    {
        if (parent == null) return;

        List<GameObject> toRemove = new List<GameObject>();

        foreach (Transform child in parent)
        {
            if (child != null && child.name == "PreviewBlock")
                toRemove.Add(child.gameObject);
        }

        for (int i = 0; i < toRemove.Count; i++)
            Destroy(toRemove[i]);
    }

    Color GetPieceColor(int index)
    {
        if (themeColors != null && themeColors.Length > 0)
            return themeColors[index % themeColors.Length];

        return Color.HSVToRGB((index * 0.14f) % 1f, 0.75f, 0.95f);
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = score.ToString();

        if (levelText != null)
            levelText.text = level.ToString();

        if (linesText != null)
            linesText.text = totalLines.ToString();
    }

    void GameOver()
    {
        isGameOver = true;

        Debug.Log("GAME OVER");

        SetGameOverUIState(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.gameOverClip);
    }

    public void TriggerGameOver()
    {
        if (!isGameOver)
            GameOver();
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void RestartGame()
    {
        Debug.Log("RESTART CLICKED");

        if (mainMenuUI != null)
            mainMenuUI.SetActive(false);

        if (gameUI != null)
            gameUI.SetActive(true);

        ResetAndStartRun();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameMusic();
    }

    public bool TryHoldPiece(Tetromino currentPiece)
    {
        if (isGameOver || holdUsedThisTurn || currentPiece == null)
            return false;

        int currentIndex = currentPiece.GetPieceIndex();

        if (currentIndex < 0 || currentIndex >= tetrominoes.Length)
            return false;

        holdUsedThisTurn = true;

        if (heldPieceIndex < 0)
        {
            heldPieceIndex = currentIndex;

            Destroy(currentPiece.gameObject);

            UpdateHoldUI();
            SpawnNew();

            return true;
        }

        int spawnIndex = heldPieceIndex;

        heldPieceIndex = currentIndex;

        Destroy(currentPiece.gameObject);

        UpdateHoldUI();
        SpawnPiece(spawnIndex);

        return true;
    }

    void ResetAndStartRun()
    {
        isGameOver = false;

        score = 0;
        totalLines = 0;
        level = 1;

        heldPieceIndex = -1;
        holdUsedThisTurn = false;

        comboCount = -1;
        backToBackDifficult = false;

        StopAllCoroutines();

        if (Board.Instance != null)
            Board.Instance.StopAllCoroutines();

        Tetromino[] allTetrominoes = FindObjectsOfType<Tetromino>();

        foreach (Tetromino t in allTetrominoes)
        {
            if (t != null)
                Destroy(t.gameObject);
        }

        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObjects)
        {
            if (go != null && go.name.EndsWith("_Ghost"))
                Destroy(go);
        }

        ClearBoard();

        nextQueue.Clear();
        RefillBag();

        SetGameOverUIState(false);

        UpdateScoreUI();
        UpdateNextPieceUI();
        UpdateHoldUI();

        SpawnNew();
    }

    void EnsureEventSystem()
    {
        EventSystem existing = FindObjectOfType<EventSystem>();

        if (existing != null)
        {
            if (existing.GetComponent<BaseInputModule>() == null)
                existing.gameObject.AddComponent<StandaloneInputModule>();

            return;
        }

        GameObject es = new GameObject("EventSystem");

        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();

        if (es.GetComponent<InputSystemUIInputModule>() == null)
            es.AddComponent<InputSystemUIInputModule>();
    }

    void SetGameOverUIState(bool active)
    {
        if (gameOverUI == null)
            return;

        gameOverUI.SetActive(active);

        CanvasGroup group = gameOverUI.GetComponent<CanvasGroup>();

        if (group != null)
        {
            group.alpha = active ? 1f : 0f;
            group.interactable = active;
            group.blocksRaycasts = active;
        }
    }

    void ClearBoard()
    {
        for (int x = 0; x < Board.width; x++)
        {
            for (int y = 0; y < Board.height; y++)
            {
                if (Board.grid[x, y] != null)
                {
                    Destroy(Board.grid[x, y].gameObject);
                    Board.grid[x, y] = null;
                }
            }
        }
    }
}