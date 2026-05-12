using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class AutoLevelBuilder : MonoBehaviour
{
    private const string RootName = "_AutoLevelRoot";
    private MinecartRideController minecartRide;
    private GameObject codeUiPanel;
    private GameObject mazeUiPanel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (GameObject.Find(RootName) != null)
        {
            return;
        }

        GameObject bootstrap = new GameObject(RootName);
        bootstrap.AddComponent<AutoLevelBuilder>();
    }

    private void Start()
    {
        BuildScene();
    }

    private void BuildScene()
    {
        Time.timeScale = 0f;
        Physics2D.gravity = new Vector2(0f, -20f);

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cam = new GameObject("Main Camera");
            mainCamera = cam.AddComponent<Camera>();
            cam.tag = "MainCamera";
        }

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 6f;
        mainCamera.transform.position = new Vector3(0f, 2f, -10f);
        mainCamera.backgroundColor = new Color(0.07f, 0.07f, 0.09f);

        GameObject systems = new GameObject("LevelSystems");
        systems.transform.SetParent(transform);
        LevelRespawnManager respawnManager = systems.AddComponent<LevelRespawnManager>();

        GameObject player = CreatePlayer();
        FollowCamera followCamera = mainCamera.gameObject.AddComponent<FollowCamera>();
        followCamera.Configure(player.transform);

        BuildGroundAndPlatforms();
        BuildCodeSection(player);
        BuildMinecartSection(respawnManager);
        BuildMazeSection(respawnManager, player.transform);
        BuildFinalSection(player.transform);
        BuildKillZone(respawnManager);
        BuildUi(respawnManager, player.GetComponent<PlayerController2D>());
        BuildMusicObject();

        LevelUiVisibilityController uiVisibility = systems.AddComponent<LevelUiVisibilityController>();
        uiVisibility.Configure(player.transform, codeUiPanel, mazeUiPanel);
    }

    private GameObject CreatePlayer()
    {
        GameObject player = new GameObject("Player", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(BoxCollider2D));
        player.transform.SetParent(transform);
        player.transform.position = new Vector3(-10f, 1.5f, 0f);
        player.transform.localScale = Vector3.one;
        player.tag = "Player";
        SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
        renderer.sprite = SpriteFactory.Pixel;
        renderer.color = new Color(0.9f, 0.9f, 1f);
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = new Vector2(0.8f, 2f);

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rb.gravityScale = 4f;

        CapsuleCollider2D capsule = player.GetComponent<CapsuleCollider2D>();
        capsule.isTrigger = false;
        capsule.size = new Vector2(0.8f, 2f);
        capsule.offset = Vector2.zero;

        BoxCollider2D feet = player.GetComponent<BoxCollider2D>();
        feet.isTrigger = false;
        feet.size = new Vector2(0.7f, 0.2f);
        feet.offset = new Vector2(0f, -0.9f);

        Transform groundCheck = new GameObject("GroundCheck").transform;
        groundCheck.SetParent(player.transform);
        groundCheck.localPosition = new Vector3(0f, -0.95f, 0f);

        PlayerController2D controller = player.AddComponent<PlayerController2D>();
        controller.ConfigureGroundCheck(groundCheck, ~0, ~0);
        return player;
    }

    private void BuildGroundAndPlatforms()
    {
        CreatePlatform("Ground", new Vector2(25f, -1f), new Vector2(100f, 1f));
        CreatePlatform("StepA", new Vector2(4f, 1.65f), new Vector2(3f, 0.5f));
        CreatePlatform("StepB", new Vector2(9f, 2.55f), new Vector2(3f, 0.5f));
        CreatePlatform("StepC", new Vector2(14f, 4.05f), new Vector2(3f, 0.5f));
        CreatePlatform("StepD", new Vector2(17.8f, 2.85f), new Vector2(2.4f, 0.45f));
    }

    private void BuildCodeSection(GameObject player)
    {
        Canvas canvas = EnsureCanvas();
        GameObject panel = CreateUiPanel(canvas.transform, "CodePanel", new Vector2(20f, -20f), new Vector2(420f, 160f), new Color(0f, 0f, 0f, 0.68f));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(20f, -20f);
        codeUiPanel = panel;

        Text codeHint = CreateText(panel.transform, "CodeHint", "Собери цифры: ", new Vector2(14f, -10f), 20);
        InputField codeInput = CreateInputField(panel.transform, "CodeInput", new Vector2(14f, -60f), "Введите код");
        Text codeStatus = CreateText(panel.transform, "CodeStatus", "", new Vector2(14f, -108f), 18);
        Button submit = CreateButton(panel.transform, "SubmitCode", "OK", new Vector2(250f, -60f), new Vector2(80f, 36f));

        GameObject door = CreatePlatform("CodeDoor", new Vector2(20f, 1.2f), new Vector2(1f, 4f));
        CodePuzzleController puzzle = door.AddComponent<CodePuzzleController>();
        puzzle.Configure(codeInput, codeStatus, door);
        submit.onClick.AddListener(puzzle.TrySubmitCode);

        CreateDigit("Digit8", "8", new Vector2(2.5f, 2.2f), codeHint);
        CreateDigit("Digit3", "3", new Vector2(5.1f, 3.2f), codeHint);
        CreateDigit("Digit0", "0", new Vector2(7.7f, 2.5f), codeHint);
        CreateDigit("Digit3b", "3", new Vector2(10.3f, 3.3f), codeHint);
    }

    private void BuildMinecartSection(LevelRespawnManager respawnManager)
    {
        GameObject cart = CreateBox("Minecart", new Vector2(28f, 1f), new Vector2(1.8f, 0.75f), new Color(0.5f, 0.4f, 0.3f), false);
        Rigidbody2D cartBody = cart.GetComponent<Rigidbody2D>();
        if (cartBody != null)
        {
            cartBody.bodyType = RigidbodyType2D.Kinematic;
        }

        BoxCollider2D boardingTrigger = cart.AddComponent<BoxCollider2D>();
        boardingTrigger.isTrigger = true;
        boardingTrigger.size = new Vector2(2.2f, 1.4f);
        boardingTrigger.offset = new Vector2(0f, 0.25f);
        MinecartRideController ride = cart.AddComponent<MinecartRideController>();
        minecartRide = ride;

        Transform p1 = CreatePoint("CartP1", new Vector2(28f, 1f));
        Transform p2 = CreatePoint("CartP2", new Vector2(34f, 1f));
        Transform p3 = CreatePoint("CartP3", new Vector2(40f, 1f));
        Transform p4 = CreatePoint("CartP4", new Vector2(46f, 1f));
        ride.Configure(new[] { p1, p2, p3, p4 }, 5.2f, false);

        CreatePlatform("MinecartRailA", new Vector2(31f, 0.2f), new Vector2(8f, 0.5f));
        CreatePlatform("MinecartRailB", new Vector2(37f, 0.2f), new Vector2(8f, 0.5f));
        CreatePlatform("MinecartRailC", new Vector2(43f, 0.2f), new Vector2(8f, 0.5f));
        CreateCrouchHazard("CartCeiling1", new Vector2(33f, 2.25f), new Vector2(0.45f, 0.8f), respawnManager);
        CreateJumpHazard("CartFloor1", new Vector2(36.2f, 0.9f), new Vector2(0.35f, 0.35f), respawnManager);
        CreateCrouchHazard("CartCeiling2", new Vector2(39.5f, 2.25f), new Vector2(0.45f, 0.8f), respawnManager);
        CreateJumpHazard("CartFloor2", new Vector2(42.8f, 0.9f), new Vector2(0.35f, 0.35f), respawnManager);
    }

    private void BuildMazeSection(LevelRespawnManager respawnManager, Transform player)
    {
        Canvas canvas = EnsureCanvas();
        GameObject timerPanel = CreateUiPanel(canvas.transform, "MazeTimerPanel", new Vector2(20f, -200f), new Vector2(300f, 60f), new Color(0f, 0f, 0f, 0.68f));
        RectTransform timerRect = timerPanel.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(0f, 1f);
        timerRect.anchorMax = new Vector2(0f, 1f);
        timerRect.pivot = new Vector2(0f, 1f);
        timerRect.anchoredPosition = new Vector2(20f, -200f);
        mazeUiPanel = timerPanel;
        Text timerText = CreateText(timerPanel.transform, "MazeTimer", "Лабиринт: 60", new Vector2(14f, -8f), 20);

        GameObject mazeRoot = new GameObject("MazeSection");
        mazeRoot.transform.SetParent(transform);
        mazeRoot.transform.position = new Vector3(58f, 0f, 0f);
        CreatePlatform("MazeFloor", new Vector2(58f, -1f), new Vector2(16f, 0.8f));
        CreatePlatform("MazeWallLTop", new Vector2(50f, 5f), new Vector2(0.6f, 4f));
        CreatePlatform("MazeWallLBottom", new Vector2(50f, 0f), new Vector2(0.6f, 2.2f));
        CreatePlatform("MazeWallR", new Vector2(66f, 3f), new Vector2(0.6f, 8f));
        CreatePlatform("MazeBlock1", new Vector2(55f, 1f), new Vector2(0.8f, 4f));
        CreatePlatform("MazeBlock2", new Vector2(60f, 3f), new Vector2(0.8f, 4f));
        CreatePlatform("MazeBlock3", new Vector2(63f, 1.5f), new Vector2(0.8f, 4f));

        GameObject avalanche = CreateBox("Avalanche", new Vector2(58f, -3.5f), new Vector2(16f, 1f), new Color(0.4f, 0.4f, 0.4f), true);
        BoxCollider2D avalancheKill = avalanche.AddComponent<BoxCollider2D>();
        avalancheKill.isTrigger = true;
        KillTrigger avalancheTrigger = avalanche.AddComponent<KillTrigger>();
        avalancheTrigger.Configure(respawnManager);

        VerticalMazeController maze = mazeRoot.AddComponent<VerticalMazeController>();
        maze.Configure(timerText, avalanche, respawnManager);

        CreatePlatform("MazeEntryBridge", new Vector2(48f, -1f), new Vector2(4f, 0.45f));
        CreateWorldLabel("MazeEntryText", "Вход в лабиринт ->", new Vector2(46f, 1f), 2.2f, Color.white);
        BuildMiniMap(canvas.transform, player);
    }

    private void BuildMiniMap(Transform canvas, Transform player)
    {
        GameObject mapPanel = CreateUiPanel(canvas, "Minimap", new Vector2(-120f, -120f), new Vector2(170f, 170f), new Color(0f, 0f, 0f, 0.6f));
        RectTransform panelRect = mapPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);

        GameObject markerObj = new GameObject("Marker", typeof(RectTransform), typeof(Image));
        markerObj.transform.SetParent(mapPanel.transform, false);
        RectTransform markerRect = markerObj.GetComponent<RectTransform>();
        markerRect.sizeDelta = new Vector2(10f, 10f);
        markerObj.GetComponent<Image>().color = Color.red;

        MinimapMarker2D marker = mapPanel.AddComponent<MinimapMarker2D>();
        marker.Configure(player, markerRect, new Vector2(48f, -3f), new Vector2(90f, 8f), new Vector2(-70f, -70f), new Vector2(70f, 70f));
    }

    private void BuildFinalSection(Transform player)
    {
        Canvas canvas = EnsureCanvas();
        Text keyText = CreateText(canvas.transform, "KeyCounter", "Ключи: 0/1", new Vector2(160f, -210f), 20);
        Text completeText = CreateText(canvas.transform, "CompleteText", "Уровень пройден!", new Vector2(760f, -40f), 30);
        completeText.alignment = TextAnchor.MiddleCenter;
        completeText.gameObject.SetActive(false);

        GameObject doorVisual = CreatePlatform("FinalDoor", new Vector2(90f, 1.2f), new Vector2(1f, 4f));
        GameObject keySystem = new GameObject("KeySystem");
        keySystem.transform.SetParent(transform);
        KeyDoorController doorController = keySystem.AddComponent<KeyDoorController>();
        doorController.Configure(keyText, completeText, 1, string.Empty);

        GameObject key = CreateBox("CorrectKey", new Vector2(84f, 1.2f), new Vector2(0.6f, 0.6f), Color.yellow, true);
        CircleCollider2D keyTrigger = key.AddComponent<CircleCollider2D>();
        keyTrigger.isTrigger = true;
        KeyCollectible collectible = key.AddComponent<KeyCollectible>();
        collectible.Configure(doorController);

        GameObject exitZone = new GameObject("ExitZone");
        exitZone.transform.SetParent(transform);
        exitZone.transform.position = new Vector3(89.5f, 1f, 0f);
        BoxCollider2D exitTrigger = exitZone.AddComponent<BoxCollider2D>();
        exitTrigger.size = new Vector2(1f, 2f);
        exitTrigger.isTrigger = true;
        DoorExitTrigger exit = exitZone.AddComponent<DoorExitTrigger>();
        exit.Configure(doorController, doorVisual);
    }

    private void BuildKillZone(LevelRespawnManager respawnManager)
    {
        GameObject pit = new GameObject("BottomKillZone");
        pit.transform.SetParent(transform);
        pit.transform.position = new Vector3(40f, -8f, 0f);
        BoxCollider2D trigger = pit.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(200f, 1f);
        KillTrigger kill = pit.AddComponent<KillTrigger>();
        kill.Configure(respawnManager);
    }

    private void BuildUi(LevelRespawnManager respawnManager, PlayerController2D playerController)
    {
        Canvas canvas = EnsureCanvas();
        Button restart = CreateButton(canvas.transform, "RestartButton", "Заново", new Vector2(80f, -80f), new Vector2(120f, 40f));
        restart.onClick.AddListener(respawnManager.RestartLevel);
        GameObject staminaPanel = CreateUiPanel(canvas.transform, "StaminaPanel", new Vector2(0f, -12f), new Vector2(650f, 110f), new Color(0f, 0f, 0f, 0.9f));
        RectTransform panelRect = staminaPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0f, -12f);
        staminaPanel.transform.SetAsLastSibling();
        CreateText(staminaPanel.transform, "StaminaLabel", "Выносливость", new Vector2(14f, -8f), 24);
        Slider stamina = CreateSlider(staminaPanel.transform, "StaminaSlider", new Vector2(14f, -55f), new Vector2(500f, 34f));
        Text staminaState = CreateText(staminaPanel.transform, "StaminaState", "Бег", new Vector2(526f, -54f), 24);
        GameObject staminaUi = new GameObject("StaminaUI");
        staminaUi.transform.SetParent(transform);
        PlayerStaminaUI ui = staminaUi.AddComponent<PlayerStaminaUI>();
        ui.Configure(playerController, stamina, staminaState);

        GameObject panel = CreateUiPanel(canvas.transform, "StartPanel", Vector2.zero, new Vector2(560f, 300f), new Color(0f, 0f, 0f, 0.85f));
        Text title = CreateCenteredText(panel.transform, "Title", "Заброшенная шахта", new Vector2(0f, 70f), new Vector2(500f, 60f), 36);
        Button play = CreateCenteredButton(panel.transform, "PlayButton", "Играть", new Vector2(0f, -20f), new Vector2(240f, 60f));
        play.onClick.AddListener(() => panel.SetActive(false));
        play.onClick.AddListener(() => Time.timeScale = 1f);

        GameObject ridePanel = CreateUiPanel(canvas.transform, "RidePanel", new Vector2(0f, -120f), new Vector2(300f, 90f), new Color(0f, 0f, 0f, 0.82f));
        CreateCenteredText(ridePanel.transform, "RideText", "Сел в вагонетку", new Vector2(0f, 24f), new Vector2(260f, 30f), 22);
        Button rideButton = CreateCenteredButton(ridePanel.transform, "RideButton", "Ехать", new Vector2(0f, -18f), new Vector2(140f, 42f));
        ridePanel.SetActive(false);

        GameObject cartActionPanel = CreateUiPanel(canvas.transform, "CartActionPanel", new Vector2(0f, -230f), new Vector2(360f, 95f), new Color(0f, 0f, 0f, 0.78f));
        Button jumpButton = CreateCenteredButton(cartActionPanel.transform, "CartJumpButton", "Прыжок", new Vector2(-85f, 0f), new Vector2(140f, 48f));
        Button crouchButton = CreateCenteredButton(cartActionPanel.transform, "CartCrouchButton", "Присесть", new Vector2(85f, 0f), new Vector2(140f, 48f));
        cartActionPanel.SetActive(false);

        jumpButton.onClick.AddListener(playerController.ExternalCartJump);
        AddHoldButtonEvents(crouchButton, () => playerController.SetExternalCrouch(true), () => playerController.SetExternalCrouch(false));

        if (minecartRide != null)
        {
            minecartRide.ConfigureRideButton(ridePanel, rideButton);
            minecartRide.ConfigureActionPanel(cartActionPanel);
        }
    }

    private void BuildMusicObject()
    {
        GameObject music = new GameObject("BackgroundMusic");
        music.transform.SetParent(transform);
        AudioSource source = music.AddComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = true;
        music.AddComponent<BackgroundMusic>();
    }

    private Canvas EnsureCanvas()
    {
        Canvas existing = FindFirstObjectByType<Canvas>();
        if (existing != null)
        {
            existing.renderMode = RenderMode.ScreenSpaceOverlay;
            existing.sortingOrder = 500;
            existing.overrideSorting = true;
            CanvasScaler existingScaler = existing.GetComponent<CanvasScaler>();
            if (existingScaler == null)
            {
                existingScaler = existing.gameObject.AddComponent<CanvasScaler>();
            }

            existingScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            existingScaler.referenceResolution = new Vector2(1920f, 1080f);
            return existing;
        }

        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        canvas.overrideSorting = true;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<InputSystemUIInputModule>();
#else
            eventSystem.AddComponent<StandaloneInputModule>();
#endif
            eventSystem.transform.SetParent(transform);
        }

        return canvas;
    }

    private Text CreateText(Transform parent, string name, string content, Vector2 anchoredPos, int fontSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(520f, 48f);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = anchoredPos;

        Text text = go.GetComponent<Text>();
        text.font = GetBuiltinFont();
        text.text = content;
        text.color = Color.white;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleLeft;
        return text;
    }

    private InputField CreateInputField(Transform parent, string name, Vector2 anchoredPos, string placeholder)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
        root.transform.SetParent(parent, false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(220f, 36f);
        rootRect.anchorMin = new Vector2(0f, 1f);
        rootRect.anchorMax = new Vector2(0f, 1f);
        rootRect.pivot = new Vector2(0f, 1f);
        rootRect.anchoredPosition = anchoredPos;
        root.GetComponent<Image>().color = Color.white;

        GameObject placeholderObj = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
        placeholderObj.transform.SetParent(root.transform, false);
        RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = new Vector2(0f, 0f);
        placeholderRect.anchorMax = new Vector2(1f, 1f);
        placeholderRect.offsetMin = new Vector2(10f, 2f);
        placeholderRect.offsetMax = new Vector2(-10f, -2f);
        Text placeholderText = placeholderObj.GetComponent<Text>();
        placeholderText.font = GetBuiltinFont();
        placeholderText.text = placeholder;
        placeholderText.color = new Color(0.35f, 0.35f, 0.35f);

        GameObject inputObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        inputObj.transform.SetParent(root.transform, false);
        RectTransform inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0f, 0f);
        inputRect.anchorMax = new Vector2(1f, 1f);
        inputRect.offsetMin = new Vector2(10f, 2f);
        inputRect.offsetMax = new Vector2(-10f, -2f);
        Text inputText = inputObj.GetComponent<Text>();
        inputText.font = GetBuiltinFont();
        inputText.text = string.Empty;
        inputText.color = Color.black;

        InputField field = root.GetComponent<InputField>();
        field.textComponent = inputText;
        field.placeholder = placeholderText;
        field.characterLimit = 4;
        field.contentType = InputField.ContentType.IntegerNumber;
        return field;
    }

    private Button CreateButton(Transform parent, string name, string text, Vector2 anchoredPos, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = anchoredPos;
        go.GetComponent<Image>().color = new Color(0.16f, 0.52f, 0.86f, 1f);

        Text label = CreateText(go.transform, "Label", text, Vector2.zero, 20);
        label.alignment = TextAnchor.MiddleCenter;
        RectTransform lrt = label.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
        lrt.pivot = new Vector2(0.5f, 0.5f);

        return go.GetComponent<Button>();
    }

    private Button CreateCenteredButton(Transform parent, string name, string text, Vector2 anchoredPos, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        go.GetComponent<Image>().color = new Color(0.16f, 0.52f, 0.86f, 1f);

        Text label = CreateText(go.transform, "Label", text, Vector2.zero, 24);
        label.alignment = TextAnchor.MiddleCenter;
        RectTransform lrt = label.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
        lrt.pivot = new Vector2(0.5f, 0.5f);

        return go.GetComponent<Button>();
    }

    private Text CreateCenteredText(Transform parent, string name, string content, Vector2 anchoredPos, Vector2 size, int fontSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;

        Text text = go.GetComponent<Text>();
        text.font = GetBuiltinFont();
        text.text = content;
        text.color = Color.white;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        return text;
    }

    private GameObject CreateUiPanel(Transform parent, string name, Vector2 anchoredPos, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private Slider CreateSlider(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(Slider));
        root.transform.SetParent(parent, false);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = anchoredPos;

        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(root.transform, false);
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        bg.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(root.transform, false);
        RectTransform faRt = fillArea.GetComponent<RectTransform>();
        faRt.anchorMin = Vector2.zero;
        faRt.anchorMax = Vector2.one;
        faRt.offsetMin = new Vector2(5f, 5f);
        faRt.offsetMax = new Vector2(-5f, -5f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        fill.GetComponent<Image>().color = new Color(0.15f, 0.74f, 0.35f);

        Slider slider = root.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.fillRect = fillRt;
        slider.targetGraphic = fill.GetComponent<Image>();
        return slider;
    }

    private void CreateDigit(string name, string digit, Vector2 position, Text target)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        go.transform.position = position;
        CircleCollider2D trigger = go.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 0.35f;
        DigitCollectible collectible = go.AddComponent<DigitCollectible>();
        collectible.Configure(digit, target);
        CreateWorldLabel(name + "_Label", digit, position + new Vector2(0f, 0.15f), 1.2f, new Color(0.2f, 0.2f, 0.2f));
    }

    private void CreateCrouchHazard(string name, Vector2 pos, Vector2 size, LevelRespawnManager manager)
    {
        GameObject go = CreateBox(name, pos, size, new Color(0.85f, 0.25f, 0.25f), true);
        CrouchHazard hazard = go.AddComponent<CrouchHazard>();
        hazard.Configure(manager);
    }

    private void CreateJumpHazard(string name, Vector2 pos, Vector2 size, LevelRespawnManager manager)
    {
        GameObject go = CreateBox(name, pos, size, new Color(0.72f, 0.22f, 0.22f), true);
        KillTrigger kill = go.AddComponent<KillTrigger>();
        kill.Configure(manager);
    }

    private Transform CreatePoint(string name, Vector2 pos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        go.transform.position = pos;
        return go.transform;
    }

    private GameObject CreatePlatform(string name, Vector2 position, Vector2 size)
    {
        return CreateBox(name, position, size, new Color(0.26f, 0.26f, 0.3f), false);
    }

    private GameObject CreateBox(string name, Vector2 position, Vector2 size, Color color, bool trigger)
    {
        GameObject go = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D));
        go.transform.SetParent(transform);
        go.transform.position = position;
        go.transform.localScale = Vector3.one;

        SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
        renderer.sprite = SpriteFactory.Pixel;
        renderer.color = color;
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = size;

        BoxCollider2D col = go.GetComponent<BoxCollider2D>();
        col.isTrigger = trigger;
        col.size = size;
        col.offset = Vector2.zero;
        if (!trigger)
        {
            Rigidbody2D staticBody = go.AddComponent<Rigidbody2D>();
            staticBody.bodyType = RigidbodyType2D.Static;
            PhysicsMaterial2D slip = new PhysicsMaterial2D("PlatformSlip")
            {
                friction = 0f,
                bounciness = 0f
            };
            col.sharedMaterial = slip;
        }

        return go;
    }

    private void CreateWorldLabel(string name, string message, Vector2 position, float scale, Color color)
    {
        GameObject label = new GameObject(name, typeof(TextMesh));
        label.transform.SetParent(transform);
        label.transform.position = new Vector3(position.x, position.y, 0f);
        label.transform.localScale = Vector3.one * scale * 0.08f;
        TextMesh text = label.GetComponent<TextMesh>();
        text.text = message;
        text.characterSize = 1f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = color;
        text.fontSize = 80;
    }

    private Font GetBuiltinFont()
    {
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private void AddHoldButtonEvents(Button button, UnityEngine.Events.UnityAction onDown, UnityEngine.Events.UnityAction onUp)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        trigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();

        EventTrigger.Entry downEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        downEntry.callback.AddListener(_ => onDown.Invoke());
        trigger.triggers.Add(downEntry);

        EventTrigger.Entry upEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        upEntry.callback.AddListener(_ => onUp.Invoke());
        trigger.triggers.Add(upEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener(_ => onUp.Invoke());
        trigger.triggers.Add(exitEntry);
    }
}
