using UnityEngine;

public class ObstacleMove : MonoBehaviour
{
    public float speed = 5f;
    public float maxSpeed = 12f;
    public float speedIncrease = 0.2f;
    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        if (transform.position.x < -15f)
            Destroy(gameObject);
        // 🚀 TĂNG TỐC THEO THỜI GIAN
        if (speed < maxSpeed)
        {
            speed += speedIncrease * Time.deltaTime;
        }
    }

}