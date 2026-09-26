using UnityEngine;

public class RatSpawner : MonoBehaviour
{
    public GameObject ratPrefab;
    public Transform spawnPoint;

    void Start()
    {
        InvokeRepeating(nameof(SpawnRat), 0f, 2f);
    }

    void SpawnRat()
    {
        Instantiate(ratPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}