using UnityEngine;

public class FinishLine : MonoBehaviour
{
    [SerializeField] private GameObject losePanel;

    private bool gameOver = false;

    void Start()
    {
        Time.timeScale = 1f;

        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(
        $"Finish line touched by {other.name}; " +
        $"tag: {other.tag}; " +
        $"root: {other.transform.root.name}; " +
        $"root tag: {other.transform.root.tag}"
    );
    
        if (gameOver || !other.CompareTag("Rat"))
        {
            return;
        }

        gameOver = true;

        Debug.Log("DEFEAT! A rat reached the finish line.");

        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }
}
