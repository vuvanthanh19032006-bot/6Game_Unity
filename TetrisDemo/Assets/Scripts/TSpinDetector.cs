using UnityEngine;
using System.Collections.Generic;

public class TSpinDetector : MonoBehaviour
{
    public enum TSpinType
    {
        None,
        TSpin,
        TSpinMini,
        TSpinSingle,
        TSpinDouble,
        TSpinTriple
    }

    private Board board;

    void Start()
    {
        board = Board.Instance;
    }

    /// <summary>
    /// Detect if the last placed piece was a T-spin
    /// </summary>
    public TSpinType DetectTSpin(Transform[] tetromino)
    {
        if (tetromino == null || tetromino.Length == 0)
            return TSpinType.None;

        // Check if this is a T-piece (has 4 blocks arranged in T shape)
        if (!IsTetromino(tetromino))
            return TSpinType.None;

        // Get piece positions
        Vector2Int[] positions = new Vector2Int[tetromino.Length];
        for (int i = 0; i < tetromino.Length; i++)
        {
            positions[i] = new Vector2Int(
                Mathf.RoundToInt(tetromino[i].position.x),
                Mathf.RoundToInt(tetromino[i].position.y)
            );
        }

        // Check if it's a T-piece shape
        if (!IsTShape(positions))
            return TSpinType.None;

        // Check for T-spin: piece must have walls on 3 sides (rotation with wall kick)
        if (IsTSpinPlacement(positions))
        {
            // Count cleared lines to determine T-spin type
            int clearedLines = CountClearedLines(positions);
            return GetTSpinType(clearedLines, positions);
        }

        return TSpinType.None;
    }

    private bool IsTetromino(Transform[] tetromino)
    {
        return tetromino.Length == 4;
    }

    private bool IsTShape(Vector2Int[] positions)
    {
        if (positions.Length != 4)
            return false;

        // T-piece has one block in center, 3 blocks around it
        // Find the center block (the one with highest "connectivity")
        for (int i = 0; i < positions.Length; i++)
        {
            Vector2Int center = positions[i];
            int connections = 0;

            // Count adjacent blocks
            for (int j = 0; j < positions.Length; j++)
            {
                if (i == j) continue;

                Vector2Int diff = positions[j] - center;
                if ((Mathf.Abs(diff.x) == 1 && diff.y == 0) || (diff.x == 0 && Mathf.Abs(diff.y) == 1))
                {
                    connections++;
                }
            }

            // T-piece center has 3 connections
            if (connections == 3)
                return true;
        }

        return false;
    }

    private bool IsTSpinPlacement(Vector2Int[] positions)
    {
        // Count empty spaces around the T-piece
        int wallCount = 0;

        foreach (Vector2Int pos in positions)
        {
            // Check 4 cardinal directions
            Vector2Int[] adjacentPositions = new Vector2Int[]
            {
                pos + Vector2Int.up,
                pos + Vector2Int.down,
                pos + Vector2Int.left,
                pos + Vector2Int.right
            };

            int adjacentWalls = 0;
            foreach (Vector2Int adjPos in adjacentPositions)
            {
                // Check if adjacent position is occupied or out of bounds
                if (adjPos.x < 0 || adjPos.x >= Board.width || adjPos.y < 0 || adjPos.y >= Board.height)
                {
                    adjacentWalls++;
                }
                else if (Board.grid[adjPos.x, adjPos.y] != null)
                {
                    adjacentWalls++;
                }
            }

            // If block has 3+ walls, it's a tight placement
            if (adjacentWalls >= 3)
                wallCount++;
        }

        // T-spin requires at least 2 blocks to be tightly placed
        return wallCount >= 2;
    }

    private int CountClearedLines(Vector2Int[] positions)
    {
        HashSet<int> filledLines = new HashSet<int>();

        foreach (Vector2Int pos in positions)
        {
            if (IsLineComplete(pos.y))
                filledLines.Add(pos.y);
        }

        return filledLines.Count;
    }

    private bool IsLineComplete(int y)
    {
        if (y < 0 || y >= Board.height)
            return false;

        for (int x = 0; x < Board.width; x++)
        {
            if (Board.grid[x, y] == null)
                return false;
        }

        return true;
    }

    private TSpinType GetTSpinType(int clearedLines, Vector2Int[] positions)
    {
        // Determine if it's a mini T-spin (rotation without line clear)
        if (clearedLines == 0)
        {
            if (IsMiniTSpin(positions))
                return TSpinType.TSpinMini;
            return TSpinType.TSpin;
        }

        // Regular T-spins with line clears
        return clearedLines switch
        {
            1 => TSpinType.TSpinSingle,
            2 => TSpinType.TSpinDouble,
            3 => TSpinType.TSpinTriple,
            _ => TSpinType.TSpin
        };
    }

    private bool IsMiniTSpin(Vector2Int[] positions)
    {
        // Mini T-spin: T-piece is on the side with one block protruding
        int edgeCount = 0;

        foreach (Vector2Int pos in positions)
        {
            if (pos.x == 0 || pos.x == Board.width - 1)
                edgeCount++;
        }

        return edgeCount >= 2;
    }

    /// <summary>
    /// Get bonus points for T-spin
    /// </summary>
    public int GetTSpinBonus(TSpinType spinType)
    {
        return spinType switch
        {
            TSpinType.TSpin => 100,
            TSpinType.TSpinMini => 50,
            TSpinType.TSpinSingle => 300,
            TSpinType.TSpinDouble => 600,
            TSpinType.TSpinTriple => 900,
            _ => 0
        };
    }
}
