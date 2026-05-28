using UnityEngine;
using UnityEngine.EventSystems;

public class Tetromino : MonoBehaviour
{
    Vector2 touchStartPos;
    float softDropUntilTime;
    const float tapThreshold = 40f;
    const float swipeThreshold = 80f;
    float previousTime;
    GameManager gm;
    Transform ghostRoot;
    int pieceIndex = -1;
    bool lastActionWasRotate;

    public void SetPieceIndex(int index)
    {
        pieceIndex = index;
    }

    public int GetPieceIndex()
    {
        return pieceIndex;
    }

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        CreateGhost();
        UpdateGhost();
    }
    void Update()
    {
        if (gm == null) return;

        // stop if game over
        if (gm.IsGameOver()) return;

        HandleInput();
        HandleTouchInput();
        HandleFall();
        UpdateGhost();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            Move(Vector3.left);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            Move(Vector3.right);

        if (Input.GetKeyDown(KeyCode.UpArrow))
            Rotate();

        if (Input.GetKeyDown(KeyCode.Space))
            HardDrop();

        if (Input.GetKeyDown(KeyCode.C))
            Hold();
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            return;

        if (touch.phase == TouchPhase.Began)
        {
            touchStartPos = touch.position;
        }

        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            Vector2 delta = touch.position - touchStartPos;
            float absX = Mathf.Abs(delta.x);
            float absY = Mathf.Abs(delta.y);
            float normalizedX = touch.position.x / Mathf.Max(1f, Screen.width);
            float normalizedY = touch.position.y / Mathf.Max(1f, Screen.height);

            if (absX < tapThreshold && absY < tapThreshold)
            {
                if (normalizedX < 0.3f && normalizedY > 0.7f)
                    Hold();
                else if (normalizedX < 0.33f)
                    Move(Vector3.left);
                else if (normalizedX > 0.66f)
                    Move(Vector3.right);
                else
                    Rotate();

                return;
            }

            if (absY > absX)
            {
                if (delta.y > swipeThreshold)
                    HardDrop();
                else if (delta.y < -swipeThreshold)
                    softDropUntilTime = Time.time + 0.35f;
            }
            else
            {
                if (delta.x > swipeThreshold)
                    Move(Vector3.right);
                else if (delta.x < -swipeThreshold)
                    Move(Vector3.left);
            }
        }
    }


    void HandleFall()
    {
        bool isSoftDrop = Input.GetKey(KeyCode.DownArrow) || Time.time < softDropUntilTime;
        float fallTime = isSoftDrop ? 0.05f : gm.GetFallTime();

        if (Time.time - previousTime > fallTime)
        {
            transform.position += Vector3.down;

            if (Board.IsValidPosition(transform))
            {
                if (isSoftDrop)
                    gm.AddDropScore(1, false);
            }
            else
            {
                transform.position += Vector3.up;
                LockPiece();
            }

            previousTime = Time.time;
        }
    }

    void HardDrop()
    {
        int droppedCells = 0;

        while (true)
        {
            transform.position += Vector3.down;

            if (!Board.IsValidPosition(transform))
            {
                transform.position += Vector3.up;
                break;
            }

            droppedCells++;
        }

        gm.AddDropScore(droppedCells, true);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.dropClip, 0.8f);
        LockPiece();
        previousTime = Time.time;
    }

    void Hold()
    {
        if (gm == null) return;

        if (ghostRoot != null)
            Destroy(ghostRoot.gameObject);

        if (gm.TryHoldPiece(this))
            enabled = false;
    }

    void Move(Vector3 dir)
    {
        transform.position += dir;

        if (!Board.IsValidPosition(transform))
            transform.position -= dir;
        else
            lastActionWasRotate = false;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.moveClip, 0.6f);
    }

    void Rotate()
    {
        lastActionWasRotate = false;
        transform.Rotate(0, 0, 90);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.rotateClip, 0.5f);

        if (!Board.IsValidPosition(transform))
        {
            if (TryMove(Vector3.right)) { lastActionWasRotate = true; return; }
            if (TryMove(Vector3.left)) { lastActionWasRotate = true; return; }
            if (TryMove(Vector3.up)) { lastActionWasRotate = true; return; }

            transform.Rotate(0, 0, -90);
            return;
        }

        lastActionWasRotate = true;
    }

    bool TryMove(Vector3 dir)
    {
        transform.position += dir;

        if (Board.IsValidPosition(transform))
            return true;

        transform.position -= dir;
        return false;
    }

    void LockPiece()
    {
        if (ghostRoot != null)
            Destroy(ghostRoot.gameObject);

        bool isTopOut = AddToGrid();
        if (isTopOut)
        {
            gm.TriggerGameOver();
            enabled = false;
            return;
        }

        int lines = Board.ClearLines();
        bool isTSpin = IsTSpin(lines);
        gm.RegisterLineClear(lines, isTSpin);

        if (AudioManager.Instance != null)
        {
            if (isTSpin)
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.tSpinClip);
            else if (lines >= 4)
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.tetrisClip);
            else if (lines > 0)
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.lineClearClip);

            AudioManager.Instance.PlayOneShot(AudioManager.Instance.lockClip, 0.6f);
        }

        gm.SpawnNew();
        enabled = false;
    }

    bool IsTSpin(int clearedLines)
    {
        if (clearedLines <= 0 || !lastActionWasRotate || !IsTPiece())
            return false;

        Vector2 pivot = Board.Round(transform.position);
        int blocked = 0;

        blocked += IsCornerBlocked(pivot + new Vector2(-1, 1)) ? 1 : 0;
        blocked += IsCornerBlocked(pivot + new Vector2(1, 1)) ? 1 : 0;
        blocked += IsCornerBlocked(pivot + new Vector2(-1, -1)) ? 1 : 0;
        blocked += IsCornerBlocked(pivot + new Vector2(1, -1)) ? 1 : 0;

        return blocked >= 3;
    }

    bool IsCornerBlocked(Vector2 cell)
    {
        int x = (int)cell.x;
        int y = (int)cell.y;

        if (x < 0 || x >= Board.width || y < 0)
            return true;

        if (y >= Board.height)
            return false;

        return Board.grid[x, y] != null;
    }

    bool IsTPiece()
    {
        if (gm == null)
            return false;

        return pieceIndex == gm.tPieceIndex;
    }

    void CreateGhost()
    {
        ghostRoot = new GameObject(name + "_Ghost").transform;

        foreach (Transform block in transform)
        {
            GameObject ghostBlock = Instantiate(block.gameObject, ghostRoot);

            Collider2D collider = ghostBlock.GetComponent<Collider2D>();
            if (collider != null)
                collider.enabled = false;

            SpriteRenderer spriteRenderer = ghostBlock.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 0.25f;
                spriteRenderer.color = c;

                spriteRenderer.sortingOrder -= 1;
            }
        }
    }

    void UpdateGhost()
    {
        if (ghostRoot == null) return;

        ghostRoot.position = transform.position;
        ghostRoot.rotation = transform.rotation;

        while (true)
        {
            ghostRoot.position += Vector3.down;

            if (!Board.IsValidPosition(ghostRoot))
            {
                ghostRoot.position += Vector3.up;
                break;
            }
        }
    }

    bool AddToGrid()
    {
        bool topOut = false;

        foreach (Transform block in transform)
        {
            Vector2 pos = Board.Round(block.position);
            int x = (int)pos.x;
            int y = (int)pos.y;

            if (x < 0 || x >= Board.width || y < 0)
            {
                topOut = true;
                continue;
            }

            if (y >= Board.height)
            {
                topOut = true;
                continue;
            }

            Board.grid[x, y] = block;
        }

        return topOut;
    }
}