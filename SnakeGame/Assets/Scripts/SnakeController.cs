using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SnakeController : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public FoodSpawner foodSpawner;
    public Transform bodyPrefab;

    [Header("Snake Settings")]
    public int initialSize = 4;
    public int pointsPerFood = 1;

    [Header("Difficulty Speed")]
    public float speedScore0 = 0.14f;   // 0 - 4 điểm
    public float speedScore5 = 0.11f;   // 5 - 9 điểm
    public float speedScore10 = 0.085f; // 10 - 14 điểm
    public float speedScore15 = 0.065f; // 15+ điểm

    [Header("Board Settings")]
    public float gridSize = 1f;
    public Vector2Int startCell = new Vector2Int(0, -1);
    public Vector2Int boardMin = new Vector2Int(-13, -6);
    public Vector2Int boardMax = new Vector2Int(13, 4);

    [Header("Mobile Swipe Settings")]
    public float minSwipeDistance = 60f;

    private readonly List<Transform> segments = new List<Transform>();

    private Vector2Int direction = Vector2Int.right;
    private Vector2Int nextDirection = Vector2Int.right;

    private float moveDelay;
    private float moveTimer;

    private int currentScore;

    private Vector2 swipeStartPosition;
    private Vector2 swipeEndPosition;
    private bool isSwiping;

    private void Awake()
    {
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

        ReadKeyboardInput();
        ReadSwipeInput();

        moveTimer += Time.deltaTime;

        if (moveTimer >= moveDelay)
        {
            moveTimer = 0f;
            MoveSnake();
        }
    }

    public void ResetSnake()
    {
        currentScore = 0;
        moveDelay = speedScore0;
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

            Vector2Int bodyCell = startCell - direction * i;

            Transform body = Instantiate(
                bodyPrefab,
                CellToWorld(bodyCell),
                Quaternion.identity
            );

            body.name = "Snake Body";
            body.SetParent(transform.parent);
            segments.Add(body);
        }
    }

    private void MoveSnake()
    {
        direction = nextDirection;

        Vector2Int currentHeadCell = WorldToCell(transform.position);
        Vector2Int nextCell = currentHeadCell + direction;

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

            currentScore += 1;
            UpdateDifficulty();

            if (gameManager != null)
                gameManager.AddScore(pointsPerFood);

            if (foodSpawner != null)
                foodSpawner.SpawnFood();
        }
    }

    private void UpdateDifficulty()
    {
        if (currentScore >= 15)
        {
            moveDelay = speedScore15;
        }
        else if (currentScore >= 10)
        {
            moveDelay = speedScore10;
        }
        else if (currentScore >= 5)
        {
            moveDelay = speedScore5;
        }
        else
        {
            moveDelay = speedScore0;
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

    private void ReadKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            TryChangeDirection(Vector2Int.up);

        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            TryChangeDirection(Vector2Int.down);

        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            TryChangeDirection(Vector2Int.left);

        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            TryChangeDirection(Vector2Int.right);
    }

    private void ReadSwipeInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;

            if (touch.phase == TouchPhase.Began)
            {
                swipeStartPosition = touch.position;
                isSwiping = true;
            }
            else if (touch.phase == TouchPhase.Ended && isSwiping)
            {
                swipeEndPosition = touch.position;
                DetectSwipe(swipeEndPosition - swipeStartPosition);
                isSwiping = false;
            }

            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            swipeStartPosition = Input.mousePosition;
            isSwiping = true;
        }

        if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            swipeEndPosition = Input.mousePosition;
            DetectSwipe(swipeEndPosition - swipeStartPosition);
            isSwiping = false;
        }
    }

    private void DetectSwipe(Vector2 swipeDelta)
    {
        if (swipeDelta.magnitude < minSwipeDistance)
            return;

        float horizontal = Mathf.Abs(swipeDelta.x);
        float vertical = Mathf.Abs(swipeDelta.y);

        if (horizontal > vertical)
        {
            if (swipeDelta.x > 0)
                TryChangeDirection(Vector2Int.right);
            else
                TryChangeDirection(Vector2Int.left);
        }
        else
        {
            if (swipeDelta.y > 0)
                TryChangeDirection(Vector2Int.up);
            else
                TryChangeDirection(Vector2Int.down);
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
            if (segments[i] != null && WorldToCell(segments[i].position) == nextCell)
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