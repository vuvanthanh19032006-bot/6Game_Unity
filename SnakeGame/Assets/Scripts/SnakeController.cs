using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public FoodSpawner foodSpawner;
    public Transform bodyPrefab;

    [Header("Snake Settings")]
    public int initialSize = 4;
    public int pointsPerFood = 1;
    public float moveDelay = 0.12f;
    public float minMoveDelay = 0.055f;
    public float speedUpRate = 0.985f;

    [Header("Board Settings")]
    public float gridSize = 1f;
    public Vector2Int startCell = Vector2Int.zero;
    public Vector2Int boardMin = new Vector2Int(-13, -6);
    public Vector2Int boardMax = new Vector2Int(13, 6);

    private readonly List<Transform> segments = new List<Transform>();

    private Vector2Int direction = Vector2Int.right;
    private Vector2Int nextDirection = Vector2Int.right;

    private float moveTimer;
    private float defaultMoveDelay;
    private Vector2 swipeStart;

    private void Awake()
    {
        defaultMoveDelay = moveDelay;

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (foodSpawner == null)
            foodSpawner = FindObjectOfType<FoodSpawner>();
    }

    private void Start()
    {
        if (segments.Count == 0)
        {
            ResetSnake();
        }
    }

    private void Update()
    {
        if (gameManager != null)
        {
            if (gameManager.IsGameOver || gameManager.IsPaused)
                return;
        }

        if (segments.Count == 0)
            return;

        ReadInput();

        moveTimer += Time.deltaTime;

        if (moveTimer >= moveDelay)
        {
            moveTimer = 0f;
            MoveSnake();
        }
    }

    public void ResetSnake()
    {
        moveDelay = defaultMoveDelay;
        moveTimer = 0f;

        direction = Vector2Int.right;
        nextDirection = Vector2Int.right;

        for (int i = 1; i < segments.Count; i++)
        {
            if (segments[i] != null)
                Destroy(segments[i].gameObject);
        }

        segments.Clear();

        transform.position = CellToWorld(startCell);
        transform.rotation = Quaternion.identity;
        segments.Add(transform);

        for (int i = 1; i < initialSize; i++)
        {
            if (bodyPrefab == null)
            {
                Debug.LogWarning("SnakeController: Chưa gán Body Prefab.");
                return;
            }

            Vector2Int cell = startCell - direction * i;
            Transform body = Instantiate(bodyPrefab, CellToWorld(cell), Quaternion.identity);
            body.name = "Snake Body";
            body.SetParent(transform.parent);
            segments.Add(body);
        }
    }

    private void MoveSnake()
    {
        if (segments.Count == 0)
            return;

        direction = nextDirection;

        Vector2Int headCell = WorldToCell(transform.position);
        Vector2Int nextCell = headCell + direction;

        if (IsOutsideBoard(nextCell))
        {
            if (gameManager != null)
                gameManager.GameOver();

            return;
        }

        if (HitsSelf(nextCell))
        {
            if (gameManager != null)
                gameManager.GameOver();

            return;
        }

        bool ateFood = foodSpawner != null && nextCell == foodSpawner.FoodCell;

        Vector3 tailPosition = segments[segments.Count - 1].position;

        for (int i = segments.Count - 1; i > 0; i--)
        {
            segments[i].position = segments[i - 1].position;
        }

        transform.position = CellToWorld(nextCell);
        RotateHead();

        if (ateFood)
        {
            Grow(tailPosition);

            if (gameManager != null)
                gameManager.AddScore(pointsPerFood);

            moveDelay = Mathf.Max(minMoveDelay, moveDelay * speedUpRate);

            if (foodSpawner != null)
                foodSpawner.SpawnFood();
        }
    }

    private void Grow(Vector3 position)
    {
        if (bodyPrefab == null)
        {
            Debug.LogWarning("SnakeController: Chưa gán Body Prefab.");
            return;
        }

        Transform body = Instantiate(bodyPrefab, position, Quaternion.identity);
        body.name = "Snake Body";
        body.SetParent(transform.parent);
        segments.Add(body);
    }

    private void ReadInput()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            TryChangeDirection(Vector2Int.up);

        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            TryChangeDirection(Vector2Int.down);

        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            TryChangeDirection(Vector2Int.left);

        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            TryChangeDirection(Vector2Int.right);

        ReadSwipeInput();
    }

    private void ReadSwipeInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            swipeStart = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - swipeStart;
            DetectSwipe(delta);
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
                swipeStart = touch.position;

            if (touch.phase == TouchPhase.Ended)
            {
                Vector2 delta = touch.position - swipeStart;
                DetectSwipe(delta);
            }
        }
    }

    private void DetectSwipe(Vector2 delta)
    {
        if (delta.magnitude < 80f)
            return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            TryChangeDirection(delta.x > 0 ? Vector2Int.right : Vector2Int.left);
        }
        else
        {
            TryChangeDirection(delta.y > 0 ? Vector2Int.up : Vector2Int.down);
        }
    }

    private void TryChangeDirection(Vector2Int newDirection)
    {
        if (IsOpposite(newDirection, direction))
            return;

        if (IsOpposite(newDirection, nextDirection))
            return;

        nextDirection = newDirection;
    }

    private bool IsOpposite(Vector2Int a, Vector2Int b)
    {
        return a.x + b.x == 0 && a.y + b.y == 0;
    }

    private bool IsOutsideBoard(Vector2Int cell)
    {
        return cell.x < boardMin.x ||
               cell.x > boardMax.x ||
               cell.y < boardMin.y ||
               cell.y > boardMax.y;
    }

    private bool HitsSelf(Vector2Int nextCell)
    {
        bool willEat = foodSpawner != null && nextCell == foodSpawner.FoodCell;

        int checkCount = willEat ? segments.Count : segments.Count - 1;

        for (int i = 1; i < checkCount; i++)
        {
            if (WorldToCell(segments[i].position) == nextCell)
                return true;
        }

        return false;
    }

    public bool IsOnSnake(Vector2Int cell)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] != null && WorldToCell(segments[i].position) == cell)
                return true;
        }

        return false;
    }

    private void RotateHead()
    {
        if (direction == Vector2Int.right)
            transform.rotation = Quaternion.Euler(0, 0, 0);

        else if (direction == Vector2Int.up)
            transform.rotation = Quaternion.Euler(0, 0, 90);

        else if (direction == Vector2Int.left)
            transform.rotation = Quaternion.Euler(0, 0, 180);

        else if (direction == Vector2Int.down)
            transform.rotation = Quaternion.Euler(0, 0, -90);
    }

    private Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(cell.x * gridSize, cell.y * gridSize, 0f);
    }

    private Vector2Int WorldToCell(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x / gridSize),
            Mathf.RoundToInt(worldPosition.y / gridSize)
        );
    }
}