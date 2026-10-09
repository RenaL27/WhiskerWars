using UnityEngine;
using System.Collections;

public class FighterRatMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float tileSpacing = 1f;
    [SerializeField] private float pauseBetweenSteps = 1f;
    [SerializeField] private Vector3 moveDirection = Vector3.left;

    void Start()
    {
        StartCoroutine(MoveAlongGrid());
    }

    IEnumerator MoveAlongGrid()
    {
        while (true)
        {
            Vector3 startPosition = transform.position;

            Vector3 targetPosition = startPosition
                + moveDirection.normalized * tileSpacing;

            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    moveSpeed * Time.deltaTime
                );

                yield return null;
            }

            transform.position = targetPosition;
            yield return new WaitForSeconds(pauseBetweenSteps);
        }
    }
}
