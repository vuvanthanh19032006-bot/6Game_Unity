using UnityEngine;

public class MovingBubble : MonoBehaviour
{
    private BubbleGrid bubbleGrid;
    private ShooterController shooterController;
    private Rigidbody2D rb;
    private float moveSpeed;
    private bool attached;

    public void Init(BubbleGrid grid, ShooterController shooter, float speed)
    {
        bubbleGrid = grid;
        shooterController = shooter;
        moveSpeed = speed;
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private void Update()
    {
        if (attached)
        {
            return;
        }

        // Chống trường hợp bóng vì tốc độ cao mà vượt mép màn hình
        if (transform.position.x <= -2.4f)
        {
            transform.position = new Vector3(-2.4f, transform.position.y, transform.position.z);
            BounceRight();
        }

        if (transform.position.x >= 2.4f)
        {
            transform.position = new Vector3(2.4f, transform.position.y, transform.position.z);
            BounceLeft();
        }

        // Chạm gần đỉnh thì dính vào lưới
        if (transform.position.y >= 4.55f)
        {
            AttachToGrid(null);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (attached)
        {
            return;
        }

        string objectName = collision.collider.gameObject.name;

        if (objectName.Contains("LeftWall"))
        {
            BounceRight();
            return;
        }

        if (objectName.Contains("RightWall"))
        {
            BounceLeft();
            return;
        }

        if (objectName.Contains("TopWall"))
        {
            AttachToGrid(null);
            return;
        }

        Bubble hitBubble = collision.collider.GetComponent<Bubble>();

        if (hitBubble != null && hitBubble.isConnectedToGrid)
        {
            AttachToGrid(hitBubble);
        }
    }

    private void BounceRight()
    {
        if (rb == null) return;

        Vector2 velocity = rb.velocity;
        velocity.x = Mathf.Abs(velocity.x);

        rb.velocity = velocity.normalized * moveSpeed;
    }

    private void BounceLeft()
    {
        if (rb == null) return;

        Vector2 velocity = rb.velocity;
        velocity.x = -Mathf.Abs(velocity.x);

        rb.velocity = velocity.normalized * moveSpeed;
    }

    private void AttachToGrid(Bubble hitBubble)
    {
        if (attached)
        {
            return;
        }

        attached = true;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
        }

        if (bubbleGrid != null)
        {
            bubbleGrid.AttachBubble(gameObject, hitBubble);
        }

        if (shooterController != null)
        {
            shooterController.OnBubbleAttached();
        }

        Destroy(this);
    }
}