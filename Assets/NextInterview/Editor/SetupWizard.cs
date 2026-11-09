using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menu: Tools > Next Interview > Create Everything
// Creates folders, job assets, character sprites, prefabs and all five scenes,
// and adds the scenes to Build Settings.
public static class SetupWizard
{
    const string Root = "Assets/NextInterview";
    const string CharactersFolder = Root + "/Art/Characters";
    static readonly Color Gray = new Color(0.35f, 0.35f, 0.42f);

    [MenuItem("Tools/Next Interview/Create Everything")]
    public static void CreateEverything()
    {
        EnsureFolder("Assets", "NextInterview");
        EnsureFolder(Root, "Prefabs");
        EnsureFolder(Root, "Data");
        EnsureFolder(Root, "Scenes");
        EnsureFolder(Root, "Art");
        EnsureFolder(Root + "/Art", "Characters");

        // Character sheets: 128x128 PNG = 4 directions x 4 frames, each 32x32
        foreach (string sheet in new[] { "player", "npc_a", "npc_b", "npc_c" })
            ImportSheet(CharactersFolder + "/" + sheet + ".png");

        JobOffer[] jobs =
        {
            CreateJob("Junior Developer", 8000, 1, 60),
            CreateJob("Senior Game Developer", 16000, 4, 80),
            CreateJob("Lead Designer", 25000, 6, 90),
        };

        // Old single prefab from the first version, no longer used
        AssetDatabase.DeleteAsset(Root + "/Prefabs/ContactNPC.prefab");

        GameObject station = SavePrefab(
            BuildVisual("PuzzleStation", Vector2.zero, new Vector2(0.8f, 0.8f), new Color(1f, 0.8f, 0.2f), false, typeof(PuzzleStation)),
            Root + "/Prefabs/PuzzleStation.prefab");

        // Three NPC prefabs with different looks. The spawner picks one at random.
        string[] npcSheets = { "npc_a", "npc_b", "npc_c" };
        var contacts = new GameObject[npcSheets.Length];
        for (int i = 0; i < npcSheets.Length; i++)
        {
            GameObject npc = BuildVisual("ContactNPC_" + npcSheets[i], Vector2.zero, Vector2.one, Color.white, false, typeof(ContactNPC));
            AddAnimator(npc, CharactersFolder + "/" + npcSheets[i] + ".png", autoPlay: true);
            contacts[i] = SavePrefab(npc, Root + "/Prefabs/ContactNPC_" + npcSheets[i] + ".prefab");
        }

        CreateMenuScene();
        CreateCityScene(station, contacts, jobs);
        CreateInterviewScene();
        CreateEndScene("Win", true);
        CreateEndScene("GameOver", false);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath("Menu"), true),
            new EditorBuildSettingsScene(ScenePath("City"), true),
            new EditorBuildSettingsScene(ScenePath("Interview"), true),
            new EditorBuildSettingsScene(ScenePath("Win"), true),
            new EditorBuildSettingsScene(ScenePath("GameOver"), true),
        };

        AssetDatabase.SaveAssets();
        Debug.Log("Next Interview is ready. Open Assets/NextInterview/Scenes/Menu.unity and press Play.");
    }

    static void CreateMenuScene()
    {
        var scene = NewScene();
        SetupCamera(new Color(0.08f, 0.1f, 0.16f), 5.5f);
        new GameObject("Menu").AddComponent<MenuController>();
        EditorSceneManager.SaveScene(scene, ScenePath("Menu"));
    }

    static void CreateCityScene(GameObject station, GameObject[] contactPrefabs, JobOffer[] jobs)
    {
        var scene = NewScene();
        SetupCamera(new Color(0.12f, 0.14f, 0.18f), 5.5f);

        // Outer walls (solid: the player collides with them)
        BuildVisual("Wall Top", new Vector2(0f, 4.5f), new Vector2(15.5f, 0.5f), Gray, true);
        BuildVisual("Wall Bottom", new Vector2(0f, -4.5f), new Vector2(15.5f, 0.5f), Gray, true);
        BuildVisual("Wall Left", new Vector2(-7.5f, 0f), new Vector2(0.5f, 9.5f), Gray, true);
        BuildVisual("Wall Right", new Vector2(7.5f, 0f), new Vector2(0.5f, 9.5f), Gray, true);

        // Divider between the city and the puzzle room, with a door gap between y = -1 and y = 1
        BuildVisual("Divider Top", new Vector2(2f, 2.75f), new Vector2(0.5f, 3.5f), Gray, true);
        BuildVisual("Divider Bottom", new Vector2(2f, -2.75f), new Vector2(0.5f, 3.5f), Gray, true);

        // Puzzle room floor (visual only). The puzzle spawner fills this area.
        BuildVisual("Puzzle Room", new Vector2(4.5f, 0f), new Vector2(4.9f, 7f), new Color(0.2f, 0.25f, 0.35f, 0.35f), false);

        // Job board room (trigger zone, interact to open the job list)
        GameObject board = BuildVisual("Job Board", new Vector2(-5.5f, -3f), new Vector2(2.5f, 1.6f),
            new Color(0.2f, 0.5f, 1f, 0.6f), false, typeof(JobBoardZone));
        board.GetComponent<JobBoardZone>().Setup(jobs);

        // Spawners: puzzles inside the puzzle room, people in the city
        var puzzles = new GameObject("Puzzle Spawner").AddComponent<Spawner>();
        puzzles.Setup(new[] { station }, new Vector2(2.6f, -3.3f), new Vector2(6.6f, 3.3f), 5f, 4);

        var contacts = new GameObject("Contact Spawner").AddComponent<Spawner>();
        contacts.Setup(contactPrefabs, new Vector2(-6.5f, -3.3f), new Vector2(1.5f, 3.3f), 8f, 3);

        // Player: animated sprite, physics body, and a small collider at the feet
        GameObject player = BuildVisual("Player", new Vector2(-4f, 0f), Vector2.one, Color.white, true, typeof(PlayerController2D));
        var feet = player.GetComponent<BoxCollider2D>();
        feet.size = new Vector2(0.6f, 0.4f);
        feet.offset = new Vector2(0f, -0.3f);
        AddAnimator(player, CharactersFolder + "/player.png", autoPlay: false);

        new GameObject("HUD").AddComponent<HUD>();

        EditorSceneManager.SaveScene(scene, ScenePath("City"));
    }

    static void CreateInterviewScene()
    {
        var scene = NewScene();
        SetupCamera(new Color(0.08f, 0.1f, 0.16f), 5.5f);
        new GameObject("Interview").AddComponent<InterviewManager>();
        EditorSceneManager.SaveScene(scene, ScenePath("Interview"));
    }

    static void CreateEndScene(string name, bool won)
    {
        var scene = NewScene();
        SetupCamera(new Color(0.08f, 0.1f, 0.16f), 5.5f);
        new GameObject(name + " Screen").AddComponent<EndScreen>().Setup(won);
        EditorSceneManager.SaveScene(scene, ScenePath(name));
    }

    // ---------- helpers ----------

    static UnityEngine.SceneManagement.Scene NewScene()
    {
        // DefaultGameObjects adds a Main Camera and a light, which SetupCamera needs.
        return EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
    }

    static void SetupCamera(Color background, float size)
    {
        var cam = Camera.main;
        cam.orthographic = true;
        cam.orthographicSize = size;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = background;
        cam.transform.position = new Vector3(0f, 0f, -10f);
    }

    static GameObject BuildVisual(string name, Vector2 pos, Vector2 scale, Color color, bool solid, System.Type script = null)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        go.AddComponent<SpriteRenderer>().color = color;
        go.AddComponent<Placeholder>();
        BoxCollider2D box = go.AddComponent<BoxCollider2D>();
        box.size = Vector2.one; // without this the collider is created with size 0 (it has no shape)
        box.isTrigger = !solid;
        if (script != null) go.AddComponent(script);
        return go;
    }

    static SpriteAnimator AddAnimator(GameObject go, string sheetPath, bool autoPlay)
    {
        Sprite[] frames = LoadFrames(sheetPath);
        if (frames.Length < 16)
        {
            Debug.LogError("Character sheet missing or not sliced: " + sheetPath + ". Copy the PNG files into Art/Characters and run the wizard again.");
            return null;
        }

        var anim = go.AddComponent<SpriteAnimator>();
        anim.Setup(frames, autoPlay);
        go.GetComponent<SpriteRenderer>().sprite = frames[0];
        return anim;
    }

    static Sprite[] LoadFrames(string sheetPath)
    {
        // Names are d<row>_f<col>, so sorting by name gives: down, left, right, up.
        return AssetDatabase.LoadAllAssetsAtPath(sheetPath)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToArray();
    }

    static void ImportSheet(string path)
    {
        AssetDatabase.ImportAsset(path);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("Character sheet not found: " + path);
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 32;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        var rects = new List<SpriteMetaData>();
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                rects.Add(new SpriteMetaData
                {
                    name = "d" + row + "_f" + col,
                    rect = new Rect(col * 32, 128 - (row + 1) * 32, 32, 32), // Unity counts Y from the bottom
                    pivot = new Vector2(0.5f, 0.5f),
                    alignment = (int)SpriteAlignment.Center
                });
            }
        }
        importer.spritesheet = rects.ToArray();
        importer.SaveAndReimport();
    }

    static GameObject SavePrefab(GameObject go, string path)
    {
        GameObject asset = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return asset;
    }

    static JobOffer CreateJob(string title, int salary, int projects, int score)
    {
        string path = Root + "/Data/" + title.Replace(" ", "") + ".asset";
        var existing = AssetDatabase.LoadAssetAtPath<JobOffer>(path);
        if (existing != null) return existing;

        var job = ScriptableObject.CreateInstance<JobOffer>();
        job.title = title;
        job.salary = salary;
        job.requiredProjects = projects;
        job.requiredScore = score;
        AssetDatabase.CreateAsset(job, path);
        return job;
    }

    static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name))
            AssetDatabase.CreateFolder(parent, name);
    }

    static string ScenePath(string name) => Root + "/Scenes/" + name + ".unity";
}
