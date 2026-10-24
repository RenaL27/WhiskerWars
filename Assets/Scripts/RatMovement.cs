using UnityEngine;

public class RatMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 0.025f;
    [SerializeField] private Vector3 moveDirection = Vector3.left;
    [SerializeField] private bool useGrayRat;

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    [Header("Blocker attack")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackInterval = 1f;
    private float nextAttackTime;
    private Renderer[] bodyRenderers;
    private Vector3 travelDirection;
    private bool finishHandled;

    private void Awake()
    {
        string modelName = useGrayRat ? "GrayRat" : "BrownRat";
        GameObject ratModel = Resources.Load<GameObject>("Models/" + modelName);
        if (ratModel == null)
        {
            Debug.LogError("Could not load the " + modelName + " model.", this);
            return;
        }

        // Keep the existing movement and collider; replace only the old green mesh.
        foreach (Renderer oldRenderer in GetComponentsInChildren<Renderer>())
            oldRenderer.enabled = false;

        GameObject visual = Instantiate(ratModel, transform);
        visual.name = modelName + " Visual";
        visual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(-90f, 0f, 0f));

        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        float largestDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (largestDimension > 0.001f)
            visual.transform.localScale *= 0.8f / largestDimension;

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        visual.transform.position += new Vector3(
            transform.position.x - bounds.center.x,
            transform.position.y - 0.4f - bounds.min.y,
            transform.position.z - bounds.center.z);
    }

    private void Start()
    {
        bodyRenderers = System.Array.FindAll(GetComponentsInChildren<Renderer>(), renderer => renderer.enabled);
        travelDirection = moveDirection.normalized;
    }

    public void ReachFinish()
    {
        if (finishHandled || !GameFlow.IsPlaying || Time.timeScale == 0f) return;
        finishHandled = true;
        if (!Lawnmower.TryActivate(this)) GameFlow.Instance?.EndRound(false);
    }

    void Update()
    {
        if (!GameFlow.IsPlaying || Time.timeScale == 0f || finishHandled || AttackBlocker()) return;
        transform.position +=
            moveDirection.normalized * moveSpeed * Time.deltaTime;
    }
    private bool AttackBlocker()
    {
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        if (bodyRenderers.Length > 0)
        {
            bounds = bodyRenderers[0].bounds;
            for (int i = 1; i < bodyRenderers.Length; i++)
                bounds.Encapsulate(bodyRenderers[i].bounds);
        }
        PinkCatBlocker blocker = PinkCatBlocker.FindInReach(bounds, travelDirection,
            0.08f + moveSpeed * Time.deltaTime);
        if (blocker == null) return false;
        if (Time.timeScale > 0f && Time.time >= nextAttackTime)
        {
            blocker.TakeDamage(attackDamage);
            nextAttackTime = Time.time + attackInterval;
        }
        return true;
    }

}
