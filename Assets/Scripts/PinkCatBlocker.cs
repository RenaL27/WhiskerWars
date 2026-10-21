using System.Collections.Generic;
using UnityEngine;

// Lane blocking is independent of physics, so friendly bullets can pass through.
public class PinkCatBlocker : MonoBehaviour
{
    private static readonly HashSet<PinkCatBlocker> activeBlockers = new();
    [SerializeField, Min(1f)] private float maxHealth = 150f;
    private float health;
    private Bounds tileBounds;
    private Camera gameCamera;
    public float Health => health;

    public void Initialize(Bounds tile)
    {
        tileBounds = tile;
        health = maxHealth;
        gameCamera = Camera.main;
    }

    private void OnEnable() => activeBlockers.Add(this);
    private void OnDisable() => activeBlockers.Remove(this);

    public void TakeDamage(float damage)
    {
        if (health <= 0f || damage <= 0f) return;
        health = Mathf.Max(0f, health - damage);
        if (health == 0f)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    public static PinkCatBlocker FindInReach(Bounds rat, Vector3 direction, float reach)
    {
        PinkCatBlocker closest = null;
        float closestDistance = float.PositiveInfinity;
        Vector3 sideways = Vector3.Cross(Vector3.up, direction).normalized;
        foreach (PinkCatBlocker blocker in activeBlockers)
        {
            if (blocker.health <= 0f) continue;
            Vector3 offset = blocker.tileBounds.center - rat.center;
            // Only stop rats in this tile's lane, never a neighbouring lane.
            float laneHalfWidth = Vector3.Dot(blocker.tileBounds.extents,
                new Vector3(Mathf.Abs(sideways.x), 0f, Mathf.Abs(sideways.z)));
            if (Mathf.Abs(Vector3.Dot(offset, sideways)) > laneHalfWidth) continue;
            float distance = Vector3.Dot(offset, direction);
            Vector3 axis = new Vector3(Mathf.Abs(direction.x), 0f, Mathf.Abs(direction.z));
            float contactDistance = Vector3.Dot(rat.extents + blocker.tileBounds.extents * 0.85f, axis);
            if (distance < -contactDistance || distance > contactDistance + reach) continue;
            if (distance < closestDistance)
            {
                closest = blocker;
                closestDistance = distance;
            }
        }
        return closest;
    }

    private void OnGUI()
    {
        if (health <= 0f || health >= maxHealth || gameCamera == null) return;
        Vector3 point = gameCamera.WorldToScreenPoint(tileBounds.center + Vector3.up * 0.8f);
        if (point.z <= 0f) return;
        float scale = Mathf.Max(1f, Screen.height / 720f);
        Rect bar = new Rect(point.x - 24 * scale, Screen.height - point.y - 8 * scale, 48 * scale, 6 * scale);
        Color previous = GUI.color;
        GUI.color = new Color(0.12f, 0.05f, 0.08f);
        GUI.DrawTexture(bar, Texture2D.whiteTexture);
        bar.width *= health / maxHealth;
        GUI.color = Color.Lerp(new Color(1f, 0.25f, 0.3f), new Color(1f, 0.6f, 0.8f), health / maxHealth);
        GUI.DrawTexture(bar, Texture2D.whiteTexture);
        GUI.color = previous;
    }
}
