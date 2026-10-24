using UnityEngine;
using UnityEngine.SceneManagement;

// Owns the illustrated menus and one finite round. Assets use 1920x1080 coordinates.
public class GameFlow : MonoBehaviour
{
    public enum ScreenState { Opening, Instructions, Playing, Win, Lose }
    public static GameFlow Instance { get; private set; }
    public ScreenState State { get; private set; } = ScreenState.Opening;
    public static bool IsPlaying => Instance != null && Instance.State == ScreenState.Playing;
    private Texture2D opening, instructions, win, lose;
    private RatSpawner[] spawners;
    private bool roundStarted;
    private int screenChangedFrame = -1;

    public static void EnsureExists()
    {
        if (Instance == null) new GameObject("Game Flow").AddComponent<GameFlow>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        opening = Resources.Load<Texture2D>("GameScreens/Opening");
        instructions = Resources.Load<Texture2D>("GameScreens/Instructions");
        win = Resources.Load<Texture2D>("GameScreens/Win");
        lose = Resources.Load<Texture2D>("GameScreens/Lose");
        AudioClip song = Resources.Load<AudioClip>("Music/Whisker Wars Main Staff");
        if (song != null)
        {
            AudioSource music = gameObject.AddComponent<AudioSource>();
            music.clip = song;
            music.loop = true;
            music.playOnAwake = false;
            music.spatialBlend = 0f;
            music.volume = 0.5f;
            music.Play();
        }
        Time.timeScale = 0f;
    }

    private void Start() => spawners = FindObjectsByType<RatSpawner>(FindObjectsSortMode.None);

    private void Update()
    {
        if (!IsPlaying || !roundStarted || spawners == null || spawners.Length == 0) return;
        foreach (RatSpawner spawner in spawners)
            if (spawner != null && !spawner.Finished) return;
        if (FindFirstObjectByType<RatMovement>() == null) EndRound(true);
    }

    public void EndRound(bool success)
    {
        if (!IsPlaying) return;
        // Release the picker's pause before applying the end screen's pause.
        CatSelectionScreen picker = FindFirstObjectByType<CatSelectionScreen>();
        if (picker != null) picker.Dismiss();
        State = success ? ScreenState.Win : ScreenState.Lose;
        screenChangedFrame = Time.frameCount;
        Time.timeScale = 0f;
    }

    private void BeginRound()
    {
        State = ScreenState.Playing;
        roundStarted = true;
        Time.timeScale = 1f;
        FindFirstObjectByType<CatSelectionScreen>()?.ShowPicker();
    }

    private void ReturnToOpening()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        Instance = null;
        Time.timeScale = 1f;
    }

    private void OnGUI()
    {
        if (State == ScreenState.Playing) return;
        GUI.depth = -100;
        Texture2D art = opening;
        Rect hit = new Rect(842, 857, 349, 134);
        switch (State)
        {
            case ScreenState.Instructions: art = instructions; hit = new Rect(996, 893, 484, 124); break;
            case ScreenState.Win: art = win; hit = new Rect(142, 877, 455, 108); break;
            case ScreenState.Lose: art = lose; hit = new Rect(732, 909, 455, 108); break;
        }
        Color oldColor = GUI.color;
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = oldColor;
        float scale = Mathf.Min(Screen.width / 1920f, Screen.height / 1080f);
        Matrix4x4 previous = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1920 * scale) / 2,
            (Screen.height - 1080 * scale) / 2, 0), Quaternion.identity, Vector3.one * scale);
        if (art != null) GUI.DrawTexture(new Rect(0, 0, 1920, 1080), art);
        // Transparent button preserves the lettering already painted into each image.
        bool clicked = GUI.Button(hit, art == null ? "Continue" : "", art == null ? GUI.skin.button : GUIStyle.none);
        if (hit.Contains(Event.current.mousePosition))
        {
            GUI.color = new Color(1, 1, 1, 0.12f);
            GUI.DrawTexture(hit, Texture2D.whiteTexture);
            GUI.color = oldColor;
        }
        GUI.matrix = previous;
        if (!clicked || Time.frameCount <= screenChangedFrame + 1) return;
        screenChangedFrame = Time.frameCount;
        if (State == ScreenState.Opening) State = ScreenState.Instructions;
        else if (State == ScreenState.Instructions) BeginRound();
        else ReturnToOpening();
    }
}
