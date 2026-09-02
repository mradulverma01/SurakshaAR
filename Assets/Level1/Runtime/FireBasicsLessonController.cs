using System;
using SurakshaAR.Level1;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public sealed class FireBasicsLessonController : MonoBehaviour
{
    // --- Design System tokens (Duolingo-inspired + SurakshaAR Gamified Redesign) ---
    private readonly Color bg = Hex("#ffffff");
    private readonly Color surface = Hex("#f7f7f7");
    private readonly Color fg = Hex("#3c3c3c");
    private readonly Color muted = Hex("#777777");
    private readonly Color border = Hex("#e5e5e5");
    private readonly Color accent = Hex("#58cc02");
    private readonly Color accentPressed = Hex("#58a700");
    private readonly Color accentHover = Hex("#89e219");
    private readonly Color accentSecondary = Hex("#ff9600");
    private readonly Color info = Hex("#1cb0f6");
    private readonly Color error = Hex("#ff4b4b");
    private readonly Color successBg = Hex("#f5ffe6");
    private readonly Color errorBg = Hex("#fff2f0");
    private readonly Color successDark = Hex("#1d5900");

    private const float Space2 = 8f, Space4 = 16f, Space6 = 24f, Space8 = 32f;
    private const float BorderWidth = 2f, BorderBottom = 4f;
    private const float ControlHeightLg = 60f;
    private const float ColWidth = 920f;
    private const int AnswerCount = 3;
    private static readonly string[] Keys = { "A", "B", "C" };

    // Cached sprites loaded from Assets/Level1/Resources (application-agnostic)
    private static Sprite fireTriangleSprite;
    private static Sprite logsSprite;
    private static Sprite logsWithFireSprite;
    private static Sprite chipHeatSprite;
    private static Sprite chipOxygenSprite;

    private FireBasicsLearningSession session = null!;

    // Header
    private Image progressFill = null!;

    // Triangle stage
    private GameObject triangleRoot = null!;
    private Text trianglePrompt = null!;

    // Placing / ignition stage
    private GameObject placingRoot = null!;
    private GameObject fireRoot = null!;
    private FireFlameGraphic flame = null!;
    private Image glow = null!;
    private Text placePrompt = null!;
    private FireElementChip[] chips = new FireElementChip[3];
    private Image dropZone = null!;
    private Image woodImage = null!;
    private float ignitionStartedAt;
    private bool ignitionTicking;

    // Quick check
    private GameObject quickCheckRoot = null!;
    private Text questionText = null!;
    private readonly Image[] answerRings = new Image[AnswerCount];
    private readonly Image[] answerFaces = new Image[AnswerCount];
    private readonly Image[] answerShadows = new Image[AnswerCount];
    private readonly Image[] answerKeyBg = new Image[AnswerCount];
    private readonly Text[] answerKeyText = new Text[AnswerCount];
    private readonly Text[] answerLabels = new Text[AnswerCount];

    // Feedback
    private GameObject feedbackRoot = null!;
    private Image feedbackPanelBg = null!;
    private Text feedbackTitle = null!;
    private Text feedbackText = null!;

    // Primary action
    private Button primaryButton = null!;
    private Text primaryButtonText = null!;
    private Image primaryBg = null!;
    private Image primaryShadow = null!;

    private FireBasicsAnswer? selectedAnswer;

    private void Start()
    {
        session = new FireBasicsLearningSession();
        CreateInterface();
        Render();
    }

    private void Update()
    {
        if (session.State.Stage == FireBasicsLearningStage.Ignited)
        {
            float t = (Time.unscaledTime - ignitionStartedAt) / FireBasicsLearningSession.IgnitionDurationSeconds;
            if (flame != null)
            {
                flame.intensity = 0.8f + 0.2f * Mathf.Sin(Time.unscaledTime * 14f);
                flame.SetVerticesDirty();
            }
            if (glow != null)
            {
                Color c = glow.color;
                c.a = 0.35f + 0.15f * Mathf.Sin(Time.unscaledTime * 6f);
                glow.color = c;
            }
            if (session.FinishIgnition(Time.unscaledTime - ignitionStartedAt).Stage != FireBasicsLearningStage.Ignited)
            {
                ignitionTicking = false;
                Render();
            }
        }
    }

    private void CreateInterface()
    {
        var canvasObject = new GameObject("Level 1 Fire Basics UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        EnsureEventSystem();

        Image background = CreateImage("Background", canvasObject.transform, bg);
        Stretch(background.rectTransform);

        CreateHeader(canvasObject.transform);
        CreateTriangleStage(canvasObject.transform);
        CreatePlacingStage(canvasObject.transform);
        CreateQuickCheck(canvasObject.transform);
        CreateFeedbackPanel(canvasObject.transform);
        CreatePrimaryButton(canvasObject.transform);
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        var eventSystem = new GameObject("Level 1 EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystem.transform.SetParent(transform, false);
    }

    private void CreateHeader(Transform parent)
    {
        Image bar = CreateImage("Header", parent, bg);
        bar.raycastTarget = false;
        SetRect(bar.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -Space8), new Vector2(0f, Space8 * 2));
        Image hairline = CreateImage("HeaderHairline", bar.transform, Hex("#ececec"));
        hairline.raycastTarget = false;
        SetRect(hairline.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, BorderWidth));

        Image back = CreateImage("Back", bar.transform, bg);
        SetRect(back.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-(ColWidth / 2f - 34f), 0f), new Vector2(64f, 64f));
        AddBorder(back, border, BorderWidth, BorderBottom);
        back.gameObject.AddComponent<Button>().targetGraphic = back;
        Text chevron = CreateText("BackChevron", back.transform, 44, TextAnchor.MiddleCenter, fg);
        chevron.text = "\u2039";
        chevron.fontStyle = FontStyle.Bold;
        Stretch(chevron.rectTransform);

        Image streak = CreateImage("Streak", bar.transform, bg);
        SetRect(streak.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2((ColWidth / 2f - 44f), 0f), new Vector2(88f, 60f));
        AddBorder(streak, border, BorderWidth, BorderBottom);
        Text streakText = CreateText("StreakText", streak.transform, 30, TextAnchor.MiddleCenter, accentSecondary);
        streakText.text = "\u25B2 12";
        streakText.fontStyle = FontStyle.Bold;
        Stretch(streakText.rectTransform);

        Image pillBg = CreateImage("ProgressTrack", bar.transform, border);
        pillBg.raycastTarget = false;
        SetRect(pillBg.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, -22f), new Vector2(ColWidth * 0.8f, 16f));
        progressFill = CreateImage("ProgressFill", pillBg.transform, accent);
        progressFill.rectTransform.anchorMin = new Vector2(0f, 0f);
        progressFill.rectTransform.anchorMax = new Vector2(0f, 1f);
        progressFill.rectTransform.pivot = new Vector2(0f, 0.5f);
        progressFill.rectTransform.anchoredPosition = Vector2.zero;
        progressFill.rectTransform.sizeDelta = Vector2.zero;
    }

    private void CreateTriangleStage(Transform canvas)
    {
        triangleRoot = new GameObject("Triangle Stage", typeof(RectTransform));
        triangleRoot.transform.SetParent(canvas, false);
        // content column below header: top = -120, height 700 -> center -470
        SetRect(triangleRoot.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -470f), new Vector2(ColWidth, 700f));

        // Fire triangle visual
        var triObject = new GameObject("FireTriangle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        triObject.transform.SetParent(triangleRoot.transform, false);
        Image triImg = triObject.GetComponent<Image>();
        if (fireTriangleSprite == null) fireTriangleSprite = LoadSprite("fire_triangle");
        triImg.sprite = fireTriangleSprite;
        triImg.type = Image.Type.Simple;
        triImg.preserveAspect = true;
        triImg.color = Color.white;
        SetRect(triObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(640f, 520f));

        Text center = CreateText("TriCenter", triObject.transform, 46, TextAnchor.MiddleCenter, Color.white);
        center.text = "FIRE\nTRIANGLE";
        center.fontStyle = FontStyle.Bold;
        center.raycastTarget = false;
        SetRect(center.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -Space2), new Vector2(300f, 130f));

        AddTriangleLabel(triObject.transform, "HEAT", new Vector2(0f, 190f), error);
        AddTriangleLabel(triObject.transform, "FUEL", new Vector2(-170f, -170f), accentSecondary);
        AddTriangleLabel(triObject.transform, "OXYGEN", new Vector2(170f, -170f), info);

        trianglePrompt = CreateText("TrianglePrompt", triangleRoot.transform, 26, TextAnchor.MiddleCenter, muted);
        trianglePrompt.text = "A fire needs all three. Tap NEXT to bring them together.";
        trianglePrompt.fontStyle = FontStyle.Bold;
        SetRect(trianglePrompt.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(760f, 60f));
    }

    private static void AddTriangleLabel(Transform parent, string text, Vector2 position, Color color)
    {
        Image labelBg = CreateImage(text, parent, color);
        labelBg.raycastTarget = false;
        labelBg.gameObject.AddComponent<RectTransform>();
        labelBg.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        labelBg.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        labelBg.rectTransform.anchoredPosition = position;
        labelBg.rectTransform.sizeDelta = new Vector2(190f, 58f);
        AddBorder(labelBg, color, BorderWidth, BorderBottom);

        Text label = CreateText(text + " text", labelBg.transform, 24, TextAnchor.MiddleCenter, Color.white);
        label.fontStyle = FontStyle.Bold;
        label.raycastTarget = false;
        Stretch(label.rectTransform);
        label.text = text;
    }

    private void CreatePlacingStage(Transform canvas)
    {
        placingRoot = new GameObject("Placing Stage", typeof(RectTransform));
        placingRoot.transform.SetParent(canvas, false);
        SetRect(placingRoot.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -470f), new Vector2(ColWidth, 700f));

        // Wood logs backdrop (reference image) — placed first so the drop ring shows above it.
        // Placing shows the plain logs; the burning-logs image only appears after ignition.
        if (logsSprite == null) logsSprite = LoadSprite("logs");
        if (logsWithFireSprite == null) logsWithFireSprite = LoadSprite("logs_with_fire");
        woodImage = CreateImage("Wood Logs", placingRoot.transform, Color.white);
        woodImage.sprite = logsSprite;
        woodImage.type = Image.Type.Simple;
        woodImage.preserveAspect = true;
        woodImage.raycastTarget = false;
        woodImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        woodImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        woodImage.rectTransform.anchoredPosition = new Vector2(0f, -20f);
        woodImage.rectTransform.sizeDelta = new Vector2(420f, 300f);

        // Drop zone hint (ring where elements must be placed)
        dropZone = CreateImage("DropHint", placingRoot.transform, new Color(0.15f, 0.75f, 0.1f, 0.06f));
        dropZone.raycastTarget = false;
        DropBorder(dropZone, Hex("#58cc02"), BorderWidth);
        dropZone.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        dropZone.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        dropZone.rectTransform.anchoredPosition = new Vector2(0f, -20f);
        dropZone.rectTransform.sizeDelta = new Vector2(420f, 300f);

        // Fire root is a child of placing root so it lines up with the logs
        CreateFireRoot(placingRoot.transform);

        placePrompt = CreateText("PlacePrompt", placingRoot.transform, 26, TextAnchor.MiddleCenter, fg);
        placePrompt.text = "Drag HEAT, FUEL, and OXYGEN onto the wood.";
        placePrompt.fontStyle = FontStyle.Bold;
        SetRect(placePrompt.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(820f, 60f));

        // Draggable element chips
        FireElement[] elements = { FireElement.Heat, FireElement.Fuel, FireElement.Oxygen };
        Color[] colors = { error, accentSecondary, info };
        Vector2[] homes =
        {
            new Vector2(-350f, 240f),
            new Vector2(350f, 120f),
            new Vector2(-350f, -250f),
        };
        Vector2[] slots =
        {
            new Vector2(-90f, 130f),
            new Vector2(90f, 130f),
            new Vector2(0f, -150f),
        };
        string[] names = { "Heat", "Fuel", "Oxygen" };
        for (int i = 0; i < elements.Length; i++)
        {
            chips[i] = CreateElementChip(elements[i], names[i], colors[i], homes[i], slots[i]);
        }
    }

    private void CreateFireRoot(Transform placingParent)
    {
        fireRoot = new GameObject("Fire Root", typeof(RectTransform));
        fireRoot.transform.SetParent(placingParent, false);
        fireRoot.transform.SetAsLastSibling();
        RectTransform fireRect = fireRoot.GetComponent<RectTransform>();
        fireRect.anchorMin = new Vector2(0.5f, 0.5f);
        fireRect.anchorMax = new Vector2(0.5f, 0.5f);
        fireRect.anchoredPosition = new Vector2(0f, -18f);
        fireRect.sizeDelta = new Vector2(360f, 340f);

        glow = MakeRoundedImage("Glow", fireRoot.transform, Hex("#ff9600"), 60f);
        glow.raycastTarget = false;
        glow.color = new Color(1f, 0.59f, 0f, 0.4f);
        Stretch(glow.rectTransform, 0f);

        var flameObject = new GameObject("Flame", typeof(RectTransform), typeof(CanvasRenderer), typeof(FireFlameGraphic));
        flameObject.transform.SetParent(fireRoot.transform, false);
        flame = flameObject.GetComponent<FireFlameGraphic>();
        flame.color = Color.white;
        RectTransform fr = flameObject.GetComponent<RectTransform>();
        fr.anchorMin = new Vector2(0f, 0f);
        fr.anchorMax = new Vector2(1f, 1f);
        fr.offsetMin = new Vector2(30f, 20f);
        fr.offsetMax = new Vector2(-30f, -10f);

        fireRoot.SetActive(false);
    }

    private FireElementChip CreateElementChip(FireElement element, string name, Color color, Vector2 home, Vector2 slot)
    {
        Image card = MakeRoundedImage("Chip " + name, placingRoot.transform, bg, 16f);
        card.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        card.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        card.rectTransform.anchoredPosition = home;
        card.rectTransform.sizeDelta = new Vector2(200f, 200f);
        AddBorder(card, border, BorderWidth, BorderBottom);

        Image inner = MakeRoundedImage("ChipColor " + name, card.transform, color, 24f);
        inner.raycastTarget = false;
        inner.rectTransform.anchorMin = new Vector2(0.5f, 0.66f);
        inner.rectTransform.anchorMax = new Vector2(0.5f, 0.66f);
        inner.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        inner.rectTransform.anchoredPosition = new Vector2(0f, -10f);
        inner.rectTransform.sizeDelta = new Vector2(120f, 120f);

        // Use the reference images for Heat / Oxygen chip art; Fuel is color-coded.
        if (element == FireElement.Heat)
        {
            if (chipHeatSprite == null) chipHeatSprite = LoadSprite("element_heat");
            Sprite s = chipHeatSprite;
            if (s != null) { inner.sprite = s; inner.type = Image.Type.Simple; inner.color = Color.white; }
        }
        else if (element == FireElement.Oxygen)
        {
            if (chipOxygenSprite == null) chipOxygenSprite = LoadSprite("element_oxygen");
            Sprite s = chipOxygenSprite;
            if (s != null) { inner.sprite = s; inner.type = Image.Type.Simple; inner.color = Color.white; }
        }

        Text label = CreateText("ChipLabel " + name, card.transform, 34, TextAnchor.MiddleCenter, fg);
        label.text = name;
        label.fontStyle = FontStyle.Bold;
        label.raycastTarget = false;
        label.rectTransform.anchorMin = new Vector2(0f, 0.08f);
        label.rectTransform.anchorMax = new Vector2(1f, 0.08f);
        label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        label.rectTransform.anchoredPosition = new Vector2(0f, -30f);
        label.rectTransform.sizeDelta = new Vector2(0f, 40f);

        FireElementChip chip = card.gameObject.AddComponent<FireElementChip>();
        chip.Init(placingRoot.GetComponent<RectTransform>(), home, slot, element);
        chip.OnDrop = OnChipDropped;
        return chip;
    }

    private void CreateQuickCheck(Transform parent)
    {
        quickCheckRoot = new GameObject("Quick Check", typeof(RectTransform));
        quickCheckRoot.transform.SetParent(parent, false);
        SetRect(quickCheckRoot.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -430f), new Vector2(ColWidth, 720f));

        Text sectionTitle = CreateText("SectionTitle", quickCheckRoot.transform, 34, TextAnchor.MiddleLeft, fg);
        sectionTitle.text = "Quick check";
        sectionTitle.fontStyle = FontStyle.Bold;
        SetRect(sectionTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-390f, -44f), new Vector2(360f, 50f));

        Text lessonCount = CreateText("LessonCount", quickCheckRoot.transform, 24, TextAnchor.MiddleRight, muted);
        lessonCount.text = "STEP 2 OF 5";
        lessonCount.fontStyle = FontStyle.Bold;
        SetRect(lessonCount.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(330f, -44f), new Vector2(300f, 40f));

        questionText = CreateText("Question", quickCheckRoot.transform, 40, TextAnchor.MiddleCenter, fg);
        questionText.fontStyle = FontStyle.Bold;
        questionText.lineSpacing = 1.05f;
        SetRect(questionText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(880f, 150f));

        for (int index = 0; index < AnswerCount; index++)
        {
            CreateAnswer(index, new Vector2(0f, -(330f + index * 100f)));
        }
    }

    private void CreateAnswer(int index, Vector2 position)
    {
        Transform root = quickCheckRoot.transform;
        string key = Keys[index];

        answerShadows[index] = CreateImage("AnswerShadow " + key, root, Hex("#d9d9d9"));
        answerShadows[index].raycastTarget = false;
        SetRect(answerShadows[index].rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), position + new Vector2(0f, -BorderBottom - 2f), new Vector2(880f, 72f));

        answerRings[index] = MakeRoundedImage("AnswerRing " + key, root, border, 16f);
        answerRings[index].raycastTarget = false;
        SetRect(answerRings[index].rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), position, new Vector2(880f, 76f));

        answerFaces[index] = MakeRoundedImage("AnswerFace " + key, answerRings[index].transform, bg, 16f);
        Stretch(answerFaces[index].rectTransform, BorderWidth);
        Button button = answerFaces[index].gameObject.AddComponent<Button>();
        button.targetGraphic = answerFaces[index];
        ColorBlock cb = button.colors;
        cb.highlightedColor = Hex("#fafafa");
        cb.pressedColor = Hex("#f0f0f0");
        cb.disabledColor = new Color(0.9f, 0.9f, 0.9f, 0.5f);
        button.colors = cb;
        FireBasicsAnswer answer = (FireBasicsAnswer)index;
        button.onClick.AddListener(() => SelectAnswer(answer));

        answerKeyBg[index] = MakeRoundedImage("KeyBg " + key, answerFaces[index].transform, surface, 12f);
        answerKeyBg[index].rectTransform.anchorMin = new Vector2(0f, 0.5f);
        answerKeyBg[index].rectTransform.anchorMax = new Vector2(0f, 0.5f);
        answerKeyBg[index].rectTransform.pivot = new Vector2(0.5f, 0.5f);
        answerKeyBg[index].rectTransform.anchoredPosition = new Vector2(34f, 0f);
        answerKeyBg[index].rectTransform.sizeDelta = new Vector2(46f, 46f);

        answerKeyText[index] = CreateText("KeyText " + key, answerKeyBg[index].transform, 28, TextAnchor.MiddleCenter, fg);
        answerKeyText[index].text = key;
        answerKeyText[index].fontStyle = FontStyle.Bold;
        Stretch(answerKeyText[index].rectTransform);

        answerLabels[index] = CreateText("AnswerLabel " + key, answerFaces[index].transform, 27, TextAnchor.MiddleLeft, fg);
        answerLabels[index].fontStyle = FontStyle.Bold;
        answerLabels[index].rectTransform.anchorMin = new Vector2(0f, 0f);
        answerLabels[index].rectTransform.anchorMax = new Vector2(1f, 1f);
        answerLabels[index].rectTransform.offsetMin = new Vector2(96f, 4f);
        answerLabels[index].rectTransform.offsetMax = new Vector2(-26f, -4f);
    }

    private void CreateFeedbackPanel(Transform parent)
    {
        feedbackRoot = new GameObject("Feedback", typeof(RectTransform));
        feedbackRoot.transform.SetParent(parent, false);
        SetRect(feedbackRoot.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -430f), new Vector2(ColWidth, 640f));

        feedbackPanelBg = MakeRoundedImage("FeedbackPanel", feedbackRoot.transform, successBg, 16f);
        AddBorder(feedbackPanelBg, accent, BorderWidth, BorderBottom);
        SetRect(feedbackPanelBg.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(ColWidth, 470f));

        Image icon = MakeRoundedImage("FeedbackIcon", feedbackPanelBg.transform, successDark, 24f);
        icon.raycastTarget = false;
        SetRect(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-410f, -60f), new Vector2(68f, 68f));
        Text iconChar = CreateText("FeedbackIconText", icon.transform, 42, TextAnchor.MiddleCenter, Color.white);
        iconChar.text = "\u2713";
        iconChar.fontStyle = FontStyle.Bold;
        Stretch(iconChar.rectTransform);

        feedbackTitle = CreateText("FeedbackTitle", feedbackPanelBg.transform, 34, TextAnchor.MiddleLeft, successDark);
        feedbackTitle.fontStyle = FontStyle.Bold;
        SetRect(feedbackTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(120f, -60f), new Vector2(600f, 50f));

        feedbackText = CreateText("FeedbackText", feedbackPanelBg.transform, 27, TextAnchor.UpperLeft, fg);
        feedbackText.lineSpacing = 1.1f;
        SetRect(feedbackText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-20f, -200f), new Vector2(840f, 230f));
    }

    private void CreatePrimaryButton(Transform parent)
    {
        var container = new GameObject("PrimaryContainer", typeof(RectTransform));
        container.transform.SetParent(parent, false);

        primaryShadow = CreateImage("PrimaryShadow", container.transform, accentPressed);
        SetRect(primaryShadow.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 128f), new Vector2(ColWidth, ControlHeightLg + BorderBottom));
        primaryShadow.raycastTarget = false;

        primaryBg = MakeRoundedImage("PrimaryBg", container.transform, accent, 16f);
        SetRect(primaryBg.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 132f), new Vector2(ColWidth, ControlHeightLg));

        primaryButton = primaryBg.gameObject.AddComponent<Button>();
        primaryButton.targetGraphic = primaryBg;
        ColorBlock cb = primaryButton.colors;
        cb.highlightedColor = accentHover;
        cb.pressedColor = accent;
        cb.disabledColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        primaryButton.colors = cb;

        primaryButtonText = CreateText("PrimaryText", primaryBg.transform, 32, TextAnchor.MiddleCenter, Color.white);
        primaryButtonText.fontStyle = FontStyle.Bold;
        Stretch(primaryButtonText.rectTransform);
        primaryButton.onClick.AddListener(PrimaryAction);
    }

    // Drop-zone check box (local coords within placing root, logs centered near 0,-20)
    private static readonly Vector2 DropCenter = new Vector2(0f, -20f);
    private static readonly Vector2 DropHalf = new Vector2(210f, 150f);

    private void OnChipDropped(FireElementChip chip)
    {
        if (session.State.Stage != FireBasicsLearningStage.Placing)
        {
            chip.ResetChip();
            return;
        }

        Vector2 local = chip.rect.anchoredPosition;
        bool inDrop = Mathf.Abs(local.x - DropCenter.x) <= DropHalf.x &&
                      Mathf.Abs(local.y - DropCenter.y) <= DropHalf.y;

        if (inDrop)
        {
            chip.Lock(chip.slotPos);
            session.PlaceElement(chip.element);
        }
        else
        {
            chip.ResetChip();
        }

        Render();
    }

    private void ResetChips()
    {
        for (int i = 0; i < chips.Length; i++)
        {
            if (chips[i] != null) chips[i].ResetChip();
        }
    }

    private void PrimaryAction()
    {
        switch (session.State.Stage)
        {
            case FireBasicsLearningStage.Triangle:
                session.BeginPlacing();
                break;
            case FireBasicsLearningStage.Question:
                SubmitAnswer();
                break;
            case FireBasicsLearningStage.Feedback:
                session.Continue();
                break;
            case FireBasicsLearningStage.Completed:
                session.Restart();
                ResetChips();
                selectedAnswer = null;
                break;
        }
        Render();
    }

    private void SelectAnswer(FireBasicsAnswer answer)
    {
        if (session.State.Stage != FireBasicsLearningStage.Question) return;
        selectedAnswer = answer;
        Render();
    }

    private void SubmitAnswer()
    {
        if (selectedAnswer.HasValue)
        {
            session.Answer(selectedAnswer.Value);
            Render();
        }
    }

    private void Render()
    {
        var stage = session.State.Stage;
        if (progressFill != null)
        {
            progressFill.rectTransform.anchorMax = new Vector2(StageProgress(stage), 1f);
        }

        bool showTriangle = stage == FireBasicsLearningStage.Triangle;
        bool showPlacing = stage == FireBasicsLearningStage.Placing;
        bool showIgnited = stage == FireBasicsLearningStage.Ignited;

        triangleRoot.SetActive(showTriangle);
        placingRoot.SetActive(showPlacing || showIgnited);
        fireRoot.SetActive(showIgnited);
        if (placePrompt != null) placePrompt.gameObject.SetActive(showPlacing);

        // Wood shows the plain logs while dragging; the burning-logs photo replaces it
        // only once the seam reports ignition (all three elements placed).
        if (woodImage != null && logsSprite != null && logsWithFireSprite != null)
        {
            woodImage.sprite = showIgnited ? logsWithFireSprite : logsSprite;
        }
        if (dropZone != null) dropZone.gameObject.SetActive(showPlacing);

        if (showIgnited && !ignitionTicking)
        {
            ignitionStartedAt = Time.unscaledTime;
            ignitionTicking = true;
        }
        if (!showIgnited)
        {
            ignitionTicking = false;
        }

        bool showQuestion = stage == FireBasicsLearningStage.Question;
        quickCheckRoot.SetActive(showQuestion);
        if (showQuestion)
        {
            questionText.text = session.State.Question;
            int count = Mathf.Min(AnswerCount, session.State.AnswerOptions.Count);
            for (int i = 0; i < count; i++)
            {
                answerLabels[i].text = session.State.AnswerOptions[i];
                StyleAnswerSlot(i, selectedAnswer.HasValue && (int)selectedAnswer.Value == i);
            }
        }

        bool showFeedback = stage == FireBasicsLearningStage.Feedback || stage == FireBasicsLearningStage.Completed;
        feedbackRoot.SetActive(showFeedback);
        if (showFeedback)
        {
            bool correct = stage == FireBasicsLearningStage.Completed || session.State.Feedback == FireBasicsFeedback.Correct;
            Color panelBg = correct ? successBg : errorBg;
            Color edge = correct ? accent : error;
            Color dark = correct ? successDark : error;
            feedbackPanelBg.color = panelBg;
            feedbackTitle.color = dark;
            feedbackPanelBg.transform.Find("FeedbackIcon").GetComponent<Image>().color = dark;
            feedbackPanelBg.transform.Find("BottomBorder").GetComponent<Image>().color = edge;
            feedbackPanelBg.transform.Find("FeedbackIcon/FeedbackIconText").GetComponent<Text>().text = correct ? "\u2713" : "\u00D7";

            if (stage == FireBasicsLearningStage.Completed)
            {
                feedbackTitle.text = "Nailed it! +90 XP";
                feedbackText.text = "You completed the What is fire? lesson. Follow your facility emergency action plan and only fight a fire when trained, authorized, and safe.";
            }
            else
            {
                feedbackTitle.text = correct ? "Correct!" : "Not quite";
                feedbackText.text = session.State.FeedbackText;
            }
        }

        bool showPrimary = stage == FireBasicsLearningStage.Triangle || stage == FireBasicsLearningStage.Question || stage == FireBasicsLearningStage.Feedback || stage == FireBasicsLearningStage.Completed;
        primaryButton.transform.parent.gameObject.SetActive(showPrimary);
        if (showPrimary) RenderPrimary(stage);
    }

    private void RenderPrimary(FireBasicsLearningStage stage)
    {
        bool available = true;
        string label;
        switch (stage)
        {
            case FireBasicsLearningStage.Question:
                label = "CHECK ANSWER";
                available = selectedAnswer.HasValue;
                break;
            case FireBasicsLearningStage.Feedback:
                label = session.State.Feedback == FireBasicsFeedback.Correct ? "CONTINUE" : "GOT IT";
                break;
            case FireBasicsLearningStage.Completed:
                label = "RESTART LESSON";
                break;
            default:
                label = "NEXT";
                break;
        }

        primaryButtonText.text = label;
        primaryButton.interactable = available;
        if (!available)
        {
            primaryBg.color = Hex("#d9d9d9");
            primaryButtonText.color = Color.white;
            return;
        }

        if (stage == FireBasicsLearningStage.Completed)
        {
            primaryBg.color = bg;
            primaryShadow.color = border;
            primaryButtonText.color = accent;
        }
        else if (stage == FireBasicsLearningStage.Feedback && session.State.Feedback != FireBasicsFeedback.Correct)
        {
            primaryBg.color = info;
            primaryShadow.color = new Color(0.07f, 0.55f, 0.82f);
            primaryButtonText.color = Color.white;
        }
        else
        {
            primaryBg.color = accent;
            primaryShadow.color = accentPressed;
            primaryButtonText.color = Color.white;
        }
    }

    private void StyleAnswerSlot(int index, bool selected)
    {
        answerRings[index].color = selected ? accent : border;
        answerFaces[index].color = selected ? surface : bg;
        answerKeyBg[index].color = selected ? accent : surface;
        answerKeyText[index].color = selected ? Color.white : fg;
        answerLabels[index].color = selected ? successDark : fg;
    }

    private static float StageProgress(FireBasicsLearningStage stage)
    {
        switch (stage)
        {
            case FireBasicsLearningStage.Triangle: return 0.15f;
            case FireBasicsLearningStage.Placing: return 0.45f;
            case FireBasicsLearningStage.Ignited: return 0.7f;
            case FireBasicsLearningStage.Question: return 0.85f;
            case FireBasicsLearningStage.Feedback: return 0.95f;
            default: return 1f;
        }
    }

    // ---------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------
    private static Sprite roundedSprite;

    private static Image MakeRoundedImage(string name, Transform parent, Color color, float radius)
    {
        Image image = CreateImage(name, parent, color);
        if (roundedSprite == null) roundedSprite = BuildRoundedSprite(16f);
        image.sprite = roundedSprite;
        image.type = Image.Type.Sliced;
        return image;
    }

    private static Sprite BuildRoundedSprite(float radius)
    {
        const int size = 64;
        float r = Mathf.Clamp(radius, 1f, size / 2f - 1f) / (size / 16f);
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nearX = Mathf.Clamp(x, r, size - 1f - r);
                float nearY = Mathf.Clamp(y, r, size - 1f - r);
                float dx = x - nearX;
                float dy = y - nearY;
                bool inside = (dx * dx + dy * dy) <= r * r;
                texture.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        }
        texture.Apply();
        float border = Mathf.Floor(r);
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
    }

    private static Sprite LoadSprite(string name)
    {
        // Load as Texture2D and wrap — avoids depending on the import TextureType (Safe import).
        Texture2D tex = Resources.Load<Texture2D>(name);
        if (tex == null) return null;
        return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        var imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static void AddBorder(Image image, Color borderColor, float width, float bottomWidth)
    {
        var bottomBorder = new GameObject("BottomBorder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bottomBorder.transform.SetParent(image.transform, false);
        Image img = bottomBorder.GetComponent<Image>();
        img.color = borderColor;
        img.raycastTarget = false;
        RectTransform rt = bottomBorder.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0f, bottomWidth);
    }

    private static void DropBorder(Image image, Color color, float width)
    {
        // top + bottom bars forming a dashed-style ring
        foreach (bool top in new[] { true, false })
        {
            var bar = new GameObject(top ? "RingTop" : "RingBottom", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bar.transform.SetParent(image.transform, false);
            Image img = bar.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            RectTransform rt = bar.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, top ? 1f : 0f);
            rt.anchorMax = new Vector2(1f, top ? 1f : 0f);
            rt.pivot = new Vector2(0.5f, top ? 0f : 1f);
            rt.anchoredPosition = new Vector2(0f, 0f);
            rt.sizeDelta = new Vector2(0f, width);
        }
    }

    private static Text CreateText(string name, Transform parent, int fontSize, TextAnchor alignment, Color color)
    {
        var textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static void Stretch(RectTransform rectTransform, float inset = 0f)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = new Vector2(inset, inset);
        rectTransform.offsetMax = new Vector2(-inset, -inset);
    }

    private static void SetRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
    }

    private static Color Hex(string hex)
    {
        hex = hex.Replace("#", "");
        int r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        int g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        int b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color(r / 255f, g / 255f, b / 255f);
    }
}

// -------------------------------------------------------------------------
// Draggable element chip (Heat / Fuel / Oxygen)
// -------------------------------------------------------------------------
public sealed class FireElementChip : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Action<FireElementChip>? OnDrop;

    public RectTransform rect = null!;
    public RectTransform travelParent = null!;
    public Vector2 home;
    public Vector2 slotPos;
    public FireElement element;
    public bool placed;
    public Image graphic = null!;

    private void Awake()
    {
        rect = (RectTransform)transform;
        graphic = GetComponent<Image>();
    }

    public void Init(RectTransform parent, Vector2 home, Vector2 slot, FireElement element)
    {
        travelParent = parent;
        this.home = home;
        slotPos = slot;
        this.element = element;
        rect.anchoredPosition = home;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (placed) return;
        rect.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (placed) return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(travelParent, eventData.position, null, out Vector2 local))
        {
            rect.anchoredPosition = local;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (placed) return;
        OnDrop?.Invoke(this);
    }

    public void Lock(Vector2 position)
    {
        placed = true;
        rect.anchoredPosition = position;
        graphic.raycastTarget = false;
    }

    public void ResetChip()
    {
        placed = false;
        rect.anchoredPosition = home;
        graphic.raycastTarget = true;
    }
}

// -------------------------------------------------------------------------
// Fire triangle:
// -------------------------------------------------------------------------
public sealed class FireTriangleGraphic : Graphic
{
    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        Rect rect = rectTransform.rect;
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = new Vector3(0f, rect.yMax, 0f);
        vertexHelper.AddVert(vertex);
        vertex.position = new Vector3(rect.xMin, rect.yMin, 0f);
        vertexHelper.AddVert(vertex);
        vertex.position = new Vector3(rect.xMax, rect.yMin, 0f);
        vertexHelper.AddVert(vertex);
        vertexHelper.AddTriangle(0, 1, 2);
    }
}

// -------------------------------------------------------------------------
// Layered flame graphic (fire pops from the logs)
// -------------------------------------------------------------------------
public sealed class FireFlameGraphic : Graphic
{
    public float intensity = 1f;

    private static readonly Color Outer = new Color(1f, 0.31f, 0f);
    private static readonly Color Mid = new Color(1f, 0.59f, 0f);
    private static readonly Color Inner = new Color(1f, 0.84f, 0.04f);

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        Rect rect = rectTransform.rect;
        float baseY = rect.yMin;

        AddLayer(vertexHelper, baseY, rect.height * 0.95f, rect.width * 0.56f, Outer);
        AddLayer(vertexHelper, baseY, rect.height * 0.74f, rect.width * 0.36f, Mid);
        AddLayer(vertexHelper, baseY, rect.height * 0.48f, rect.width * 0.18f, Inner);
    }

    private void AddLayer(VertexHelper vertexHelper, float baseY, float height, float halfWidth, Color color)
    {
        float h = height * (0.9f + 0.1f * intensity);
        float w = halfWidth * (0.95f + 0.05f * intensity);

        UIVertex v = UIVertex.simpleVert;
        v.color = color;

        v.position = new Vector3(0f, baseY + h, 0f);
        vertexHelper.AddVert(v);
        v.position = new Vector3(-w, baseY, 0f);
        vertexHelper.AddVert(v);
        v.position = new Vector3(w, baseY, 0f);
        vertexHelper.AddVert(v);

        int start = vertexHelper.currentVertCount - 3;
        vertexHelper.AddTriangle(start, start + 1, start + 2);
    }
}
