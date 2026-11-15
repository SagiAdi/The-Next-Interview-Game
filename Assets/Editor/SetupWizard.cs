using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menu: Tools > Next Interview > Create Everything
// Creates the data assets, the prefabs and all five scenes, connects their references,
// and adds the scenes to the build list. It is safe to run it again: everything is rebuilt.
public static class SetupWizard
{
    private const string DataFolder = "Assets/Data";
    private const string PrefabsFolder = "Assets/Prefabs";
    private const string ScenesFolder = "Assets/Scenes";
    private const string CharactersFolder = "Assets/Art/Characters";

    private const string MessagesPath = DataFolder + "/Events/Messages.asset";
    private const string PuzzleRequestsPath = DataFolder + "/Events/PuzzleRequests.asset";
    private const string JobBoardOpenedPath = DataFolder + "/Events/JobBoardOpened.asset";
    private const string StationPrefabPath = PrefabsFolder + "/PuzzleStation.prefab";
    private const string DistractionPrefabPath = PrefabsFolder + "/Distraction.prefab";

    private static readonly Color WallColor = new Color(0.35f, 0.35f, 0.42f);
    private static readonly Color ScreenColor = new Color(0.08f, 0.1f, 0.16f);

    [MenuItem("Tools/Next Interview/Create Everything")]
    public static void CreateEverything()
    {
        EnsureFolder("Assets", "Data");
        EnsureFolder(DataFolder, "Events");
        EnsureFolder(DataFolder, "Jobs");
        EnsureFolder(DataFolder, "Distractions");
        EnsureFolder(DataFolder, "Physics");
        EnsureFolder("Assets", "Prefabs");
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets", "Resources");

        // Character sheets: 128x128 PNG = 4 directions x 4 frames, each 32x32.
        foreach (string sheet in new[] { "player", "npc_a", "npc_b", "npc_c" })
        {
            ImportSheet(CharactersFolder + "/" + sheet + ".png");
        }

        // Event channels and settings.
        var messages = LoadOrCreate<MessageChannel>(MessagesPath);
        var puzzleRequests = LoadOrCreate<PuzzleRequestChannel>(PuzzleRequestsPath);
        LoadOrCreate<JobBoardChannel>(JobBoardOpenedPath);
        LoadOrCreate<GameSettings>("Assets/Resources/GameSettings.asset");

        string[] jobPaths =
        {
            CreateJob("Junior Developer", 8000, 1, 60),
            CreateJob("Senior Game Developer", 16000, 4, 80),
            CreateJob("Lead Designer", 25000, 6, 90),
        };

        string[] distractionTypePaths =
        {
            CreateDistractionType("PhoneNotification", "Phone notification", new Color(1f, 0.35f, 0.35f), 0.45f, 0.4f, 3.2f),
            CreateDistractionType("SocialMediaFeed", "Social media feed", new Color(0.85f, 0.3f, 0.9f), 0.8f, 1.2f, 1.8f),
        };

        // Prefabs.
        GameObject stationObject = BuildVisual("PuzzleStation", Vector2.zero, new Vector2(0.8f, 0.8f), new Color(1f, 0.8f, 0.2f), false, 1);
        var station = stationObject.AddComponent<PuzzleStation>();
        Wire(station, "puzzleRequests", puzzleRequests);
        Wire(station, "messages", messages);
        SavePrefab(stationObject, StationPrefabPath);

        // Three contact prefabs with different looks. The spawner picks one at random.
        string[] npcSheets = { "npc_a", "npc_b", "npc_c" };
        var contactPrefabPaths = new string[npcSheets.Length];
        for (int i = 0; i < npcSheets.Length; i++)
        {
            GameObject npc = BuildVisual("ContactNPC_" + npcSheets[i], Vector2.zero, Vector2.one, Color.white, false, 1);
            Wire(npc.AddComponent<ContactNPC>(), "messages", messages);
            AddAnimator(npc, CharactersFolder + "/" + npcSheets[i] + ".png", true);
            contactPrefabPaths[i] = PrefabsFolder + "/ContactNPC_" + npcSheets[i] + ".prefab";
            SavePrefab(npc, contactPrefabPaths[i]);
        }

        SavePrefab(BuildDistraction(), DistractionPrefabPath);
        AssetDatabase.SaveAssets();

        // Scenes. Opening a new scene unloads the assets that were loaded above,
        // so every scene loads the assets it needs again, by path.
        CreateScreenScene("Menu", typeof(MenuScreen));
        CreateCityScene(contactPrefabPaths, distractionTypePaths, jobPaths);
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
        EditorSceneManager.OpenScene(ScenePath("Menu"));
        Debug.Log("Next Interview is ready. The Menu scene is open: press Play.");
    }

    // ---------- scenes ----------

    private static void CreateScreenScene(string name, System.Type screenScript)
    {
        var scene = NewScene();
        SetupCamera(ScreenColor);
        new GameObject(name + " Screen").AddComponent(screenScript);
        EditorSceneManager.SaveScene(scene, ScenePath(name));
    }

    private static void CreateEndScene(string name, bool won)
    {
        var scene = NewScene();
        SetupCamera(ScreenColor);
        WireBool(new GameObject(name + " Screen").AddComponent<EndScreen>(), "won", won);
        EditorSceneManager.SaveScene(scene, ScenePath(name));
    }

    private static void CreateInterviewScene()
    {
        var scene = NewScene();
        SetupCamera(ScreenColor);
        var interview = new GameObject("Interview Session").AddComponent<InterviewSession>();
        Wire(new GameObject("Interview Screen").AddComponent<InterviewScreen>(), "interview", interview);
        EditorSceneManager.SaveScene(scene, ScenePath("Interview"));
    }

    private static void CreateCityScene(string[] contactPrefabPaths, string[] distractionTypePaths, string[] jobPaths)
    {
        var scene = NewScene();
        SetupCamera(new Color(0.12f, 0.14f, 0.18f));

        var messages = Load<MessageChannel>(MessagesPath);
        var puzzleRequests = Load<PuzzleRequestChannel>(PuzzleRequestsPath);
        var jobBoardOpened = Load<JobBoardChannel>(JobBoardOpenedPath);
        GameObject stationPrefab = Load<GameObject>(StationPrefabPath);
        GameObject distractionPrefab = Load<GameObject>(DistractionPrefabPath);
        GameObject[] contactPrefabs = LoadAll<GameObject>(contactPrefabPaths);
        DistractionType[] distractionTypes = LoadAll<DistractionType>(distractionTypePaths);
        JobOffer[] jobs = LoadAll<JobOffer>(jobPaths);

        // Outer walls (solid: the player and the distractions collide with them).
        Transform walls = new GameObject("Walls").transform;
        BuildVisual("Wall Top", new Vector2(0f, 4.5f), new Vector2(15.5f, 0.5f), WallColor, true, 0).transform.SetParent(walls);
        BuildVisual("Wall Bottom", new Vector2(0f, -4.5f), new Vector2(15.5f, 0.5f), WallColor, true, 0).transform.SetParent(walls);
        BuildVisual("Wall Left", new Vector2(-7.5f, 0f), new Vector2(0.5f, 9.5f), WallColor, true, 0).transform.SetParent(walls);
        BuildVisual("Wall Right", new Vector2(7.5f, 0f), new Vector2(0.5f, 9.5f), WallColor, true, 0).transform.SetParent(walls);

        // Divider between the city and the puzzle room, with a door gap between y = -1 and y = 1.
        BuildVisual("Divider Top", new Vector2(2f, 2.75f), new Vector2(0.5f, 3.5f), WallColor, true, 0).transform.SetParent(walls);
        BuildVisual("Divider Bottom", new Vector2(2f, -2.75f), new Vector2(0.5f, 3.5f), WallColor, true, 0).transform.SetParent(walls);

        // Puzzle room floor (visual only, so its collider is removed).
        GameObject floor = BuildVisual("Puzzle Room Floor", new Vector2(4.5f, 0f), new Vector2(4.9f, 7f), new Color(0.2f, 0.25f, 0.35f, 0.35f), false, -1);
        Object.DestroyImmediate(floor.GetComponent<BoxCollider2D>());

        // Job board (a trigger zone: face it and interact to open the job list).
        GameObject boardObject = BuildVisual("Job Board", new Vector2(-5.5f, -3f), new Vector2(2.5f, 1.6f), new Color(0.2f, 0.5f, 1f, 0.6f), false, -1);
        var board = boardObject.AddComponent<JobBoardZone>();
        Wire(board, "jobBoardOpened", jobBoardOpened);
        WireArray(board, "jobs", jobs);

        // Spawners: puzzles inside the puzzle room, people and distractions in the city.
        CreateSpawner("Puzzle Spawner", new[] { stationPrefab }, new Vector2(2.9f, -3.3f), new Vector2(6.6f, 3.3f), 5f, 4);
        CreateSpawner("Contact Spawner", contactPrefabs, new Vector2(-6.5f, -1.5f), new Vector2(1.2f, 3.3f), 8f, 3);
        Spawner distractionSpawner = CreateSpawner("Distraction Spawner", new[] { distractionPrefab }, new Vector2(-6.5f, -3.3f), new Vector2(1.2f, 3.3f), 12f, 3);
        WireArray(distractionSpawner.gameObject.AddComponent<DistractionLauncher>(), "types", distractionTypes);

        // Player: animated sprite, a physics body, and a small solid collider at the feet.
        GameObject player = BuildVisual("Player", new Vector2(-4f, 0f), Vector2.one, Color.white, true, 2);
        var feet = player.GetComponent<BoxCollider2D>();
        feet.size = new Vector2(0.6f, 0.4f);
        feet.offset = new Vector2(0f, -0.3f);

        var body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        AddAnimator(player, CharactersFolder + "/player.png", false);
        var input = player.AddComponent<PlayerInputReader>();
        player.AddComponent<PlayerMover>();
        var sensor = player.AddComponent<InteractionSensor>();
        player.AddComponent<PlayerInteractor>();
        Wire(player.AddComponent<PlayerFocus>(), "messages", messages);
        player.AddComponent<VelocityAnimationDriver>();

        // Round rules.
        var round = new GameObject("Round");
        round.AddComponent<RoundTimer>();
        round.AddComponent<RoundRules>();

        // UI: every element listens to the game through events or channels.
        Transform ui = new GameObject("UI").transform;
        AddUi<RoundHud>(ui, "Round HUD");
        Wire(AddUi<MessageLabel>(ui, "Message Label"), "messages", messages);
        Wire(AddUi<InteractPrompt>(ui, "Interact Prompt"), "sensor", sensor);
        Wire(AddUi<TouchControls>(ui, "Touch Controls"), "input", input);
        Wire(AddUi<PuzzlePanel>(ui, "Puzzle Panel"), "puzzleRequests", puzzleRequests);
        Wire(AddUi<JobBoardPanel>(ui, "Job Board Panel"), "jobBoardOpened", jobBoardOpened);

        EditorSceneManager.SaveScene(scene, ScenePath("City"));
    }

    // ---------- building blocks ----------

    private static T AddUi<T>(Transform parent, string name) where T : Component
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        return go.AddComponent<T>();
    }

    private static Spawner CreateSpawner(string name, GameObject[] prefabs, Vector2 areaMin, Vector2 areaMax, float interval, int maxAlive)
    {
        var spawner = new GameObject(name).AddComponent<Spawner>();
        WireArray(spawner, "prefabs", prefabs);
        WireVector2(spawner, "areaMin", areaMin);
        WireVector2(spawner, "areaMax", areaMax);
        WireFloat(spawner, "interval", interval);
        WireInt(spawner, "maxAlive", maxAlive);
        return spawner;
    }

    // A colored rectangle with a box collider. solid = things collide with it. Not solid = trigger.
    private static GameObject BuildVisual(string name, Vector2 pos, Vector2 scale, Color color, bool solid, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(scale.x, scale.y, 1f);

        var spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = sortingOrder;
        go.AddComponent<Placeholder>();

        BoxCollider2D box = go.AddComponent<BoxCollider2D>();
        box.size = Vector2.one; // without this the collider is created with size 0 (the object has no sprite yet)
        box.isTrigger = !solid;
        return go;
    }

    // A bouncing ball: circle collider with a bouncy material, and a physics body without gravity (top-down game).
    private static GameObject BuildDistraction()
    {
        string materialPath = DataFolder + "/Physics/Bouncy.physicsMaterial2D";
        var bouncy = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(materialPath);
        if (bouncy == null)
        {
            bouncy = new PhysicsMaterial2D("Bouncy");
            bouncy.bounciness = 1f;
            bouncy.friction = 0f;
            AssetDatabase.CreateAsset(bouncy, materialPath);
        }

        var go = new GameObject("Distraction");
        go.AddComponent<SpriteRenderer>().sortingOrder = 3;
        WireBool(go.AddComponent<Placeholder>(), "round", true);

        var circle = go.AddComponent<CircleCollider2D>();
        circle.radius = 0.5f;
        circle.sharedMaterial = bouncy;

        var body = go.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.linearDamping = 0f;
        body.angularDamping = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        go.AddComponent<Distraction>();
        return go;
    }

    private static void AddAnimator(GameObject go, string sheetPath, bool autoPlay)
    {
        // Sprite names are d<row>_f<col>, so sorting by name gives: down, left, right, up.
        Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().OrderBy(sprite => sprite.name).ToArray();
        if (frames.Length < 16)
        {
            Debug.LogError("Character sheet missing or not sliced: " + sheetPath);
            return;
        }

        var animator = go.AddComponent<SpriteAnimator>();
        WireArray(animator, "frames", frames);
        WireBool(animator, "autoPlay", autoPlay);
        go.GetComponent<SpriteRenderer>().sprite = frames[0];
    }

    private static void ImportSheet(string path)
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

    // ---------- assets ----------

    // Creates (or updates) a job asset and returns its path.
    private static string CreateJob(string title, int salary, int projects, int score)
    {
        string path = DataFolder + "/Jobs/" + title.Replace(" ", "") + ".asset";
        var job = LoadOrCreate<JobOffer>(path);
        job.title = title;
        job.salary = salary;
        job.requiredProjects = projects;
        job.requiredScore = score;
        EditorUtility.SetDirty(job);
        return path;
    }

    // Creates a distraction type asset (if it does not exist yet) and returns its path.
    private static string CreateDistractionType(string fileName, string displayName, Color color, float size, float mass, float cruiseSpeed)
    {
        string path = DataFolder + "/Distractions/" + fileName + ".asset";
        if (AssetDatabase.LoadAssetAtPath<DistractionType>(path) != null)
        {
            return path; // keep the values that were tuned in the Inspector
        }

        var type = LoadOrCreate<DistractionType>(path);
        type.displayName = displayName;
        type.color = color;
        type.size = size;
        type.mass = mass;
        type.cruiseSpeed = cruiseSpeed;
        EditorUtility.SetDirty(type);
        return path;
    }

    private static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
        }

        return asset;
    }

    private static T Load<T>(string path) where T : Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            throw new System.IO.FileNotFoundException("Asset not found: " + path);
        }

        return asset;
    }

    private static T[] LoadAll<T>(string[] paths) where T : Object
    {
        return paths.Select(path => Load<T>(path)).ToArray();
    }

    private static GameObject SavePrefab(GameObject go, string path)
    {
        GameObject asset = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return asset;
    }

    private static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static UnityEngine.SceneManagement.Scene NewScene()
    {
        // DefaultGameObjects adds the Main Camera that SetupCamera needs.
        return EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
    }

    private static void SetupCamera(Color background)
    {
        var camera = Camera.main;
        camera.orthographic = true;
        camera.orthographicSize = 5.5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = background;
        camera.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static string ScenePath(string name)
    {
        return ScenesFolder + "/" + name + ".unity";
    }

    // ---------- wiring: sets a [SerializeField] private field of a component, by name ----------

    private static SerializedProperty Find(SerializedObject serialized, string field)
    {
        SerializedProperty property = serialized.FindProperty(field);
        if (property == null)
        {
            throw new System.ArgumentException("No serialized field '" + field + "' on " + serialized.targetObject.GetType().Name);
        }

        return property;
    }

    private static void Wire(Object target, string field, Object value)
    {
        if (value == null)
        {
            throw new System.ArgumentException("Nothing to put in the field '" + field + "' of " + target.GetType().Name);
        }

        var serialized = new SerializedObject(target);
        Find(serialized, field).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WireBool(Object target, string field, bool value)
    {
        var serialized = new SerializedObject(target);
        Find(serialized, field).boolValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WireInt(Object target, string field, int value)
    {
        var serialized = new SerializedObject(target);
        Find(serialized, field).intValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WireFloat(Object target, string field, float value)
    {
        var serialized = new SerializedObject(target);
        Find(serialized, field).floatValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WireVector2(Object target, string field, Vector2 value)
    {
        var serialized = new SerializedObject(target);
        Find(serialized, field).vector2Value = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WireArray(Object target, string field, Object[] values)
    {
        var serialized = new SerializedObject(target);
        SerializedProperty property = Find(serialized, field);
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == null)
            {
                throw new System.ArgumentException("Empty item " + i + " for the field '" + field + "' of " + target.GetType().Name);
            }

            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
