using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class GameSceneBuilder
{
    [MenuItem("Tools/Jezne Ptice/Setup GameScene")]
    public static void SetupGameScene()
    {
        if (!EditorUtility.DisplayDialog("Setup GameScene",
            "To bo zbrisalo VSE iz scene in jo postavilo na novo.\nNadaljuješ?",
            "Da, zbriši in postavi", "Prekliči"))
            return;

        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var go in allObjects)
            if (go != null && go.transform != null && go.transform.parent == null)
                Object.DestroyImmediate(go);

        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.13f, 0.13f, 0.13f);
        camGO.AddComponent<AudioListener>();
        camGO.transform.position = new Vector3(0f, 1f, -10f);

        EnsureTag("Bird");
        EnsureTag("Pig");
        EnsureTag("Block");

        new GameObject("GameManager").AddComponent<GameManager>();
        new GameObject("LevelManager").AddComponent<LevelManager>();

        var sky = new GameObject("Sky");
        var skySR = sky.AddComponent<SpriteRenderer>();
        skySR.sprite = LoadSprite("Sprites/Environment/sky");
        skySR.sortingOrder = -10;
        sky.transform.position = new Vector3(0f, 2f, 0f);
        sky.transform.localScale = new Vector3(25f, 16f, 1f);

        var grass = new GameObject("Grass");
        var grassSR = grass.AddComponent<SpriteRenderer>();
        grassSR.sprite = LoadSprite("Sprites/Environment/grass");
        grassSR.sortingOrder = -5;
        grass.transform.position = new Vector3(0f, -2.8f, 0f);
        grass.transform.localScale = new Vector3(25f, 1.2f, 1f);

        // Ground za travo (sortingOrder nižji), vrh pri y=-2.0 da zapolni transparentne piksle trave
        var ground = new GameObject("Ground");
        ground.transform.position = new Vector3(0f, -4.0f, 0f);
        var groundSR = ground.AddComponent<SpriteRenderer>();
        groundSR.sprite = LoadSprite("Sprites/Environment/ground");
        groundSR.sortingOrder = -6;
        ground.transform.localScale = new Vector3(25f, 4f, 1f);
        // Ločen neviden physics collider točno na površini trave
        var groundPhysics = new GameObject("GroundCollider");
        groundPhysics.transform.position = new Vector3(0f, -3.1f, 0f);
        groundPhysics.layer = LayerMask.NameToLayer("Default");
        var groundCol = groundPhysics.AddComponent<BoxCollider2D>();
        groundCol.size = new Vector2(50f, 0.5f);
        var groundRb = groundPhysics.AddComponent<Rigidbody2D>();
        groundRb.bodyType = RigidbodyType2D.Static;

        // Nevidni zidovi levo in desno
        CreateWall("WallLeft",  new Vector3(-22f, 0f, 0f), new Vector2(1f, 30f));
        CreateWall("WallRight", new Vector3( 22f, 0f, 0f), new Vector2(1f, 30f));

        // ScreenBounds script na GameManagerju
        var gmGO = GameObject.Find("GameManager");
        if (gmGO != null) gmGO.AddComponent<ScreenBounds>();

        var slingshot = new GameObject("Slingshot");
        slingshot.transform.position = new Vector3(-4f, -1.5f, 0f);
        var slingshotSR = slingshot.AddComponent<SpriteRenderer>();
        slingshotSR.sprite = LoadSprite("Sprites/UI/sling_touch");
        slingshotSR.sortingOrder = 3;

        var anchorL = new GameObject("AnchorLeft");
        anchorL.transform.SetParent(slingshot.transform);
        anchorL.transform.localPosition = new Vector3(-0.3f, 0.5f, 0f);

        var anchorR = new GameObject("AnchorRight");
        anchorR.transform.SetParent(slingshot.transform);
        anchorR.transform.localPosition = new Vector3(0.3f, 0.5f, 0f);

        var leftBand = anchorL.AddComponent<LineRenderer>();
        SetupBandRenderer(leftBand);

        var rightBand = anchorR.AddComponent<LineRenderer>();
        SetupBandRenderer(rightBand);

        var birdQueue = slingshot.AddComponent<BirdQueue>();
        var sc = slingshot.AddComponent<SlingshotController>();
        sc.anchorLeft  = anchorL.transform;
        sc.anchorRight = anchorR.transform;
        sc.leftBand    = leftBand;
        sc.rightBand   = rightBand;
        sc.birdQueue   = birdQueue;
        sc.mainCamera  = cam;
        birdQueue.slingshotSpawnPoint = slingshot.transform;
        birdQueue.birdSequence.Add(QueuedBirdType.Red);
        birdQueue.birdSequence.Add(QueuedBirdType.Blue);
        birdQueue.birdSequence.Add(QueuedBirdType.Orange);
        birdQueue.birdSequence.Add(QueuedBirdType.Red);
        birdQueue.redBirdSprite = LoadSprite("Sprites/Birds/red_bird");
        birdQueue.blueBirdSprite = LoadSprite("Sprites/Birds/blue_bird");
        birdQueue.orangeBirdSprite = LoadSprite("Sprites/Birds/yellow_bird");
        birdQueue.birdLaunchSfx = LoadAudioClip("Sounds/launch");
        birdQueue.birdScale = 1.4f;
        birdQueue.queueRowSpacing = 0.9f;
        birdQueue.queueScreenPadding = 0.6f;
        birdQueue.queueBackgroundColor = new Color(0f, 0f, 0f, 0.65f);
        birdQueue.queueBackgroundPadding = new Vector2(0.7f, 0.45f);
        slingshot.AddComponent<TrajectoryPreview>();

        var bird = new GameObject("Bird_Red");
        bird.transform.position = new Vector3(-4f, -1f, 0f);
        var birdSR = bird.AddComponent<SpriteRenderer>();
        birdSR.sprite = LoadSprite("Sprites/Birds/red_bird");
        birdSR.sortingOrder = 5;
        bird.AddComponent<CircleCollider2D>();
        var birdRb = bird.AddComponent<Rigidbody2D>();
        birdRb.bodyType = RigidbodyType2D.Kinematic;
        birdRb.gravityScale = 1.5f;
        var birdController = bird.AddComponent<BirdController>();
        birdController.hasSpecialAbility = true;
        birdController.energyBurstRadius = 2.2f;
        birdController.energyBurstImpulse = 2.8f;
        birdController.energyBurstLift = 0.35f;
        birdController.energyBurstVisualDuration = 0.35f;
        birdController.energyBurstRingWidth = 0.08f;
        birdController.energyBurstColor = new Color(1f, 0.25f, 0.05f, 0.85f);
        bird.tag = "Bird";

        bird.SetActive(false);
        birdQueue.testBird = null;

        var pig = new GameObject("Pig_Test");
        pig.transform.position = new Vector3(3f, -2.7f, 0f);
        var pigSR = pig.AddComponent<SpriteRenderer>();
        pigSR.sprite = LoadSprite("Sprites/Pigs/pig_small");
        pigSR.sortingOrder = 5;
        pig.AddComponent<CircleCollider2D>();
        var pigRb = pig.AddComponent<Rigidbody2D>();
        pigRb.bodyType = RigidbodyType2D.Dynamic;
        pig.AddComponent<PigController>();
        pig.tag = "Pig";

        var block = new GameObject("Block_Wood");
        block.transform.position = new Vector3(2.5f, -2.7f, 0f);
        var blockSR = block.AddComponent<SpriteRenderer>();
        blockSR.sprite = LoadSprite("Sprites/Blocks/wood_block");
        blockSR.sortingOrder = 4;
        block.AddComponent<BoxCollider2D>();
        var blockRb = block.AddComponent<Rigidbody2D>();
        blockRb.bodyType = RigidbodyType2D.Dynamic;
        block.AddComponent<BlockDamage>();
        block.tag = "Block";

        BuildUI();

        RenderSettings.ambientLight = new Color(0.85f, 0.85f, 0.85f);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("[GameSceneBuilder] GameScene postavljena!");
        EditorUtility.DisplayDialog("Končano!", "Shrani sceno: File → Save As → Assets/Scenes/GameScene.unity\nPotem dodaj jo v Build Settings (File → Build Settings → Add Open Scenes).", "OK");
    }

    private static void BuildUI()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        var canvasGO = new GameObject("GameCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var gameUI = canvasGO.AddComponent<GameUI>();

        // Score text (top-left)
        gameUI.scoreText = MakeText(canvasGO, "ScoreText", "Score: 0", font, 28,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(20, -20), new Vector2(300, 50));

        // Win panel
        var winPanel = MakePanel(canvasGO, "WinPanel", new Color(0, 0.6f, 0, 0.85f));
        gameUI.winPanel = winPanel;
        MakeText(winPanel, "WinTitle", "ZMAGA!", font, 52,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(400, 80));
        gameUI.winScoreText = MakeText(winPanel, "WinScore", "Score: 0", font, 36,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(400, 60));
        var winBtn = MakeButton(winPanel, "RestartBtn", "Ponovi nivo", font, canvasGO.GetComponent<GameUI>());
        SetRectCenter(winBtn, new Vector2(0, -80), new Vector2(260, 60));
        winPanel.SetActive(false);

        // Lose panel
        var losePanel = MakePanel(canvasGO, "LosePanel", new Color(0.7f, 0, 0, 0.85f));
        gameUI.losePanel = losePanel;
        MakeText(losePanel, "LoseTitle", "PORAZ!", font, 52,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(400, 80));
        gameUI.loseScoreText = MakeText(losePanel, "LoseScore", "Score: 0", font, 36,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(400, 60));
        var loseBtn = MakeButton(losePanel, "RestartBtn", "Ponovi nivo", font, canvasGO.GetComponent<GameUI>());
        SetRectCenter(loseBtn, new Vector2(0, -80), new Vector2(260, 60));
        losePanel.SetActive(false);
    }

    private static GameObject MakePanel(GameObject parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }

    private static Text MakeText(GameObject parent, string name, string content, Font font,
        int size, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var txt = go.AddComponent<Text>();
        txt.text = content;
        txt.font = font;
        txt.fontSize = size;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return txt;
    }

    private static GameObject MakeButton(GameObject parent, string name, string label,
        Font font, GameUI uiRef)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(1f, 0.8f, 0f);
        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(uiRef.RestartLevel);

        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(go.transform, false);
        var txt = txtGO.AddComponent<Text>();
        txt.text = label;
        txt.font = font;
        txt.fontSize = 28;
        txt.color = Color.black;
        txt.alignment = TextAnchor.MiddleCenter;
        var trt = txtGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        return go;
    }

    private static void SetRectCenter(GameObject go, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
    }

    private static void CreateWall(string name, Vector3 position, Vector2 size)
    {
        var wall = new GameObject(name);
        wall.transform.position = position;
        var col = wall.AddComponent<BoxCollider2D>();
        col.size = size;
        var rb = wall.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
    }

    private static void EnsureTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        for (int i = 0; i < tagsProp.arraySize; i++)
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;
        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/{path}.png");
    }

    private static AudioClip LoadAudioClip(string path)
    {
        return AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/{path}.wav");
    }

    private static void SetupBandRenderer(LineRenderer lr)
    {
        lr.positionCount = 2;
        lr.startWidth    = 0.05f;
        lr.endWidth      = 0.05f;
        lr.startColor    = new Color(0.4f, 0.25f, 0.1f);
        lr.endColor      = new Color(0.4f, 0.25f, 0.1f);
        lr.useWorldSpace = true;
        lr.enabled       = false;
    }
}
