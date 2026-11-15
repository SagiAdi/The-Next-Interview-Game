using UnityEngine.SceneManagement;

// Scene names. Every scene change goes through Scenes.Load.
public static class Scenes
{
    public const string Menu = "Menu";
    public const string City = "City";
    public const string Interview = "Interview";
    public const string Win = "Win";
    public const string GameOver = "GameOver";

    public static void Load(string sceneName)
    {
        // A panel that was open in the old scene must not leave the new scene paused.
        GameSession.Instance.SetPaused(false);
        SceneManager.LoadScene(sceneName);
    }
}
