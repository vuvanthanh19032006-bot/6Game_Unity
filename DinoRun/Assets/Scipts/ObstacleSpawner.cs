using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] cactusPrefabs; // 4 cây
    public GameObject birdPrefab;      // 1 chim

    public float spawnRate = 2f;
    public float spawnX = 10f;
    public float minSpawnRate = 0.8f;
    public float spawnSpeedUp = 0.05f;

    public Transform ground;

    void Start()
    {
        InvokeRepeating(nameof(Spawn), 1f, spawnRate);
    }

    void Spawn()
    {
        int random = Random.Range(0, 5);

        GameObject obj;

        if (random < 4)
            obj = Instantiate(cactusPrefabs[random]);
        else
            obj = Instantiate(birdPrefab);

        float groundTop = ground.GetComponent<Collider2D>().bounds.max.y;

        if (random < 4)
        {
            // 🌵 Cactus đứng trên đất
            float objHalf = obj.GetComponent<SpriteRenderer>().bounds.extents.y;
            obj.transform.position = new Vector2(spawnX, groundTop + objHalf);
        }
        else
        {
            // 🐦 KhungLongBay bay cao hơn mặt đất
            float birdHeight = 1.8f; // chỉnh cao/thấp tại đây

            obj.transform.position = new Vector2(spawnX, groundTop + birdHeight);
        }
        // 🚀 SPAWN NHANH DẦN
        spawnRate -= spawnSpeedUp * Time.deltaTime;
        spawnRate = Mathf.Clamp(spawnRate, minSpawnRate, 2f);

        CancelInvoke();
        InvokeRepeating(nameof(Spawn), spawnRate, spawnRate);
    }
}