using UnityEngine;

public class Lawnmower : MonoBehaviour
{
    public float speed = 8f;
    private bool sweeping;
    private float leftEdge = -12f;

    private void Start()
    {
        CatPlacement grid = FindFirstObjectByType<CatPlacement>();
        if (grid == null) return;
        leftEdge = float.PositiveInfinity;
        foreach (Collider tile in grid.GetComponentsInChildren<Collider>())
            leftEdge = Mathf.Min(leftEdge, tile.bounds.min.x - 2f);
    }

    // A rat reaching the defended edge uses the one remaining mower in its row.
    public static bool TryActivate(RatMovement rat)
    {
        foreach (Lawnmower mower in FindObjectsByType<Lawnmower>(FindObjectsSortMode.None))
        {
            if (mower.sweeping || Mathf.Abs(mower.transform.position.z - rat.transform.position.z) > 0.6f)
                continue;
            mower.sweeping = true;
            Destroy(rat.gameObject);
            return true;
        }
        return false;
    }

    private void Update()
    {
        if (!sweeping || !GameFlow.IsPlaying || Time.timeScale == 0f) return;
        float previousX = transform.position.x;
        transform.position += Vector3.left * speed * Time.deltaTime;

        // Only rats in this row can be removed. No physics or rotation needed.
        foreach (RatMovement rat in FindObjectsByType<RatMovement>(FindObjectsSortMode.None))
        {
            Vector3 position = rat.transform.position;
            if (Mathf.Abs(position.z - transform.position.z) <= 0.6f &&
                position.x >= transform.position.x - 0.65f && position.x <= previousX + 0.65f)
                Destroy(rat.gameObject);
        }
        if (transform.position.x < leftEdge) Destroy(gameObject);
    }
}
