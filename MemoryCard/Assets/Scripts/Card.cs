using System.Collections;
using UnityEngine;

public class Card : MonoBehaviour
{
    public int id;
    public Sprite frontSprite;
    public Sprite backSprite;

    private SpriteRenderer sr;
    private bool isFlipped = false;
    private GameManager gameManager;

    public void Init(GameManager gm, Sprite front, Sprite back, int cardId)
    {
        gameManager = gm;
        frontSprite = front;
        backSprite = back;
        id = cardId;

        sr = GetComponent<SpriteRenderer>();
        sr.sprite = backSprite;

        // 🔥 auto fit collider theo sprite
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = sr.bounds.size;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                if (!isFlipped && gameManager.CanClick())
                {
                    Flip();
                    gameManager.OnCardClicked(this);
                }
            }
        }
    }

    public void Flip()
    {
        isFlipped = !isFlipped;
        sr.sprite = isFlipped ? frontSprite : backSprite;
    }

    public bool IsFlipped()
    {
        return isFlipped;
    }

    // 🔥 Ẩn card khi match (có animation)
    public void Hide()
    {
        StartCoroutine(HideAnim());
    }

    IEnumerator HideAnim()
    {
        float t = 0;
        Vector3 start = transform.localScale;

        while (t < 1)
        {
            t += Time.deltaTime * 5f;
            transform.localScale = Vector3.Lerp(start, Vector3.zero, t);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}