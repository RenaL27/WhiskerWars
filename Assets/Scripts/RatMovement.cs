using UnityEngine;

public class RatMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 0.025f;
    [SerializeField] private Vector3 moveDirection = Vector3.left;

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

    private void Start()
    {
        bodyRenderers = GetComponentsInChildren<Renderer>();
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
