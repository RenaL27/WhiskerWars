using UnityEngine;

public class FinishLine : MonoBehaviour
{
    // Resolve the rat root even when a child collider enters the finish line.
    private void OnTriggerEnter(Collider other) => CheckRat(other);
    private void OnTriggerStay(Collider other) => CheckRat(other);

    private void CheckRat(Collider other)
    {
        RatMovement rat = other.GetComponentInParent<RatMovement>();
        if (rat != null) rat.ReachFinish();
    }
}
