using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KhungLong : MonoBehaviour
{
    [Header("Physics")]
    public Rigidbody2D rb;
    public float jumpForce = 8f;

    [Header("UI")]
    public Image gameOverUI;

    [Header("Duck System")]
    public Sprite normalSprite;
    public Sprite duckSprite;

    [Header("Mobile Control")]
    public float swipeDownDistance = 80f;
    public float tapMaxDistance = 35f;
    public float mobileDuckTime = 0.35f;

    [Header("Scale Fix")]
    public Vector3 duckScale = new Vector3(1.3f, 1.3f, 1f);

    private SpriteRenderer sr;
    private BoxCollider2D box;
    private Animator animator;

    private Vector2 normalColliderSize;
    private Vector2 normalColliderOffset;

    private Vector2 duckColliderSize;
    private Vector2 duckColliderOffset;

    private Vector3 normalScale;

    private bool isGrounded = true;
    private bool isGameOver = false;
    private bool isDucking = false;

    private Vector2 touchStartPos;
    private float duckTimer = 0f;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        sr = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();

        normalScale = transform.localScale;

        if (normalSprite == null && sr != null)
            normalSprite = sr.sprite;

        normalColliderSize = box.size;
        normalColliderOffset = box.offset;

        duckColliderSize = new Vector2(normalColliderSize.x, normalColliderSize.y * 0.55f);
        duckColliderOffset = new Vector2(
            normalColliderOffset.x,
            normalColliderOffset.y - (normalColliderSize.y - duckColliderSize.y) / 2f
        );

        // Ẩn Game Over khi bắt đầu game
        if (gameOverUI != null)
            gameOverUI.gameObject.SetActive(false);
    }

    void Update()
    {
        // Nếu đã chết: chạm màn hình hoặc click chuột hoặc Space để chơi lại
        if (isGameOver)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                RestartGame();
            }

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                RestartGame();
            }

            return;
        }

        HandleKeyboardInput();
        HandleMobileInput();

        if (duckTimer > 0)
        {
            duckTimer -= Time.deltaTime;
            SetDuck(true);
        }
        else
        {
            bool keyboardDuck = (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) && isGrounded;

            if (!keyboardDuck)
            {
                SetDuck(false);
            }
        }
    }

    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        if ((Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) && isGrounded)
        {
            SetDuck(true);
        }
    }

    void HandleMobileInput()
    {
        // Click chuột để test nhảy trên máy tính
        if (Input.touchCount == 0 && Input.GetMouseButtonDown(0))
        {
            Jump();
        }

        if (Input.touchCount <= 0) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            touchStartPos = touch.position;
        }

        if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
        {
            Vector2 delta = touch.position - touchStartPos;

            bool swipeDown = delta.y < -swipeDownDistance && Mathf.Abs(delta.y) > Mathf.Abs(delta.x);

            if (swipeDown && isGrounded)
            {
                SetDuck(true);
            }
        }

        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            Vector2 delta = touch.position - touchStartPos;

            bool swipeDown = delta.y < -swipeDownDistance && Mathf.Abs(delta.y) > Mathf.Abs(delta.x);
            bool tap = delta.magnitude <= tapMaxDistance;

            if (swipeDown && isGrounded)
            {
                duckTimer = mobileDuckTime;
            }
            else if (tap)
            {
                Jump();
            }
        }
    }

    void Jump()
    {
        if (!isGrounded) return;

        SetDuck(false);

        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isGrounded = false;
    }

    void SetDuck(bool duck)
    {
        if (!isGrounded)
            duck = false;

        if (isDucking == duck) return;

        isDucking = duck;

        if (duck)
        {
            if (animator != null)
                animator.enabled = false;

            if (duckSprite != null)
                sr.sprite = duckSprite;

            transform.localScale = duckScale;

            box.size = duckColliderSize;
            box.offset = duckColliderOffset;
        }
        else
        {
            transform.localScale = normalScale;

            box.size = normalColliderSize;
            box.offset = normalColliderOffset;

            if (normalSprite != null)
                sr.sprite = normalSprite;

            if (animator != null)
                animator.enabled = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

        if (collision.gameObject.CompareTag("CayXuongRong"))
        {
            GameOver();
        }
    }

    void GameOver()
    {
        isGameOver = true;

        // Hiện ảnh Game Over
        if (gameOverUI != null)
            gameOverUI.gameObject.SetActive(true);

        // Dừng toàn bộ game
        Time.timeScale = 0f;
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}