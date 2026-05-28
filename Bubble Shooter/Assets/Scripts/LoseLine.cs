using UnityEngine;

public class LoseLine : MonoBehaviour
{
    private StartMenuManager startMenuManager;

    private void Start()
    {
        startMenuManager = FindObjectOfType<StartMenuManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Bubble bubble = collision.GetComponent<Bubble>();

        if (bubble == null) return;

        if (bubble.isConnectedToGrid)
        {
            if (startMenuManager == null)
            {
                startMenuManager = FindObjectOfType<StartMenuManager>();
            }

            if (startMenuManager != null)
            {
                startMenuManager.ShowGameOver();
            }
        }
    }
}