// ============================================================
// BalloonPopperSetup.cs  –  EDITOR ONLY
// ============================================================
// This script adds a "Balloon Popper" menu to the Unity
// menu bar. Clicking the menu items automatically builds
// the complete Main Menu scene and Game Scene from scratch,
// including:
//   • Camera, lighting, skybox colour
//   • All URP materials (balloon colours)
//   • Balloon prefab with correct components
//   • Pop particle-effect prefab
//   • Canvas with all UI elements (TextMeshPro)
//   • GameManager, UIManager, BalloonSpawner GameObjects
//   • Both scenes saved and added to Build Settings
//
// HOW TO USE (see Phase 5 instructions):
//   In Unity menu bar → "Balloon Popper" → "Build Complete Game"
// ============================================================

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using UnityEngine.SceneManagement;

public static class BalloonPopperSetup
{
    // ── Colour palette ──────────────────────────────────────────────────────
    private static readonly Color[] BalloonColors = new Color[]
    {
        new Color(0.95f, 0.20f, 0.20f),  // Red
        new Color(0.20f, 0.50f, 0.95f),  // Blue
        new Color(0.98f, 0.85f, 0.10f),  // Yellow
        new Color(0.15f, 0.80f, 0.30f),  // Green
        new Color(0.70f, 0.20f, 0.95f),  // Purple
    };
    private static readonly string[] ColorNames = { "Red", "Blue", "Yellow", "Green", "Purple" };

    // ── Material GUIDs cache ────────────────────────────────────────────────
    private static Material[] _balloonMaterials;
    private static Material   _popParticleMaterial;
    private static GameObject _balloonPrefab;
    private static GameObject _popEffectPrefab;

    // ════════════════════════════════════════════════════════════════════════
    // MENU ENTRY POINT
    // ════════════════════════════════════════════════════════════════════════
    [MenuItem("Balloon Popper/Build Complete Game %#b")]   // Ctrl+Shift+B
    public static void BuildCompleteGame()
    {
        try
        {
            EditorUtility.DisplayProgressBar("Building Balloon Popper", "Creating materials…", 0.05f);
            CreateMaterials();

            EditorUtility.DisplayProgressBar("Building Balloon Popper", "Creating balloon prefab…", 0.15f);
            CreateBalloonPrefab();

            EditorUtility.DisplayProgressBar("Building Balloon Popper", "Creating pop-effect prefab…", 0.25f);
            CreatePopEffectPrefab();

            EditorUtility.DisplayProgressBar("Building Balloon Popper", "Building Main Menu scene…", 0.40f);
            BuildMainMenuScene();

            EditorUtility.DisplayProgressBar("Building Balloon Popper", "Building Game scene…", 0.65f);
            BuildGameScene();

            EditorUtility.DisplayProgressBar("Building Balloon Popper", "Configuring Build Settings…", 0.90f);
            ConfigureBuildSettings();

            EditorUtility.DisplayProgressBar("Building Balloon Popper", "Done!", 1.00f);

            // Open the MainMenu scene ready to play
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

            Debug.Log("✅ Balloon Popper setup complete! Press Play to start.");
            EditorUtility.DisplayDialog(
                "✅ Balloon Popper — Setup Complete",
                "Both scenes have been created and saved.\n\n" +
                "• Press PLAY to start from the Main Menu.\n" +
                "• The scenes are already in Build Settings.",
                "Great!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("BalloonPopperSetup failed: " + e);
            EditorUtility.DisplayDialog("Setup Error", e.Message, "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // STEP 1 — MATERIALS
    // ════════════════════════════════════════════════════════════════════════
    private static void CreateMaterials()
    {
        EnsureFolder("Assets/Materials");
        _balloonMaterials = new Material[BalloonColors.Length];

        for (int i = 0; i < BalloonColors.Length; i++)
        {
            string path = $"Assets/Materials/Balloon_{ColorNames[i]}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(GetURPLitShader());
                AssetDatabase.CreateAsset(mat, path);
            }
            // URP Lit properties
            mat.SetColor("_BaseColor", BalloonColors[i]);
            mat.SetFloat("_Smoothness", 0.85f);
            mat.SetFloat("_Metallic",   0.0f);
            // Slight emission glow so balloons look shiny
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", BalloonColors[i] * 0.15f);
            EditorUtility.SetDirty(mat);
            _balloonMaterials[i] = mat;
        }

        // Material for pop particles
        string popMatPath = "Assets/Materials/PopParticle.mat";
        _popParticleMaterial = AssetDatabase.LoadAssetAtPath<Material>(popMatPath);
        if (_popParticleMaterial == null)
        {
            // Use URP Particles/Unlit shader for particles
            Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (particleShader == null) particleShader = GetURPLitShader();
            _popParticleMaterial = new Material(particleShader);
            _popParticleMaterial.SetColor("_BaseColor", Color.white);
            AssetDatabase.CreateAsset(_popParticleMaterial, popMatPath);
        }
        EditorUtility.SetDirty(_popParticleMaterial);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // ════════════════════════════════════════════════════════════════════════
    // STEP 2 — BALLOON PREFAB
    // ════════════════════════════════════════════════════════════════════════
    private static void CreateBalloonPrefab()
    {
        EnsureFolder("Assets/Prefabs");
        string prefabPath = "Assets/Prefabs/Balloon.prefab";

        // Create balloon body
        GameObject balloonGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        balloonGO.name = "Balloon";
        balloonGO.tag  = "Balloon";   // Needed so GameManager can find all balloons at end

        // Scale to look like a balloon (taller than wide)
        balloonGO.transform.localScale = new Vector3(1f, 1.3f, 1f);

        // Replace default material with our first balloon material
        Renderer rend = balloonGO.GetComponent<Renderer>();
        if (rend != null && _balloonMaterials != null && _balloonMaterials.Length > 0)
            rend.sharedMaterial = _balloonMaterials[0];

        // Ensure SphereCollider exists (CreatePrimitive adds it automatically)
        SphereCollider col = balloonGO.GetComponent<SphereCollider>();
        if (col == null) col = balloonGO.AddComponent<SphereCollider>();

        // Add BalloonController script
        BalloonController bc = balloonGO.AddComponent<BalloonController>();
        bc.floatSpeed       = 2f;
        bc.destroyYPosition = 12f;

        // Add a string as the balloon's "rope" (small cylinder)
        GameObject rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rope.name = "Rope";
        rope.transform.SetParent(balloonGO.transform);
        rope.transform.localPosition = new Vector3(0f, -0.9f, 0f);
        rope.transform.localScale    = new Vector3(0.04f, 0.4f, 0.04f);
        // Disable rope collider
        Collider ropeCol = rope.GetComponent<Collider>();
        if (ropeCol != null) ropeCol.enabled = false;
        // Dark grey material for rope
        Material ropeMat = new Material(GetURPLitShader());
        ropeMat.SetColor("_BaseColor", new Color(0.2f, 0.2f, 0.2f));
        rope.GetComponent<Renderer>().sharedMaterial = ropeMat;

        // Save prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(balloonGO, prefabPath);
        _balloonPrefab = prefab;

        // Clean up temporary scene object
        Object.DestroyImmediate(balloonGO);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // ════════════════════════════════════════════════════════════════════════
    // STEP 3 — POP EFFECT PREFAB
    // ════════════════════════════════════════════════════════════════════════
    private static void CreatePopEffectPrefab()
    {
        EnsureFolder("Assets/Particles");
        string prefabPath = "Assets/Particles/PopEffect.prefab";

        GameObject effectGO = new GameObject("PopEffect");
        ParticleSystem ps   = effectGO.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(3f,  8f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
        main.startColor      = new ParticleSystem.MinMaxGradient(Color.red, Color.yellow);
        main.maxParticles    = 30;
        main.duration        = 0.3f;
        main.loop            = false;
        main.playOnAwake     = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.3f;

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 20, 25)
        });
        emission.rateOverTime = 0;

        var shape = ps.shape;
        shape.enabled     = true;
        shape.shapeType   = ParticleSystemShapeType.Sphere;
        shape.radius      = 0.3f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white,  0.0f),
                new GradientColorKey(Color.yellow, 0.5f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0.0f),
                new GradientAlphaKey(0f, 1.0f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        // Assign particle material
        ParticleSystemRenderer psr = effectGO.GetComponent<ParticleSystemRenderer>();
        if (_popParticleMaterial != null)
            psr.sharedMaterial = _popParticleMaterial;

        // Save prefab
        _popEffectPrefab = PrefabUtility.SaveAsPrefabAsset(effectGO, prefabPath);
        Object.DestroyImmediate(effectGO);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // ════════════════════════════════════════════════════════════════════════
    // STEP 4 — MAIN MENU SCENE
    // ════════════════════════════════════════════════════════════════════════
    private static void BuildMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenu";

        // ── Camera ───────────────────────────────────────────────────────────
        GameObject camGO = new GameObject("Main Camera");
        Camera cam = camGO.AddComponent<Camera>();
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.53f, 0.81f, 0.98f); // sky blue
        cam.orthographic     = false;
        cam.fieldOfView      = 60f;
        camGO.transform.position = new Vector3(0, 0, -10);
        camGO.tag = "MainCamera";

        // URP camera data
        var urpCamData = camGO.AddComponent<UniversalAdditionalCameraData>();
        urpCamData.renderShadows = false;

        // ── Directional Light ────────────────────────────────────────────────
        GameObject lightGO = new GameObject("Directional Light");
        Light light = lightGO.AddComponent<Light>();
        light.type      = LightType.Directional;
        light.intensity = 1.2f;
        light.color     = new Color(1f, 0.95f, 0.85f);
        lightGO.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

        // ── Background gradient quad ─────────────────────────────────────────
        CreateColoredBackground(new Color(0.40f, 0.72f, 0.95f), new Color(0.90f, 0.95f, 1.00f));

        // ── Decorative balloons ──────────────────────────────────────────────
        SpawnDecorativeBalloons();

        // ── UI Canvas ────────────────────────────────────────────────────────
        BuildMainMenuUI();

        // ── MainMenuManager ──────────────────────────────────────────────────
        // (Button wiring is done inside BuildMainMenuUI)

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        AssetDatabase.Refresh();
    }

    private static void CreateColoredBackground(Color topColor, Color bottomColor)
    {
        // Simple large quad as a background
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "Background";
        bg.transform.position   = new Vector3(0, 0, 5);
        bg.transform.localScale = new Vector3(30, 20, 1);
        bg.GetComponent<Collider>().enabled = false;

        Material bgMat = new Material(GetURPLitShader());
        bgMat.SetColor("_BaseColor", new Color(0.53f, 0.81f, 0.98f));
        bg.GetComponent<Renderer>().sharedMaterial = bgMat;
    }

    private static void SpawnDecorativeBalloons()
    {
        if (_balloonPrefab == null) return;

        float[] xPositions = { -7f, -4f, -1f, 2f, 5f, 7f };
        for (int i = 0; i < xPositions.Length; i++)
        {
            float startY = Random.Range(-10f, 2f);
            Vector3 pos  = new Vector3(xPositions[i], startY, 2f);
            GameObject balloon = PrefabUtility.InstantiatePrefab(_balloonPrefab) as GameObject;
            balloon.name = "MenuBalloon_" + i;
            balloon.transform.position = pos;
            balloon.transform.localScale *= Random.Range(0.8f, 1.4f);

            // Assign random material
            Renderer rend = balloon.GetComponent<Renderer>();
            if (rend != null && _balloonMaterials != null && _balloonMaterials.Length > 0)
                rend.sharedMaterial = _balloonMaterials[i % _balloonMaterials.Length];

            // Remove BalloonController, add MenuBalloon
            Object.DestroyImmediate(balloon.GetComponent<BalloonController>());
            MenuBalloon mb = balloon.AddComponent<MenuBalloon>();
            mb.floatSpeed  = Random.Range(1f, 2.5f);

            // Disable collider (not clickable on menu)
            Collider col = balloon.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
    }

    private static void BuildMainMenuUI()
    {
        // ── Canvas ────────────────────────────────────────────────────────────
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Event system
        GameObject evtSys = new GameObject("EventSystem");
        evtSys.AddComponent<UnityEngine.EventSystems.EventSystem>();
        evtSys.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // ── Background panel ─────────────────────────────────────────────────
        // We'll use the camera background colour so no extra panel needed.

        // ── Title ─────────────────────────────────────────────────────────────
        GameObject titleGO = CreateTMProText(canvasGO.transform, "Title",
            "BALLOON POPPER",
            new Vector2(0, 180),
            new Vector2(1000, 150));
        TextMeshProUGUI titleTMP = titleGO.GetComponent<TextMeshProUGUI>();
        titleTMP.fontSize       = 90;
        titleTMP.fontStyle      = FontStyles.Bold;
        titleTMP.color          = new Color(0.98f, 0.95f, 0.10f); // golden
        titleTMP.alignment      = TextAlignmentOptions.Center;
        AddOutlineEffect(titleTMP, new Color(0.7f, 0.1f, 0.1f), 0.4f);

        // ── Instructions panel ────────────────────────────────────────────────
        GameObject instrPanel = CreatePanel(canvasGO.transform, "InstructionsPanel",
            new Vector2(0, -30),
            new Vector2(680, 220),
            new Color(0.1f, 0.1f, 0.4f, 0.75f));

        GameObject instrText = CreateTMProText(instrPanel.transform, "InstructionsText",
            "HOW TO PLAY\n\n" +
            "- Balloons float up from the bottom\n" +
            "- Click a balloon to POP it and score a point\n" +
            "- You have 60 seconds - pop as many as you can!\n" +
            "- Balloons that reach the top disappear",
            Vector2.zero,
            new Vector2(640, 200));
        TextMeshProUGUI instrTMP = instrText.GetComponent<TextMeshProUGUI>();
        instrTMP.fontSize  = 26;
        instrTMP.color     = Color.white;
        instrTMP.alignment = TextAlignmentOptions.Center;

        // ── Start Game button ─────────────────────────────────────────────────
        GameObject startBtn = CreateButton(canvasGO.transform, "StartGameButton",
            "START GAME",
            new Vector2(0, -240),
            new Vector2(340, 80),
            new Color(0.15f, 0.80f, 0.30f),
            60);

        // ── Quit button ───────────────────────────────────────────────────────
        GameObject quitBtn = CreateButton(canvasGO.transform, "QuitButton",
            "QUIT",
            new Vector2(0, -340),
            new Vector2(240, 65),
            new Color(0.80f, 0.15f, 0.15f),
            48);

        // ── MainMenuManager ───────────────────────────────────────────────────
        GameObject mgrGO = new GameObject("MainMenuManager");
        MainMenuManager mgr = mgrGO.AddComponent<MainMenuManager>();
        mgr.startGameButton = startBtn.GetComponent<Button>();
        mgr.quitButton      = quitBtn.GetComponent<Button>();
    }

    // ════════════════════════════════════════════════════════════════════════
    // STEP 5 — GAME SCENE
    // ════════════════════════════════════════════════════════════════════════
    private static void BuildGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "GameScene";

        // ── Camera ────────────────────────────────────────────────────────────
        GameObject camGO = new GameObject("Main Camera");
        Camera cam = camGO.AddComponent<Camera>();
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.53f, 0.81f, 0.98f);
        cam.orthographic    = false;
        cam.fieldOfView     = 60f;
        camGO.transform.position = new Vector3(0, 2, -15);
        camGO.tag = "MainCamera";

        var urpCamData = camGO.AddComponent<UniversalAdditionalCameraData>();
        urpCamData.renderShadows = true;

        // AudioListener lives on the camera
        if (camGO.GetComponent<AudioListener>() == null)
            camGO.AddComponent<AudioListener>();

        // ── Directional Light ─────────────────────────────────────────────────
        GameObject lightGO = new GameObject("Directional Light");
        Light light = lightGO.AddComponent<Light>();
        light.type        = LightType.Directional;
        light.intensity   = 1.1f;
        light.color       = new Color(1f, 0.95f, 0.85f);
        light.shadows     = LightShadows.Soft;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // ── Background quad ───────────────────────────────────────────────────
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "Background";
        bg.transform.position   = new Vector3(0, 2, 10);
        bg.transform.localScale = new Vector3(40, 28, 1);
        bg.GetComponent<Collider>().enabled = false;
        Material bgMat = new Material(GetURPLitShader());
        bgMat.SetColor("_BaseColor", new Color(0.53f, 0.81f, 0.98f));
        bg.GetComponent<Renderer>().sharedMaterial = bgMat;

        // ── Ground plane ──────────────────────────────────────────────────────
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position   = new Vector3(0, -9f, 0);
        ground.transform.localScale = new Vector3(3, 1, 3);
        Material groundMat = new Material(GetURPLitShader());
        groundMat.SetColor("_BaseColor", new Color(0.4f, 0.75f, 0.3f));
        ground.GetComponent<Renderer>().sharedMaterial = groundMat;

        // ── GameManager ────────────────────────────────────────────────────────
        GameObject gmGO = new GameObject("GameManager");
        GameManager gm = gmGO.AddComponent<GameManager>();
        gm.gameDuration = 60f;

        // ── BalloonSpawner ─────────────────────────────────────────────────────
        GameObject spawnerGO = new GameObject("BalloonSpawner");
        BalloonSpawner spawner = spawnerGO.AddComponent<BalloonSpawner>();
        spawner.balloonPrefab    = _balloonPrefab;
        spawner.balloonMaterials  = _balloonMaterials;
        spawner.popEffectPrefab  = _popEffectPrefab;
        spawner.spawnInterval    = 1.2f;
        spawner.spawnRangeX      = 7f;
        spawner.spawnY           = -9f;
        spawner.minSpeed         = 1.5f;
        spawner.maxSpeed         = 4.5f;

        // ── UI Canvas ─────────────────────────────────────────────────────────
        UIManager uiMgr = BuildGameUI(gmGO, out GameObject gameOverPanel,
            out TextMeshProUGUI scoreTMP, out TextMeshProUGUI timerTMP,
            out TextMeshProUGUI finalScoreTMP,
            out Button restartBtn, out Button mainMenuBtn);

        // ── Wire UIManager references ─────────────────────────────────────────
        uiMgr.scoreText      = scoreTMP;
        uiMgr.timerText      = timerTMP;
        uiMgr.gameOverPanel  = gameOverPanel;
        uiMgr.finalScoreText = finalScoreTMP;
        uiMgr.restartButton  = restartBtn;
        uiMgr.mainMenuButton = mainMenuBtn;

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
        AssetDatabase.Refresh();
    }

    private static UIManager BuildGameUI(
        GameObject gmGO,
        out GameObject gameOverPanel,
        out TextMeshProUGUI scoreTMP,
        out TextMeshProUGUI timerTMP,
        out TextMeshProUGUI finalScoreTMP,
        out Button restartBtn,
        out Button mainMenuBtn)
    {
        // Canvas
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem
        GameObject evtSys = new GameObject("EventSystem");
        evtSys.AddComponent<UnityEngine.EventSystems.EventSystem>();
        evtSys.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // ── HUD bar at top ────────────────────────────────────────────────────
        GameObject hudPanel = CreatePanel(canvasGO.transform, "HUDPanel",
            new Vector2(0, 490),
            new Vector2(1920, 80),
            new Color(0f, 0f, 0f, 0.45f));

        RectTransform hudRT = hudPanel.GetComponent<RectTransform>();
        hudRT.anchorMin = new Vector2(0, 1);
        hudRT.anchorMax = new Vector2(1, 1);
        hudRT.pivot     = new Vector2(0.5f, 1f);
        hudRT.offsetMin = Vector2.zero;
        hudRT.offsetMax = Vector2.zero;
        hudRT.sizeDelta = new Vector2(0, 80);
        hudRT.anchoredPosition = Vector2.zero;

        // Score text (left side)
        GameObject scoreGO = CreateTMProText(hudPanel.transform, "ScoreText",
            "Score: 0",
            new Vector2(-700, 0),
            new Vector2(400, 60));
        scoreTMP = scoreGO.GetComponent<TextMeshProUGUI>();
        scoreTMP.fontSize  = 42;
        scoreTMP.fontStyle = FontStyles.Bold;
        scoreTMP.color     = Color.white;
        scoreTMP.alignment = TextAlignmentOptions.Left;

        // Timer text (right side)
        GameObject timerGO = CreateTMProText(hudPanel.transform, "TimerText",
            "Time: 60",
            new Vector2(700, 0),
            new Vector2(300, 60));
        timerTMP = timerGO.GetComponent<TextMeshProUGUI>();
        timerTMP.fontSize  = 42;
        timerTMP.fontStyle = FontStyles.Bold;
        timerTMP.color     = Color.white;
        timerTMP.alignment = TextAlignmentOptions.Right;

        // ── Game Over Panel ───────────────────────────────────────────────────
        gameOverPanel = CreatePanel(canvasGO.transform, "GameOverPanel",
            Vector2.zero,
            new Vector2(700, 500),
            new Color(0.05f, 0.05f, 0.20f, 0.95f));

        // "GAME OVER" header
        GameObject goHeader = CreateTMProText(gameOverPanel.transform, "GameOverTitle",
            "GAME OVER",
            new Vector2(0, 160),
            new Vector2(650, 100));
        TextMeshProUGUI goHeaderTMP = goHeader.GetComponent<TextMeshProUGUI>();
        goHeaderTMP.fontSize  = 64;
        goHeaderTMP.fontStyle = FontStyles.Bold;
        goHeaderTMP.color     = new Color(0.98f, 0.85f, 0.10f);
        goHeaderTMP.alignment = TextAlignmentOptions.Center;

        // Final score
        GameObject finalScoreGO = CreateTMProText(gameOverPanel.transform, "FinalScoreText",
            "Final Score: 0",
            new Vector2(0, 50),
            new Vector2(600, 80));
        finalScoreTMP = finalScoreGO.GetComponent<TextMeshProUGUI>();
        finalScoreTMP.fontSize  = 52;
        finalScoreTMP.fontStyle = FontStyles.Bold;
        finalScoreTMP.color     = Color.white;
        finalScoreTMP.alignment = TextAlignmentOptions.Center;

        // Restart button
        GameObject restartGO = CreateButton(gameOverPanel.transform, "RestartButton",
            "RESTART",
            new Vector2(0, -70),
            new Vector2(320, 70),
            new Color(0.15f, 0.75f, 0.25f),
            48);
        restartBtn = restartGO.GetComponent<Button>();

        // Main Menu button
        GameObject mainMenuGO = CreateButton(gameOverPanel.transform, "MainMenuButton",
            "MAIN MENU",
            new Vector2(0, -160),
            new Vector2(320, 70),
            new Color(0.20f, 0.40f, 0.80f),
            44);
        mainMenuBtn = mainMenuGO.GetComponent<Button>();

        gameOverPanel.SetActive(false);  // Hidden at start

        // ── UIManager component ───────────────────────────────────────────────
        GameObject uiMgrGO = new GameObject("UIManager");
        UIManager uiMgr = uiMgrGO.AddComponent<UIManager>();

        return uiMgr;
    }

    // ════════════════════════════════════════════════════════════════════════
    // STEP 6 — BUILD SETTINGS
    // ════════════════════════════════════════════════════════════════════════
    private static void ConfigureBuildSettings()
    {
        var scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity",  true),
            new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true),
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("Build Settings updated: MainMenu (index 0), GameScene (index 1)");
    }

    // ════════════════════════════════════════════════════════════════════════
    // UI HELPER METHODS
    // ════════════════════════════════════════════════════════════════════════

    private static GameObject CreatePanel(Transform parent, string name,
        Vector2 anchoredPos, Vector2 sizeDelta, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = sizeDelta;
        return go;
    }

    private static GameObject CreateTMProText(Transform parent, string name,
        string text, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 36;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = sizeDelta;
        return go;
    }

    private static GameObject CreateButton(Transform parent, string name,
        string label, Vector2 anchoredPos, Vector2 sizeDelta, Color bgColor, int fontSize)
    {
        // Container
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        Image img = btnGO.AddComponent<Image>();
        img.color = bgColor;

        // Add a rounded look via sprite (using default Unity sprite is fine for now)
        Button btn = btnGO.AddComponent<Button>();

        // Colour tint on hover/press
        ColorBlock cb = btn.colors;
        cb.normalColor      = bgColor;
        cb.highlightedColor = bgColor * 1.3f;
        cb.pressedColor     = bgColor * 0.7f;
        cb.selectedColor    = bgColor;
        btn.colors = cb;

        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = sizeDelta;

        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(btnGO.transform, false);
        TextMeshProUGUI tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin       = Vector2.zero;
        labelRT.anchorMax       = Vector2.one;
        labelRT.offsetMin       = Vector2.zero;
        labelRT.offsetMax       = Vector2.zero;

        return btnGO;
    }

    private static void AddOutlineEffect(TextMeshProUGUI tmp, Color outlineColor, float thickness)
    {
        tmp.outlineColor = outlineColor;
        tmp.outlineWidth = thickness;
        tmp.fontMaterial.EnableKeyword("OUTLINE_ON");
    }

    // ════════════════════════════════════════════════════════════════════════
    // UTILITY
    // ════════════════════════════════════════════════════════════════════════
    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    private static Shader GetURPLitShader()
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null)
        {
            Debug.LogWarning("URP Lit shader not found — falling back to Standard.");
            s = Shader.Find("Standard");
        }
        return s;
    }
}
#endif
