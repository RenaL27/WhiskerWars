using UnityEngine;
using UnityEngine.InputSystem;

public class Bullet : MonoBehaviour
{
    [SerializeField] private GameObject visualPrefab;
    public float life = 3;
    void Awake()
    {
        if (visualPrefab != null)
        {
            MeshRenderer placeholder = GetComponent<MeshRenderer>();
            Vector3 center = placeholder != null ? placeholder.bounds.center : transform.position;
            GameObject visual = Instantiate(visualPrefab, transform);
            visual.name = "Yarn Bullet Visual";
            visual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(-90f, 0f, 0f));
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                float largestDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                // The model includes a long strand; size it so the yarn ball is visible in play.
                if (largestDimension > 0.001f) visual.transform.localScale *= 0.75f / largestDimension;
                bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                visual.transform.position += center - bounds.center;
                if (placeholder != null) placeholder.enabled = false;
            }
            else Destroy(visual);
        }
        Destroy(gameObject, life);
    }

    void OnCollisionEnter(Collision collision)
    {
        RatMovement rat = collision.gameObject.GetComponentInParent<RatMovement>();
        if (rat != null) rat.TakeHit();
        Destroy(gameObject);
    }
}
