using UnityEngine;
using System.Collections.Generic;

public class BubbleGrid : MonoBehaviour
{

    [Header("Prefab & Sprites")]
    public GameObject bubblePrefab;
    public Sprite[] bubbleSprites;

    [Header("Grid Settings")]
    public int totalRows = 13;
    public int startingRows = 4;
    public int maxColumns = 8;

    public float bubbleSpacingX = 0.60f;
    public float bubbleSpacingY = 0.52f;

    // X = 0 để căn giữa toàn bộ lưới
    public Vector2 startPosition = new Vector2(0f, 4.15f);

    public Bubble[,] grid;


    [Header("Lose Settings")]
    public Transform loseLine;
    public float loseOffset = 0.02f;

    private StartMenuManager startMenuManager;
    private void Start()
    {
        startMenuManager = FindObjectOfType<StartMenuManager>();
        GenerateGrid();
    }

    private int GetColumnCountForRow(int row)
    {
        // Hàng chẵn: 8 bóng
        // Hàng lẻ: 7 bóng
        return row % 2 == 0 ? maxColumns : maxColumns - 1;
    }

    public void GenerateGrid()
    {
        grid = new Bubble[totalRows, maxColumns];

        for (int row = 0; row < startingRows; row++)
        {
            int colCount = GetColumnCountForRow(row);

            for (int col = 0; col < colCount; col++)
            {
                Vector2 spawnPos = GetWorldPosition(row, col);

                GameObject newBubble = Instantiate(
                    bubblePrefab,
                    spawnPos,
                    Quaternion.identity,
                    transform
                );

                Bubble bubble = newBubble.GetComponent<Bubble>();

                int randomColorIndex = Random.Range(0, bubbleSprites.Length);

                bubble.Init((BubbleColor)randomColorIndex, bubbleSprites[randomColorIndex]);
                bubble.row = row;
                bubble.column = col;
                bubble.isConnectedToGrid = true;

                Rigidbody2D rb = newBubble.GetComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0;
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0;

                grid[row, col] = bubble;

                SnapBubbleToGrid(bubble);
            }
        }

        SnapAllBubblesToGrid();
    }

    public Vector2 GetWorldPosition(int row, int col)
    {
        int colCount = GetColumnCountForRow(row);

        // Căn giữa từng hàng.
        // Vì vậy hàng ngang sẽ thẳng, không bị quả cao quả thấp.
        float rowWidth = (colCount - 1) * bubbleSpacingX;
        float rowStartX = -rowWidth / 2f;

        float x = startPosition.x + rowStartX + col * bubbleSpacingX;
        float y = startPosition.y - row * bubbleSpacingY;

        return new Vector2(x, y);
    }

    public void AttachBubble(GameObject bubbleObject, Bubble hitBubble)
    {
        Bubble bubble = bubbleObject.GetComponent<Bubble>();

        List<Vector2Int> candidateCells;

        if (hitBubble != null)
        {
            // Bóng bắn lên sẽ chỉ được dính vào ô trống quanh quả vừa va chạm
            candidateCells = GetEmptyNeighborCells(hitBubble);
        }
        else
        {
            // Nếu chạm trần thì lấy ô trống gần nhất trên lưới
            candidateCells = GetAllEmptyCells();
        }

        if (candidateCells.Count == 0)
        {
            Destroy(bubbleObject);
            return;
        }

        Vector2Int bestCell = candidateCells[0];
        float bestDistance = Mathf.Infinity;

        foreach (Vector2Int cell in candidateCells)
        {
            Vector2 cellPos = GetWorldPosition(cell.x, cell.y);
            float distance = Vector2.Distance(bubbleObject.transform.position, cellPos);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestCell = cell;
            }
        }

        int bestRow = bestCell.x;
        int bestCol = bestCell.y;

        bubbleObject.transform.SetParent(transform);

        bubble.row = bestRow;
        bubble.column = bestCol;
        bubble.isConnectedToGrid = true;

        Rigidbody2D rb = bubbleObject.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        grid[bestRow, bestCol] = bubble;

        SnapBubbleToGrid(bubble);
        SnapAllBubblesToGrid();

        CheckMatchAndFloating(bubble);

        SnapAllBubblesToGrid();

        // Kiểm tra Game Over sau khi bóng đã dính vào lưới
        CheckLoseCondition();
    }

    private void SnapBubbleToGrid(Bubble bubble)
    {
        if (bubble == null) return;

        bubble.transform.position = GetWorldPosition(bubble.row, bubble.column);
        bubble.transform.rotation = Quaternion.identity;
        bubble.transform.localScale = bubblePrefab.transform.localScale;

        Rigidbody2D rb = bubble.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
        }
    }

    private void SnapAllBubblesToGrid()
    {
        for (int row = 0; row < totalRows; row++)
        {
            int colCount = GetColumnCountForRow(row);

            for (int col = 0; col < colCount; col++)
            {
                Bubble bubble = grid[row, col];

                if (bubble == null) continue;

                bubble.row = row;
                bubble.column = col;
                bubble.isConnectedToGrid = true;

                SnapBubbleToGrid(bubble);
            }
        }
    }

    private List<Vector2Int> GetAllEmptyCells()
    {
        List<Vector2Int> emptyCells = new List<Vector2Int>();

        for (int row = 0; row < totalRows; row++)
        {
            int colCount = GetColumnCountForRow(row);

            for (int col = 0; col < colCount; col++)
            {
                if (grid[row, col] == null)
                {
                    emptyCells.Add(new Vector2Int(row, col));
                }
            }
        }

        return emptyCells;
    }

    private List<Vector2Int> GetEmptyNeighborCells(Bubble bubble)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        int row = bubble.row;
        int col = bubble.column;

        int[,] evenDirections =
        {
            { 0, -1 },
            { 0, 1 },
            { -1, -1 },
            { -1, 0 },
            { 1, -1 },
            { 1, 0 }
        };

        int[,] oddDirections =
        {
            { 0, -1 },
            { 0, 1 },
            { -1, 0 },
            { -1, 1 },
            { 1, 0 },
            { 1, 1 }
        };

        int[,] directions = row % 2 == 0 ? evenDirections : oddDirections;

        for (int i = 0; i < 6; i++)
        {
            int newRow = row + directions[i, 0];
            int newCol = col + directions[i, 1];

            if (newRow < 0 || newRow >= totalRows) continue;
            if (newCol < 0 || newCol >= GetColumnCountForRow(newRow)) continue;

            if (grid[newRow, newCol] == null)
            {
                cells.Add(new Vector2Int(newRow, newCol));
            }
        }

        return cells;
    }

    private void CheckMatchAndFloating(Bubble bubble)
    {
        List<Bubble> matches = FindSameColorGroup(bubble);

        if (matches.Count >= 3)
        {
            foreach (Bubble matchBubble in matches)
            {
                RemoveBubble(matchBubble);
            }

            DropFloatingBubbles();

            SnapAllBubblesToGrid();
            CheckWinCondition();
        }
    }

    private List<Bubble> FindSameColorGroup(Bubble startBubble)
    {
        List<Bubble> result = new List<Bubble>();
        Queue<Bubble> queue = new Queue<Bubble>();
        HashSet<Bubble> visited = new HashSet<Bubble>();

        queue.Enqueue(startBubble);
        visited.Add(startBubble);

        while (queue.Count > 0)
        {
            Bubble current = queue.Dequeue();
            result.Add(current);

            List<Bubble> neighbors = GetNeighbors(current);

            foreach (Bubble neighbor in neighbors)
            {
                if (neighbor == null) continue;
                if (visited.Contains(neighbor)) continue;
                if (neighbor.bubbleColor != startBubble.bubbleColor) continue;

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        return result;
    }

    private List<Bubble> GetNeighbors(Bubble bubble)
    {
        List<Bubble> neighbors = new List<Bubble>();

        int row = bubble.row;
        int col = bubble.column;

        int[,] evenDirections =
        {
            { 0, -1 },
            { 0, 1 },
            { -1, -1 },
            { -1, 0 },
            { 1, -1 },
            { 1, 0 }
        };

        int[,] oddDirections =
        {
            { 0, -1 },
            { 0, 1 },
            { -1, 0 },
            { -1, 1 },
            { 1, 0 },
            { 1, 1 }
        };

        int[,] directions = row % 2 == 0 ? evenDirections : oddDirections;

        for (int i = 0; i < 6; i++)
        {
            int newRow = row + directions[i, 0];
            int newCol = col + directions[i, 1];

            if (newRow < 0 || newRow >= totalRows) continue;
            if (newCol < 0 || newCol >= GetColumnCountForRow(newRow)) continue;

            if (grid[newRow, newCol] != null)
            {
                neighbors.Add(grid[newRow, newCol]);
            }
        }

        return neighbors;
    }

    private void RemoveBubble(Bubble bubble)
    {
        if (bubble == null) return;

        grid[bubble.row, bubble.column] = null;
        Destroy(bubble.gameObject);
    }

    private void DropFloatingBubbles()
    {
        HashSet<Bubble> connectedToTop = new HashSet<Bubble>();
        Queue<Bubble> queue = new Queue<Bubble>();

        int topColCount = GetColumnCountForRow(0);

        for (int col = 0; col < topColCount; col++)
        {
            Bubble topBubble = grid[0, col];

            if (topBubble != null)
            {
                connectedToTop.Add(topBubble);
                queue.Enqueue(topBubble);
            }
        }

        while (queue.Count > 0)
        {
            Bubble current = queue.Dequeue();
            List<Bubble> neighbors = GetNeighbors(current);

            foreach (Bubble neighbor in neighbors)
            {
                if (neighbor == null) continue;
                if (connectedToTop.Contains(neighbor)) continue;

                connectedToTop.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        for (int row = 0; row < totalRows; row++)
        {
            int colCount = GetColumnCountForRow(row);

            for (int col = 0; col < colCount; col++)
            {
                Bubble bubble = grid[row, col];

                if (bubble != null && !connectedToTop.Contains(bubble))
                {
                    grid[row, col] = null;

                    Rigidbody2D rb = bubble.GetComponent<Rigidbody2D>();
                    CircleCollider2D collider2D = bubble.GetComponent<CircleCollider2D>();

                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.gravityScale = 2.5f;
                    collider2D.enabled = false;

                    Destroy(bubble.gameObject, 1.5f);
                }
            }
        }
    }
    private int GetRemainingBubbleCount()
    {
        int count = 0;

        for (int row = 0; row < totalRows; row++)
        {
            int colCount = GetColumnCountForRow(row);

            for (int col = 0; col < colCount; col++)
            {
                if (grid[row, col] != null)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private void CheckWinCondition()
    {
        if (GetRemainingBubbleCount() == 0)
        {
            if (startMenuManager == null)
            {
                startMenuManager = FindObjectOfType<StartMenuManager>();
            }

            if (startMenuManager != null)
            {
                startMenuManager.ShowYouWin();
            }
        }
    }
    private void CheckLoseCondition()
    {
        if (loseLine == null)
        {
            return;
        }

        float loseY = loseLine.position.y;

        for (int row = 0; row < totalRows; row++)
        {
            int colCount = GetColumnCountForRow(row);

            for (int col = 0; col < colCount; col++)
            {
                Bubble bubble = grid[row, col];

                if (bubble == null) continue;

                CircleCollider2D collider = bubble.GetComponent<CircleCollider2D>();

                float bubbleRadius = 0.25f;

                if (collider != null)
                {
                    bubbleRadius = collider.radius * bubble.transform.lossyScale.y;
                }

                float bubbleBottomY = bubble.transform.position.y - bubbleRadius;

                // Mép dưới quả bóng chạm hoặc vượt LoseLine thì thua
                if (bubbleBottomY <= loseY + loseOffset)
                {
                    if (startMenuManager == null)
                    {
                        startMenuManager = FindObjectOfType<StartMenuManager>();
                    }

                    if (startMenuManager != null)
                    {
                        startMenuManager.ShowGameOver();
                    }

                    return;
                }
            }
        }
    }
}
