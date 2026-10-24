using UnityEngine;

// A runtime overlay keeps the picker available without a separate scene or UI setup.
public class CatSelectionScreen : MonoBehaviour
{
    private CatPlacement placement;
    private bool pinkSelected;
    private float previousTimeScale;
    private int closedFrame = -1;
    private GUIStyle title, subtitle, card, button, status, hudText, hudButton;
    private static readonly Color Background = new Color(0.055f, 0.07f, 0.12f, 0.97f);
    private static readonly Color Pink = new Color(1f, 0.58f, 0.76f);
    private static readonly Color Cream = new Color(1f, 0.85f, 0.6f);
    public bool IsOpen { get; private set; }
    public bool JustClosed => Time.frameCount <= closedFrame + 1;

    public void Initialize(CatPlacement owner)
    {
        placement = owner;
    }

    public void ShowPicker() => Open();
    public void Dismiss() => Close();

    private void Open()
    {
        if (IsOpen || !GameFlow.IsPlaying) return;
        previousTimeScale = Time.timeScale;
        IsOpen = true;
        Time.timeScale = 0f;
    }

    private void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        closedFrame = Time.frameCount;
        Time.timeScale = previousTimeScale;
    }

    private void OnDisable() => Close();

    // IMGUI does not participate in EventSystem raycasts. Explicitly block the HUD.
    public bool BlocksPlacement(Vector2 mousePosition)
    {
        return IsOpen || JustClosed || HudRect
            .Contains(new Vector2(mousePosition.x, Screen.height - mousePosition.y));
    }

    // Scale with the rendered resolution so a 1080p or Retina Game view stays readable.
    private static float UiScale => Mathf.Clamp(
        Mathf.Min(Screen.width / 1280f, Screen.height / 720f), 0.75f, 3f);
    private static Rect HudRect => new Rect(10 * UiScale, 10 * UiScale,
        Mathf.Min(280 * UiScale, Screen.width - 20 * UiScale), 84 * UiScale);

    private void CreateStyles()
    {
        if (title != null) return;
        title = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        subtitle = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        card = new GUIStyle(GUI.skin.button) { fontSize = 19, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        button = new GUIStyle(GUI.skin.button) { fontSize = 16, fontStyle = FontStyle.Bold };
        status = new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        hudText = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleLeft };
        hudButton = new GUIStyle(GUI.skin.button) { fontSize = 14 };
    }

    private static void Fill(Rect rect, Color color)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = old;
    }

    private void OnGUI()
    {
        if (!GameFlow.IsPlaying || placement == null || !placement.enabled) return;
        CreateStyles();
        if (!IsOpen)
        {
            Matrix4x4 hudMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(Vector3.one * UiScale);
            Rect hud = new Rect(10, 10, HudRect.width / UiScale, 84);
            Fill(hud, Background);
            GUI.Label(new Rect(hud.x + 10, hud.y + 8, hud.width - 100, 28),
                "Level " + GameFlow.Instance.CurrentLevel + " - " + placement.SelectedCatName, hudText);
            if (GUI.Button(new Rect(hud.xMax - 86, hud.y + 10, 76, 28), "Change", hudButton)) Open();
            string message = placement.CooldownRemaining > 0
                ? $"Ready in {Mathf.CeilToInt(placement.CooldownRemaining)}s"
                : "Ready - click an empty tile";
            GUI.Label(new Rect(hud.x + 10, hud.y + 44, hud.width - 20, 28), message, hudText);
            GUI.matrix = hudMatrix;
            return;
        }

        Matrix4x4 oldMatrix = GUI.matrix;
        // Enlarge the picker with resolution while leaving a margin around the board.
        float scale = Mathf.Min(UiScale * 1.15f, Screen.width * 0.8f / 560f, Screen.height * 0.8f / 330f);
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 560 * scale) / 2,
            (Screen.height - 330 * scale) / 2, 0), Quaternion.identity, Vector3.one * scale);
        Fill(new Rect(0, 0, 560, 330), Background);
        GUI.Label(new Rect(20, 16, 520, 40), "Choose your cat", title);
        GUI.Label(new Rect(20, 58, 520, 28), "Choose a cat, then click an empty tile.", subtitle);

        DrawCard(new Rect(24, 104, 248, 125), false, "Regular Cat", Cream, true);
        DrawCard(new Rect(288, 104, 248, 125), true, "Pink Cat", Pink, placement.PinkCatAvailable);
        GUI.Label(new Rect(20, 238, 520, 24), "Game paused while choosing. Switch anytime.", status);
        if (GUI.Button(new Rect(160, 276, 240, 36), "Place " + (pinkSelected ? "Pink Cat" : "Regular Cat"), button))
        {
            placement.SelectCat(pinkSelected);
            Close();
        }
        GUI.matrix = oldMatrix;
    }

    private void DrawCard(Rect rect, bool pink, string name, Color accent, bool available)
    {
        bool selected = pinkSelected == pink;
        Fill(new Rect(rect.x - 3, rect.y - 3, rect.width + 6, rect.height + 6),
            selected ? accent : new Color(0.22f, 0.25f, 0.32f));
        Color old = GUI.backgroundColor;
        GUI.backgroundColor = accent;
        GUI.enabled = available;
        string caption = name + (pink ? "\nBlocks rats" : "\nShoots rats") + "\n" + (available ? (selected ? "SELECTED" : "Select cat") : "Unavailable");
        if (GUI.Button(rect, caption, card)) pinkSelected = pink;
        GUI.enabled = true;
        GUI.backgroundColor = old;
    }
}
