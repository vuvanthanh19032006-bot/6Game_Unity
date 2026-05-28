using System.Collections;
using UnityEngine;

public class Board : MonoBehaviour
{
    public static int width = 10;
    public static int height = 20;

    public static Transform[,] grid = new Transform[width, height];

    [Header("Visuals")]
    public bool drawRuntimeGrid = true;
    public bool drawRuntimeBorder = true;
    public bool drawRuntimeBackground = true;
    public bool drawBackgroundShadow = true;
    public bool useGradientBackground = true;
    public bool drawTopBorder = true;
    public Color gridColor = new Color(1f, 1f, 1f, 0.05f);
    public Color borderColor = new Color(0.7f, 0.8f, 1.0f, 0.35f);
    public Color backgroundColor = new Color(0.08f, 0.10f, 0.16f, 0.85f);
    public Color backgroundTopColor = new Color(0.15f, 0.20f, 0.30f, 0.92f);
    public Color backgroundBottomColor = new Color(0.06f, 0.08f, 0.13f, 0.95f);
    public Color shadowColor = new Color(0f, 0f, 0f, 0.35f);
    public Vector2 shadowOffset = new Vector2(0.3f, -0.3f);
    public float gridLineWidth = 0.05f;
    public float borderLineWidth = 0.1f;
    public float borderThickness = 0.01f;

    public Color lineClearColor = new Color(1f, 1f, 1f, 0.9f);
    public float lineClearDuration = 0.28f;

    public static Board Instance;

    Transform visualsRoot;
    Sprite squareSprite;

    void Start()
    {
        Instance = this;
        BuildVisuals();
    }

    void BuildVisuals()
    {
        if (visualsRoot != null)
            Destroy(visualsRoot.gameObject);

        visualsRoot = new GameObject("BoardVisuals").transform;
        visualsRoot.SetParent(transform, false);

        if (drawRuntimeBackground)
            CreateBackground();

        if (drawRuntimeGrid)
            CreateGridLines();

        if (drawRuntimeBorder)
            CreateBorder();
    }

    public static Vector2 Round(Vector2 pos)
    {
        return new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
    }

    void CreateBackground()
    {
        Vector3 center = new Vector3((width - 1) * 0.5f, (height - 1) * 0.5f, 1f);

        if (drawBackgroundShadow)
        {
            Vector3 shadowPos = center + new Vector3(shadowOffset.x, shadowOffset.y, 0f);
            CreateRect("BoardShadow", shadowPos, new Vector2(width, height), shadowColor, -12);
        }

        if (useGradientBackground)
        {
            for (int y = 0; y < height; y++)
            {
                float t = height <= 1 ? 0f : (float)y / (height - 1);
                Color rowColor = Color.Lerp(backgroundBottomColor, backgroundTopColor, t);
                Vector3 rowPos = new Vector3(center.x, y, 1f);
                CreateRect("BoardRow_" + y, rowPos, new Vector2(width, 1f), rowColor, -10);
            }
        }
        else
        {
            CreateRect("BoardBackground", center, new Vector2(width, height), backgroundColor, -10);
        }
    }

    void CreateGridLines()
    {
        for (int x = 0; x <= width; x++)
        {
            Vector3 pos = new Vector3(x - 0.5f, (height - 1) * 0.5f, 0f);
            CreateRect("GridV_" + x, pos, new Vector2(gridLineWidth, height), gridColor, -5);
        }

        for (int y = 0; y <= height; y++)
        {
            Vector3 pos = new Vector3((width - 1) * 0.5f, y - 0.5f, 0f);
            CreateRect("GridH_" + y, pos, new Vector2(width, gridLineWidth), gridColor, -5);
        }
    }

    void CreateBorder()
    {
        float centerX = (width - 1) * 0.5f;
        float centerY = (height - 1) * 0.5f;
        float t = Mathf.Clamp(borderThickness, 0.02f, 0.25f);

        CreateRect("BorderLeft", new Vector3(-0.5f, centerY, 0f), new Vector2(t, height), borderColor, -4);
        CreateRect("BorderRight", new Vector3(width - 0.5f, centerY, 0f), new Vector2(t, height), borderColor, -4);
        CreateRect("BorderBottom", new Vector3(centerX, -0.5f, 0f), new Vector2(width, t), borderColor, -4);

        if (drawTopBorder)
            CreateRect("BorderTop", new Vector3(centerX, height - 0.5f, 0f), new Vector2(width, t), borderColor, -4);
    }

    void CreateRect(string objectName, Vector3 position, Vector2 scale, Color color, int sortingOrder)
    {
        GameObject rectObj = new GameObject(objectName);
        rectObj.transform.SetParent(visualsRoot, false);
        rectObj.transform.localPosition = position;
        rectObj.transform.localScale = new Vector3(scale.x, scale.y, 1f);

        SpriteRenderer spriteRenderer = rectObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetSquareSprite();
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = sortingOrder;
    }

    Sprite GetSquareSprite()
    {
        if (squareSprite != null)
            return squareSprite;

        Texture2D texture = Texture2D.whiteTexture;
        squareSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
        return squareSprite;
    }

    void CreateLine(string lineName, Vector3 start, Vector3 end, Color color, float widthValue, int sortingOrder)
    {
        GameObject lineObj = new GameObject(lineName);
        lineObj.transform.SetParent(visualsRoot, false);

        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.startWidth = widthValue;
        line.endWidth = widthValue;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = color;
        line.endColor = color;
        line.sortingOrder = sortingOrder;
        line.useWorldSpace = false;
    }

    public static bool InsideGrid(Vector2 pos)
    {
        return (int)pos.x >= 0 && (int)pos.x < width &&
               (int)pos.y >= 0;
    }

    public static bool IsValidPosition(Transform tetromino)
    {
        foreach (Transform block in tetromino)
        {
            Vector2 pos = Round(block.position);

            if (!InsideGrid(pos))
                return false;

            if (pos.y < height && grid[(int)pos.x, (int)pos.y] != null)
                return false;
        }

        return true;
    }
    public static int ClearLines()
    {
        int linesCleared = 0;

        for (int y = 0; y < height; y++)
        {
            if (IsLineFull(y))
            {
                if (Instance != null)
                    Instance.StartCoroutine(Instance.PlayLineClearFlash(y));

                DeleteLine(y);
                MoveAllDown(y);
                y--;
                linesCleared++;
            }
        }

        return linesCleared;
    }

    static bool IsLineFull(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] == null)
                return false;
        }
        return true;
    }

    static void DeleteLine(int y)
    {
        for (int x = 0; x < width; x++)
        {
            Destroy(grid[x, y].gameObject);
            grid[x, y] = null;
        }
    }

    static void MoveAllDown(int y)
    {
        for (int i = y; i < height; i++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, i] != null)
                {
                    grid[x, i - 1] = grid[x, i];
                    grid[x, i] = null;
                    grid[x, i - 1].position += Vector3.down;
                }
            }
        }
    }

    IEnumerator PlayLineClearFlash(int y)
    {
        if (visualsRoot != null)
        {
            Transform flashRow = visualsRoot.Find("ClearFlashRow_" + y);
            if (flashRow == null)
            {
                CreateRect("ClearFlashRow_" + y, new Vector3((width - 1) * 0.5f, y, -9f), new Vector2(width, 1f), lineClearColor, -9);
                flashRow = visualsRoot.Find("ClearFlashRow_" + y);
            }

            if (flashRow != null)
            {
                SpriteRenderer sr = flashRow.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    float elapsed = 0f;
                    Color start = sr.color;
                    while (elapsed < lineClearDuration)
                    {
                        float t = elapsed / lineClearDuration;
                        sr.color = Color.Lerp(start, new Color(start.r, start.g, start.b, 0f), t);
                        elapsed += Time.deltaTime;
                        yield return null;
                    }
                    Destroy(flashRow.gameObject);
                }
            }
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Gizmos.DrawWireCube(new Vector3(x, y, 0), Vector3.one);
            }
        }
    }
}