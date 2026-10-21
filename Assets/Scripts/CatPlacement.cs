using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Attach to the grid root; only its tile colliders accept placement.
public class CatPlacement : MonoBehaviour
{
    [SerializeField] private Camera placementCamera;
    [Tooltip("Use the Cat prefab asset so deleting scene cats does not break placement.")]
    [SerializeField] private GameObject catTemplate;
    [SerializeField] private GameObject pinkCatTemplate;
    [SerializeField] private GameObject gunPrefab;
    [SerializeField, Min(0f)] private float placementCooldown = 5f;

    private readonly Dictionary<Collider, GameObject> occupants = new();
    private Collider[] tiles;
    private float nextPlacementTime;
    private CatSelectionScreen selectionScreen;
    private bool usePinkCat;

    public bool PinkCatAvailable => pinkCatTemplate != null;
    public string SelectedCatName => usePinkCat ? "Pink Cat" : "Regular Cat";

    private void Awake()
    {
        GameFlow.EnsureExists();
        selectionScreen = gameObject.AddComponent<CatSelectionScreen>();
        selectionScreen.Initialize(this);
    }

    public void SelectCat(bool pink)
    {
        usePinkCat = pink && PinkCatAvailable;
    }

    public float CooldownRemaining => Mathf.Max(0f, nextPlacementTime - Time.time);

    private void Start()
    {
        if (placementCamera == null)
            placementCamera = Camera.main;

        tiles = GetComponentsInChildren<Collider>();
        if (placementCamera == null || catTemplate == null || gunPrefab == null || tiles.Length == 0)
        {
            Debug.LogError("Cat placement needs a camera, a cat template, a gun prefab, and tile colliders.", this);
            enabled = false;
            selectionScreen.enabled = false;
            return;
        }

        // Reserve tiles under the cats already present in the scene.
        foreach (Transform existingCat in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            if (!existingCat.name.StartsWith("Cat") || !existingCat.gameObject.activeInHierarchy)
                continue;

            Vector3 position = GetVisualBounds(existingCat.gameObject).center;
            foreach (Collider tile in tiles)
            {
                Bounds bounds = tile.bounds;
                if (position.x >= bounds.min.x && position.x <= bounds.max.x &&
                    position.z >= bounds.min.z && position.z <= bounds.max.z)
                {
                    occupants[tile] = existingCat.gameObject;
                    break;
                }
            }
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f || CooldownRemaining > 0f || Mouse.current == null ||
            !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (selectionScreen.BlocksPlacement(Mouse.current.position.ReadValue()))
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = placementCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Ignore))
            TryPlaceCat(hit.collider);
    }

    public bool TryPlaceCat(Collider tile)
    {
        if (!GameFlow.IsPlaying || !enabled || selectionScreen.IsOpen || selectionScreen.JustClosed || tiles == null || Time.timeScale == 0f || CooldownRemaining > 0f ||
            System.Array.IndexOf(tiles, tile) < 0 ||
            (occupants.TryGetValue(tile, out GameObject occupant) && occupant != null))
            return false;

        // Spawn from the prefab asset independently of any cats in the scene.
        GameObject template = usePinkCat ? pinkCatTemplate : catTemplate;
        GameObject cat = Instantiate(template, template.transform.position,
            template.transform.rotation);
        cat.transform.localScale = template.transform.lossyScale;
        cat.name = "Placed " + SelectedCatName;
        cat.SetActive(true);

        Bounds catBounds = GetVisualBounds(cat);
        Bounds tileBounds = tile.bounds;
        if (usePinkCat)
        {
            float footprint = Mathf.Max(catBounds.size.x / tileBounds.size.x,
                catBounds.size.z / tileBounds.size.z);
            if (footprint > 0.001f)
                cat.transform.localScale *= 0.85f / footprint;
            catBounds = GetVisualBounds(cat);
        }
        cat.transform.position += new Vector3(
            tileBounds.center.x - catBounds.center.x,
            tileBounds.max.y - catBounds.min.y,
            tileBounds.center.z - catBounds.center.z);

        if (usePinkCat)
            cat.AddComponent<PinkCatBlocker>().Initialize(tileBounds);
        else
            AttachGun(cat);

        occupants[tile] = cat;
        nextPlacementTime = Time.time + placementCooldown;
        return true;
    }

    private void AttachGun(GameObject cat)
    {
        Bounds bounds = GetVisualBounds(cat);
        // Rats enter from the left. Tuck the gun against the body while
        // keeping bullets at the existing lane height, above the tiles.
        Vector3 position = new Vector3(bounds.min.x + 0.10f,
            bounds.min.y + 0.45f, bounds.center.z);
        GameObject gun = Instantiate(gunPrefab, position, gunPrefab.transform.rotation);
        gun.name = "Gun";
        gun.transform.SetParent(cat.transform, true);

        Gun shooter = gun.GetComponent<Gun>();
        shooter.bulletSpawnPoint.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
    }

    private static Bounds GetVisualBounds(GameObject cat)
    {
        Renderer[] renderers = cat.GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(cat.transform.position, Vector3.zero);
        if (renderers.Length == 0)
            return bounds;

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

}
