using UnityEngine;
using System.Collections.Generic;

public class RatSpawner : MonoBehaviour
{
    [Header("Rat Prefabs")]
    public GameObject ratPrefab;
    public GameObject fastRatPrefab;
    public Transform[] spawnPoints;

    [Header("Spawning")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float speedIncreasePerBatch = 0.35f;

    [Header("Victory")]
    [SerializeField] private GameObject winPanel;

    private const int MaxRats = 90;
    private const int RatsPerBatch = 6;

    public bool Finished => ratsSpawned >= MaxRats;
    private float timeToNextSpawn;
    private int ratsSpawned = 0;
    private float normalRatBaseSpeed;
    private bool winDeclared = false;

    private readonly List<Transform> remainingSpawnPoints = new List<Transform>();

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


    }

    void SpawnRat()
    {
        if (ratsSpawned >= MaxRats)
        {
            CancelInvoke(nameof(SpawnRat));
            return;
        }

        // Visit every row once per shuffled cycle so no lane is skipped.
        if (remainingSpawnPoints.Count == 0)
            remainingSpawnPoints.AddRange(spawnPoints);
        int laneIndex = Random.Range(0, remainingSpawnPoints.Count);
        Transform spawnPoint = remainingSpawnPoints[laneIndex];
        remainingSpawnPoints.RemoveAt(laneIndex);

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
        if (!GameFlow.IsPlaying || Time.timeScale == 0f) return;
        if (!Finished)
        {
            timeToNextSpawn -= Time.deltaTime;
            if (timeToNextSpawn <= 0f)
            {
                SpawnRat();
                // Later batches arrive closer together, down to one rat every 0.8 seconds.
                int batchIndex = (ratsSpawned - 1) / RatsPerBatch;
                timeToNextSpawn = Mathf.Max(0.8f, spawnInterval - batchIndex * 0.1f);
            }
        }
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

        GameFlow.Instance?.EndRound(true);

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }
}
