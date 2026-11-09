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
        PlayerController2D.Frozen = false;
        SceneManager.LoadScene(sceneName);
    }
}
