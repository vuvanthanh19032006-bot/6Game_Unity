using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    public GameObject pipePrefab;
    public float spawnRate = 2f;
    private float timer = 0;

    public float minY = -1f;
    public float maxY = 2.5f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnRate)
        {
            SpawnPipe();
            timer = 0;
        }
    }

    void SpawnPipe()
    {
        float y = Random.Range(minY, maxY);
        Instantiate(pipePrefab, new Vector3(10f, y, 0), Quaternion.identity);
    }
}