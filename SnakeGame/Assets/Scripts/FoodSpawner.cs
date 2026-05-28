using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("References")]
    public SnakeController snake;
    public GameManager gameManager;

    [Header("Board Settings")]
    public float gridSize = 1f;
    public Vector2Int boardMin = new Vector2Int(-13, -6);
    public Vector2Int boardMax = new Vector2Int(13, 6);

    public Vector2Int FoodCell { get; private set; }

    private Vector3 defaultScale;
    private Coroutine popRoutine;
    private void Start()
    {
        if (snake == null)
            snake = FindObjectOfType<SnakeController>();

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
    }
    private void Awake()
    {
        defaultScale = transform.localScale;

        if (defaultScale == Vector3.zero)
            defaultScale = Vector3.one;
    }

    public void SpawnFood()
    {
        List<Vector2Int> freeCells = new List<Vector2Int>();

        for (int x = boardMin.x; x <= boardMax.x; x++)
        {
            for (int y = boardMin.y; y <= boardMax.y; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);

                if (snake == null || !snake.IsOnSnake(cell))
                {
                    freeCells.Add(cell);
                }
            }
        }

        if (freeCells.Count == 0)
        {
            if (gameManager != null)
                gameManager.WinGame();

            return;
        }

        FoodCell = freeCells[Random.Range(0, freeCells.Count)];
        transform.position = CellToWorld(FoodCell);

        if (popRoutine != null)
            StopCoroutine(popRoutine);

        popRoutine = StartCoroutine(PopEffect());
    }

    private IEnumerator PopEffect()
    {
        transform.localScale = Vector3.zero;

        float timer = 0f;

        while (timer < 1f)
        {
            timer += Time.unscaledDeltaTime * 8f;

            float scale = Mathf.SmoothStep(0.2f, 1f, timer);
            transform.localScale = defaultScale * scale;

            yield return null;
        }

        transform.localScale = defaultScale;
    }

    private Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(cell.x * gridSize, cell.y * gridSize, 0f);
    }
}