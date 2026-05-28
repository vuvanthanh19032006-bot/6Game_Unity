using UnityEngine;

public class BoardGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public SpriteRenderer cellPrefab;
    public Vector2Int boardMin = new Vector2Int(-13, -6);
    public Vector2Int boardMax = new Vector2Int(13, 6);
    public float gridSize = 1f;
    public float cellScale = 0.95f;

    [Header("Style")]
    public Color evenColor = new Color(0.06f, 0.12f, 0.14f, 1f);
    public Color oddColor = new Color(0.08f, 0.18f, 0.20f, 1f);
    public int sortingOrder = -5;

    private void Start()
    {
        BuildBoard();
    }

    [ContextMenu("Build Board")]
    public void BuildBoard()
    {
        ClearBoard();

        if (cellPrefab == null)
        {
            Debug.LogWarning("BoardGrid: Chưa gán Cell Prefab.");
            return;
        }

        for (int x = boardMin.x; x <= boardMax.x; x++)
        {
            for (int y = boardMin.y; y <= boardMax.y; y++)
            {
                Vector3 pos = new Vector3(x * gridSize, y * gridSize, 0f);

                SpriteRenderer cell = Instantiate(cellPrefab, pos, Quaternion.identity, transform);

                cell.name = "Cell_" + x + "_" + y;
                cell.transform.localScale = Vector3.one * cellScale * gridSize;
                cell.sortingOrder = sortingOrder;
                cell.color = (x + y) % 2 == 0 ? evenColor : oddColor;
            }
        }
    }

    private void ClearBoard()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}