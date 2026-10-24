using UnityEngine;

public class Lawnmower : MonoBehaviour
{
    public float speed = 8f;
    [SerializeField, Min(0.1f)] private float visualSize = 0.9f;
    private bool sweeping;
    private float leftEdge = -12f;

    private void Start()
    {
        ReplaceSphereWithCat();

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

    private void ReplaceSphereWithCat()
    {
        GameObject catModel = Resources.Load<GameObject>("Models/LawnmowerCat");
        if (catModel == null)
        {
            Debug.LogError("Could not load the lawnmower cat model.", this);
            return;
        }

        // Keep the original sphere as the behavior's root, but hide its visual.
        MeshRenderer sphereRenderer = GetComponent<MeshRenderer>();
        if (sphereRenderer != null)
            sphereRenderer.enabled = false;

        GameObject visual = Instantiate(catModel, transform);
        visual.name = "Lawnmower Cat Visual";
        visual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        float largestDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (largestDimension > 0.001f)
            visual.transform.localScale *= visualSize / largestDimension;

        // Recalculate after scaling, center it on the old sphere, and rest it on the board.
        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        visual.transform.position += new Vector3(
            transform.position.x - bounds.center.x,
            transform.position.y - 0.4f - bounds.min.y,
            transform.position.z - bounds.center.z);
    }
}
