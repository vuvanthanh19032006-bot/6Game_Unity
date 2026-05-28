using UnityEngine;

public enum BubbleColor
{
    Yellow,
    Green,
    Red,
    Blue,
    Orange,
    Pink
}

public class Bubble : MonoBehaviour
{
    public BubbleColor bubbleColor;
    public int row;
    public int column;
    public bool isConnectedToGrid;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(BubbleColor color, Sprite sprite)
    {
        bubbleColor = color;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        spriteRenderer.sprite = sprite;
    }
}