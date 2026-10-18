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

    void Update()
    {
        transform.position +=
            moveDirection.normalized * moveSpeed * Time.deltaTime;
    }
}
