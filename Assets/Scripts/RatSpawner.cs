using UnityEngine;
using System.Collections.Generic;

public class RatSpawner : MonoBehaviour
{
    [Header("Rat Prefabs")]
    public GameObject ratPrefab;
    public GameObject fastRatPrefab;
    public Transform[] spawnPoints;

    [Header("Spawning")]
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float speedIncreasePerBatch = 0.25f;

    [Header("Victory")]
    [SerializeField] private GameObject winPanel;

    private const int MaxRats = 90;
    private const int RatsPerBatch = 6;

    private int ratsSpawned = 0;
    private float normalRatBaseSpeed;
    private bool winDeclared = false;

    private readonly List<GameObject> spawnedRats =
        new List<GameObject>();

    void Start()
    {
        if (ratPrefab == null || fastRatPrefab == null)
        {
            Debug.LogError("Assign both rat prefabs in the Inspector.");
            enabled = false;
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Assign at least one spawn point.");
            enabled = false;
            return;
        }

        RatMovement normalMovement =
            ratPrefab.GetComponent<RatMovement>();

        if (normalMovement == null)
        {
            Debug.LogError("The normal Rat prefab needs RatMovement.");
            enabled = false;
            return;
        }

        normalRatBaseSpeed = normalMovement.GetMoveSpeed();

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        InvokeRepeating(nameof(SpawnRat), 0f, spawnInterval);
    }

    void SpawnRat()
    {
        if (ratsSpawned >= MaxRats)
        {
            CancelInvoke(nameof(SpawnRat));
            return;
        }

        // Randomly choose one of the available spawn points.
        Transform spawnPoint =
            spawnPoints[Random.Range(0, spawnPoints.Length)];

        if (spawnPoint == null)
        {
            Debug.LogError("A spawn point is unassigned.");
            return;
        }

        // Spawn: normal, normal, fast, then repeat.
        bool isFastRat = ratsSpawned % 3 == 2;

        GameObject prefab = isFastRat
            ? fastRatPrefab
            : ratPrefab;

        // Each group of six rats has its own speed tier.
        int batchIndex = ratsSpawned / RatsPerBatch;

        Quaternion rotation =
            spawnPoint.rotation * prefab.transform.localRotation;

        GameObject newRat = Instantiate(
            prefab,
            spawnPoint.position,
            rotation
        );

        spawnedRats.Add(newRat);

        RatMovement movement =
            newRat.GetComponent<RatMovement>();

        if (movement != null)
        {
            float speedBonus =
                normalRatBaseSpeed *
                speedIncreasePerBatch *
                batchIndex;

            movement.SetMoveSpeed(
                movement.GetMoveSpeed() + speedBonus
            );
        }
        else
        {
            Debug.LogWarning(
                "A spawned rat is missing RatMovement."
            );
        }

        ratsSpawned++;

        // Stop after spawning the 90th rat.
        if (ratsSpawned >= MaxRats)
        {
            CancelInvoke(nameof(SpawnRat));
        }
    }

    void Update()
    {
        // Victory is possible only after all 90 rats have spawned.
        if (ratsSpawned < MaxRats || winDeclared)
        {
            return;
        }

        // Wait until every spawned rat has been destroyed.
        foreach (GameObject rat in spawnedRats)
        {
            if (rat != null)
            {
                return;
            }
        }

        winDeclared = true;

        Debug.Log("VICTORY! All 90 rats have been defeated.");

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }
}
