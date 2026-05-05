using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KhungLong : MonoBehaviour
{
    public Rigidbody2D rb;
    public float jumpForce = 8f;

    public Image gameOverUI;

    // 🔥 DUCK SYSTEM
    public Sprite normalSprite;
    public Sprite duckSprite;

    private SpriteRenderer sr;
    private BoxCollider2D box;

    private bool isGrounded = true;
    private bool isGameOver = false;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        sr = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();

        if (gameOverUI != null)
            gameOverUI.gameObject.SetActive(false);
    }

    void Update()
    {
        // 🔥 GAME OVER → restart
        if (isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            return;
        }

        // 🔥 NHẢY
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
        }

        // 🔥 CÚI XUỐNG (DUCK)
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            sr.sprite = duckSprite;

            // giảm hitbox khi cúi
            box.size = new Vector2(box.size.x, 0.5f);
        }
        else
        {
            sr.sprite = normalSprite;

            // trả hitbox về bình thường
            box.size = new Vector2(box.size.x, 1f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;

        if (collision.gameObject.CompareTag("CayXuongRong"))
            GameOver();
    }

    void GameOver()
    {
        isGameOver = true;

        if (gameOverUI != null)
            gameOverUI.gameObject.SetActive(true);

        Time.timeScale = 0f;
    }
}