using UnityEngine;
using UnityEngine.InputSystem;

public class Bullet : MonoBehaviour
{
    public float life = 3;
    void Awake()
    {
        Destroy(gameObject, life);
    }

    void OnCollisionEnter(Collision collision)
    {
        RatMovement rat = collision.gameObject.GetComponentInParent<RatMovement>();
        if (rat != null) Destroy(rat.gameObject);
        Destroy(gameObject);
    }
}
