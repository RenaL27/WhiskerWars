using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    [SerializeField] private GameObject visualPrefab;
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10;
    public float fireRate = 1f;
    private void Awake()
    {
        if (visualPrefab == null) return;

        MeshRenderer placeholder = GetComponent<MeshRenderer>();
        Vector3 center = placeholder != null ? placeholder.bounds.center : transform.position;
        GameObject visual = Instantiate(visualPrefab, transform);
        visual.name = "Red Yarn Gun Visual";
        // Turn the imported model toward the incoming rats; shooting already points left.
        visual.transform.SetLocalPositionAndRotation(Vector3.zero,
            Quaternion.Euler(0f, 0f, 90f) * Quaternion.Euler(-90f, 0f, 0f));
        // Roll around the barrel so the gun stays aimed left but sits upright.
        visual.transform.rotation = Quaternion.AngleAxis(180f, Vector3.left) * visual.transform.rotation;
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Destroy(visual);
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        float largestDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (largestDimension > 0.001f) visual.transform.localScale *= 0.8f / largestDimension;

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        visual.transform.position += center - bounds.center;
        if (placeholder != null) placeholder.enabled = false;
    }

    void Start()
    {
        InvokeRepeating(nameof(Shoot), 0f, fireRate);
    }
    void Shoot()
    {
        if (bulletPrefab == null || bulletSpawnPoint == null) return;
        Vector3 spawnPosition = bulletSpawnPoint.position + Vector3.left * 0.35f + Vector3.up * 0.15f;
        var bullet = Instantiate(
            bulletPrefab,
            spawnPosition,
            Quaternion.LookRotation(Vector3.left, Vector3.up)
        );

        Rigidbody body = bullet.GetComponent<Rigidbody>();
        if (body != null) body.linearVelocity = Vector3.left * bulletSpeed;
    }
}
