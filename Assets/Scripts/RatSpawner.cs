using UnityEngine;

public class RatSpawner : MonoBehaviour
{
    public GameObject ratPrefab;
    public Transform spawnPoint;
    public GameObject fastRatPrefab;

    private bool spawnFastRat;

    void Start()
    {
        InvokeRepeating(nameof(SpawnRat), 0f, 3f);
    }

    void SpawnRat()
    {
        GameObject prefab = spawnFastRat && fastRatPrefab != null ? fastRatPrefab : ratPrefab;
        // Preserve the prefab's facing direction relative to the spawn point.
        Quaternion rotation = spawnPoint.rotation * prefab.transform.localRotation;
        Instantiate(prefab, spawnPoint.position, rotation);
        spawnFastRat = !spawnFastRat;
    }
}