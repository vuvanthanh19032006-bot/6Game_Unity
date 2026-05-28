using UnityEngine;

public class ShooterController : MonoBehaviour
{
    [Header("References")]
    public GameObject bubblePrefab;
    public Sprite[] bubbleSprites;
    public Transform firePoint;
    public SpriteRenderer nextBubblePreview;
    public LineRenderer aimLine;
    public BubbleGrid bubbleGrid;

    [Header("Shoot Settings")]
    public float shootSpeed = 9f;
    public float minAimY = 0.2f;

    [Header("Trajectory Settings")]
    public float trajectoryLength = 9f;
    public int maxTrajectoryBounces = 3;
    public LayerMask trajectoryMask = ~0;

    private Camera mainCamera;
    private GameObject currentBubble;
    private int currentBubbleIndex;
    private int nextBubbleIndex;
    private Vector2 currentAimDirection = Vector2.up;
    private bool canShoot = true;

    private void Start()
    {
        Physics2D.queriesHitTriggers = false;

        mainCamera = Camera.main;

        if (bubbleGrid == null)
        {
            bubbleGrid = FindObjectOfType<BubbleGrid>();
        }

        SetupAimLine();

        currentBubbleIndex = Random.Range(0, bubbleSprites.Length);
        nextBubbleIndex = Random.Range(0, bubbleSprites.Length);

        SpawnCurrentBubble();
        UpdateNextBubblePreview();
    }

    private void Update()
    {
        if (!canShoot || currentBubble == null)
        {
            return;
        }

        HandleInput();
    }

    private void HandleInput()
    {
        bool isHolding = false;
        bool isReleased = false;
        Vector2 screenPosition = Vector2.zero;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            screenPosition = touch.position;

            if (touch.phase == TouchPhase.Began ||
                touch.phase == TouchPhase.Moved ||
                touch.phase == TouchPhase.Stationary)
            {
                isHolding = true;
            }

            if (touch.phase == TouchPhase.Ended)
            {
                isReleased = true;
            }
        }
        else
        {
            screenPosition = Input.mousePosition;

            if (Input.GetMouseButton(0))
            {
                isHolding = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                isReleased = true;
            }
        }

        if (isHolding)
        {
            AimAt(screenPosition);
        }

        if (isReleased)
        {
            Shoot();
        }
    }

    private void AimAt(Vector2 screenPosition)
    {
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0;

        Vector2 direction = ((Vector2)worldPosition - (Vector2)firePoint.position).normalized;

        // Không cho bắn xuống dưới
        if (direction.y < minAimY)
        {
            HideAimLine();
            return;
        }

        currentAimDirection = direction;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        ShowTrajectory(direction);
    }

    private void Shoot()
    {
        if (currentBubble == null)
        {
            return;
        }

        if (currentAimDirection.y < minAimY)
        {
            return;
        }

        HideAimLine();

        Rigidbody2D rb = currentBubble.GetComponent<Rigidbody2D>();
        CircleCollider2D col = currentBubble.GetComponent<CircleCollider2D>();

        currentBubble.transform.parent = null;
        currentBubble.transform.rotation = Quaternion.identity;
        currentBubble.transform.localScale = bubblePrefab.transform.localScale;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0;
        rb.velocity = currentAimDirection.normalized * shootSpeed;
        rb.angularVelocity = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        col.enabled = true;
        col.isTrigger = false;

        MovingBubble movingBubble = currentBubble.GetComponent<MovingBubble>();

        if (movingBubble == null)
        {
            movingBubble = currentBubble.AddComponent<MovingBubble>();
        }

        movingBubble.Init(bubbleGrid, this, shootSpeed);

        currentBubble = null;
        canShoot = false;
    }

    public void OnBubbleAttached()
    {
        PrepareNextBubble();
        canShoot = true;
    }

    private void PrepareNextBubble()
    {
        currentBubbleIndex = nextBubbleIndex;
        nextBubbleIndex = Random.Range(0, bubbleSprites.Length);

        SpawnCurrentBubble();
        UpdateNextBubblePreview();
    }

    private void SpawnCurrentBubble()
    {
        currentBubble = Instantiate(
            bubblePrefab,
            firePoint.position,
            Quaternion.identity,
            firePoint
        );

        currentBubble.transform.localPosition = Vector3.zero;
        currentBubble.transform.localRotation = Quaternion.identity;
        currentBubble.transform.localScale = bubblePrefab.transform.localScale;

        Bubble bubble = currentBubble.GetComponent<Bubble>();
        bubble.Init((BubbleColor)currentBubbleIndex, bubbleSprites[currentBubbleIndex]);
        bubble.isConnectedToGrid = false;

        Rigidbody2D rb = currentBubble.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CircleCollider2D col = currentBubble.GetComponent<CircleCollider2D>();
        col.enabled = true;
        col.isTrigger = false;
    }

    private void UpdateNextBubblePreview()
    {
        if (nextBubblePreview != null)
        {
            nextBubblePreview.sprite = bubbleSprites[nextBubbleIndex];
            nextBubblePreview.transform.localScale = bubblePrefab.transform.localScale;
        }
    }

    private void SetupAimLine()
    {
        if (aimLine == null)
        {
            return;
        }

        aimLine.enabled = false;
        aimLine.positionCount = 2;
        aimLine.startWidth = 0.04f;
        aimLine.endWidth = 0.04f;
        aimLine.useWorldSpace = true;

        if (aimLine.material == null)
        {
            aimLine.material = new Material(Shader.Find("Sprites/Default"));
        }

        aimLine.sortingOrder = 50;
    }

    private void ShowTrajectory(Vector2 direction)
    {
        if (aimLine == null || currentBubble == null)
        {
            return;
        }

        CircleCollider2D currentCollider = currentBubble.GetComponent<CircleCollider2D>();

        float radius = 0.25f;

        if (currentCollider != null)
        {
            radius = currentCollider.radius * currentBubble.transform.lossyScale.x;
        }

        bool oldColliderState = true;

        if (currentCollider != null)
        {
            oldColliderState = currentCollider.enabled;
            currentCollider.enabled = false;
        }

        Vector2 startPoint = firePoint.position;
        Vector2 castPosition = startPoint + direction.normalized * (radius + 0.03f);
        Vector2 castDirection = direction.normalized;

        float remainingLength = trajectoryLength;

        aimLine.enabled = true;
        aimLine.positionCount = 1;
        aimLine.SetPosition(0, startPoint);

        int linePointIndex = 1;

        for (int bounce = 0; bounce <= maxTrajectoryBounces; bounce++)
        {
            RaycastHit2D hit = Physics2D.CircleCast(
                castPosition,
                radius,
                castDirection,
                remainingLength,
                trajectoryMask
            );

            if (hit.collider == null)
            {
                Vector2 endPoint = castPosition + castDirection * remainingLength;

                aimLine.positionCount = linePointIndex + 1;
                aimLine.SetPosition(linePointIndex, endPoint);
                break;
            }

            Vector2 hitPoint = hit.centroid;

            aimLine.positionCount = linePointIndex + 1;
            aimLine.SetPosition(linePointIndex, hitPoint);
            linePointIndex++;

            string hitObjectName = hit.collider.gameObject.name;

            bool hitLeftWall = hitObjectName.Contains("LeftWall");
            bool hitRightWall = hitObjectName.Contains("RightWall");
            bool hitTopWall = hitObjectName.Contains("TopWall");

            Bubble hitBubble = hit.collider.GetComponent<Bubble>();

            // Gặp bóng hoặc trần thì dừng đường ngắm
            if (hitBubble != null || hitTopWall)
            {
                break;
            }

            // Gặp tường trái/phải thì phản xạ tiếp
            if (hitLeftWall || hitRightWall)
            {
                castDirection = Vector2.Reflect(castDirection, hit.normal).normalized;
                castPosition = hitPoint + castDirection * 0.04f;
                remainingLength -= hit.distance;

                if (remainingLength <= 0)
                {
                    break;
                }

                continue;
            }

            break;
        }

        if (currentCollider != null)
        {
            currentCollider.enabled = oldColliderState;
        }
    }

    private void HideAimLine()
    {
        if (aimLine != null)
        {
            aimLine.enabled = false;
        }
    }
}