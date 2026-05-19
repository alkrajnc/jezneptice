using UnityEngine;
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

        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var go in allObjects)
            if (go.transform.parent == null)
                Object.DestroyImmediate(go);

        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.13f, 0.13f, 0.13f);
        camGO.AddComponent<AudioListener>();
        camGO.transform.position = new Vector3(0f, 1f, -10f);

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
        grass.transform.position = new Vector3(0f, -2.5f, 0f);
        grass.transform.localScale = new Vector3(25f, 1.5f, 1f);

        var ground = new GameObject("Ground");
        ground.transform.position = new Vector3(0f, -3.2f, 0f);
        var groundSR = ground.AddComponent<SpriteRenderer>();
        groundSR.sprite = LoadSprite("Sprites/Environment/ground");
        groundSR.sortingOrder = -4;
        ground.transform.localScale = new Vector3(25f, 1f, 1f);
        var groundCol = ground.AddComponent<BoxCollider2D>();
        groundCol.size = Vector2.one;
        var groundRb = ground.AddComponent<Rigidbody2D>();
        groundRb.bodyType = RigidbodyType2D.Static;

        var slingshot = new GameObject("Slingshot");
        slingshot.transform.position = new Vector3(-4f, -1.5f, 0f);

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
        bird.AddComponent<BirdController>();
        bird.tag = "Bird";

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

        RenderSettings.ambientLight = new Color(0.85f, 0.85f, 0.85f);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("[GameSceneBuilder] GameScene postavljena!");
        EditorUtility.DisplayDialog("Končano!", "Shrani: File → Save As → Assets/Scenes/GameScene.unity", "OK");
    }

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/{path}.png");
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
