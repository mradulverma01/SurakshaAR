using SurakshaAR.Level1;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public sealed class FireBasicsLessonController : MonoBehaviour
{
    // Design System tokens (exact from Duolingo-inspired design system)
    // Colors
    private readonly Color bgLayout = Hex("#f7f7f7");        // --brand-color-bg-layout / --surface
    private readonly Color bgContainer = Hex("#ffffff");      // --brand-color-bg-container
    private readonly Color fg = Hex("#3c3c3c");               // --brand-color-text
    private readonly Color muted = Hex("#777777");            // --brand-color-text-secondary
    private readonly Color border = Hex("#e5e5e5");           // --brand-color-border
    private readonly Color accent = Hex("#58cc02");           // --brand-color-primary (Owl Green)
    private readonly Color accentPressed = Hex("#58a700");    // --brand-color-primary-active
    private readonly Color accentHover = Hex("#89e219");      // --brand-color-primary-hover
    private readonly Color accentSecondary = Hex("#ff9600");  // --brand-color-warning (Streak Orange)
    private readonly Color info = Hex("#1cb0f6");             // --brand-color-info
    private readonly Color error = Hex("#ff4b4b");            // --brand-color-error
    private readonly Color errorBg = Hex("#fff2f0");          // --brand-color-error-bg
    private readonly Color successBg = Hex("#f5ffe6");        // --brand-color-success-bg
    private readonly Color warningBg = Hex("#fff9e6");        // --brand-color-warning-bg
    private readonly Color infoBg = Hex("#e8fbff");           // --brand-color-info-bg

    // Sizing (4px base scale)
    private const float Space1 = 4f;
    private const float Space2 = 8f;
    private const float Space3 = 12f;
    private const float Space4 = 16f;
    private const float Space6 = 24f;
    private const float Space8 = 32f;
    private const float Space12 = 48f;

    // Border radius
    private const float RadiusCard = 16f;
    private const float RadiusPill = 9999f;

    // Border width (2px standard, 4px bottom on tactile controls)
    private const float BorderWidth = 2f;
    private const float BorderBottom = 4f;

    // Control heights
    private const float ControlHeight = 48f;
    private const float ControlHeightLg = 60f;

    // Motion
    private const float MotionFast = 0.18f;
    private const float MotionMid = 0.36f;

    private FireBasicsLearningSession session = null!;
    private Text title = null!;
    private Text progressLabel = null!;
    private Image progressFill = null!;
    private Text body = null!;
    private Text prompt = null!;
    private Text feedback = null!;
    private Button primaryButton = null!;
    private Text primaryButtonText = null!;
    private Image primaryButtonBg = null!;
    private Image primaryButtonShadow = null!;
    private Button fireTriangleButton = null!;
    private FireTriangleGraphic fireTriangleGraphic = null!;
    private Button[] answerButtons = null!;
    private Text[] answerButtonTexts = null!;
    private Image[] answerButtonBgs = null!;
    private Image[] answerButtonShadows = null!;
    private float animationStartedAt;

    private void Start()
    {
        session = new FireBasicsLearningSession();
        CreateInterface();
        Render();
    }

    private void Update()
    {
        if (session.State.Stage != FireBasicsLearningStage.Animation)
        {
            return;
        }

        float pulse = 1f + (Mathf.Sin(Time.unscaledTime * 8f) + 1f) * 0.04f;
        fireTriangleButton.transform.localScale = Vector3.one * pulse;

        if (session.FinishAnimation(Time.unscaledTime - animationStartedAt).Stage != FireBasicsLearningStage.Animation)
        {
            fireTriangleButton.transform.localScale = Vector3.one;
            Render();
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

        // Root background (layout bg)
        Image background = CreateImage("Background", canvasObject.transform, bgLayout);
        Stretch(background.rectTransform);

        // Top bar — white header with progress (Duolingo style)
        Image topBar = CreateImage("TopBar", canvasObject.transform, bgContainer);
        SetRect(topBar.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -(Space8 + Space4)), new Vector2(0f, Space8 * 2 + Space4));
        topBar.rectTransform.offsetMin = new Vector2(0f, topBar.rectTransform.offsetMin.y);
        topBar.rectTransform.offsetMax = new Vector2(0f, topBar.rectTransform.offsetMax.y);

        // Progress bar in top bar
        Image progressBg = CreateImage("ProgressBg", topBar.transform, border);
        SetRect(progressBg.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(Space6, 0f), new Vector2(780f, 22f));
        AddBorder(progressBg, border, BorderWidth);

        progressFill = CreateImage("ProgressFill", progressBg.transform, accent);
        progressFill.rectTransform.anchorMin = new Vector2(0f, 0f);
        progressFill.rectTransform.anchorMax = new Vector2(0f, 1f);
        progressFill.rectTransform.pivot = new Vector2(0f, 0.5f);
        progressFill.rectTransform.anchoredPosition = Vector2.zero;
        progressFill.rectTransform.sizeDelta = new Vector2(0f, 0f);

        // Lesson card — white with 2px border + 4px bottom border (tactile)
        Image card = CreateImage("Lesson card", canvasObject.transform, bgContainer);
        SetRect(card.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(960f, 1420f));
        AddBorder(card, border, BorderWidth, BorderBottom);
        card.rectTransform.offsetMin = new Vector2(card.rectTransform.offsetMin.x, card.rectTransform.offsetMin.y);
        card.rectTransform.offsetMax = new Vector2(card.rectTransform.offsetMax.x, card.rectTransform.offsetMax.y);

        // Title
        title = CreateText("Title", card.transform, 58, TextAnchor.MiddleCenter, fg);
        title.fontStyle = FontStyle.Bold;
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -(Space8 + Space6)), new Vector2(860f, 90f));

        // Progress label (Streak Orange)
        progressLabel = CreateText("Progress", card.transform, 26, TextAnchor.MiddleCenter, accentSecondary);
        progressLabel.fontStyle = FontStyle.Bold;
        SetRect(progressLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -(Space8 * 2 + Space4)), new Vector2(860f, 40f));

        // Body text
        body = CreateText("Lesson text", card.transform, 32, TextAnchor.UpperCenter, fg);
        body.lineSpacing = 1.1f;
        SetRect(body.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -(Space8 * 3 + Space6 + Space4)), new Vector2(840f, 170f));

        // Fire triangle interaction
        fireTriangleButton = CreateFireTriangle(card.transform);

        // Prompt
        prompt = CreateText("Prompt", card.transform, 28, TextAnchor.MiddleCenter, muted);
        prompt.fontStyle = FontStyle.Bold;
        SetRect(prompt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -Space6), new Vector2(840f, 50f));

        // Feedback
        feedback = CreateText("Feedback", card.transform, 30, TextAnchor.UpperCenter, fg);
        feedback.lineSpacing = 1.05f;
        SetRect(feedback.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, Space6), new Vector2(820f, 200f));

        // Answer buttons — Duolingo pill style with 3D shadow
        answerButtons = new Button[3];
        answerButtonTexts = new Text[3];
        answerButtonBgs = new Image[3];
        answerButtonShadows = new Image[3];
        for (int index = 0; index < answerButtons.Length; index++)
        {
            FireBasicsAnswer answer = (FireBasicsAnswer)index;
            CreateDuolingoAnswerButton(card.transform, answer, index, out Button button, out Text buttonText, out Image buttonBg, out Image buttonShadow);
            answerButtons[index] = button;
            answerButtonTexts[index] = buttonText;
            answerButtonBgs[index] = buttonBg;
            answerButtonShadows[index] = buttonShadow;
        }

        // Primary button — Duolingo 3D pill (green + 4px bottom shadow)
        CreatePrimaryButton(card.transform);

        // Bottom bar like Duolingo footer
        Image bottomBar = CreateImage("BottomBar", canvasObject.transform, bgContainer);
        SetRect(bottomBar.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, Space8 + Space4), new Vector2(0f, Space8 * 2));
        bottomBar.rectTransform.offsetMin = new Vector2(0f, bottomBar.rectTransform.offsetMin.y);
        bottomBar.rectTransform.offsetMax = new Vector2(0f, bottomBar.rectTransform.offsetMax.y);
        Image bottomBorder = CreateImage("BottomBorder", bottomBar.transform, border);
        SetRect(bottomBorder.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, BorderWidth));
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        var eventSystem = new GameObject("Level 1 EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystem.transform.SetParent(transform, false);
    }

    private Button CreateFireTriangle(Transform parent)
    {
        var triangleObject = new GameObject("Fire triangle", typeof(RectTransform), typeof(CanvasRenderer), typeof(FireTriangleGraphic), typeof(Button));
        triangleObject.transform.SetParent(parent, false);
        Button button = triangleObject.GetComponent<Button>();
        fireTriangleGraphic = triangleObject.GetComponent<FireTriangleGraphic>();
        fireTriangleGraphic.color = Hex("#ff6b00"); // vibrant orange for fire
        button.targetGraphic = fireTriangleGraphic;

        // Button states
        ColorBlock colors = button.colors;
        colors.highlightedColor = Hex("#ff8533");
        colors.pressedColor = Hex("#e65c00");
        colors.disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        button.colors = colors;

        SetRect(triangleObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, Space4), new Vector2(520f, 440f));

        // Center label
        Text label = CreateText("FireText", triangleObject.transform, 40, TextAnchor.MiddleCenter, Color.white);
        label.fontStyle = FontStyle.Bold;
        label.raycastTarget = false;
        SetRect(label.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -Space2), new Vector2(260f, 110f));
        label.text = "FIRE\nTRIANGLE";

        // Triangle corner labels — pill shaped with design system colors
        AddTriangleLabel(button.transform, "HEAT", new Vector2(0f, 150f), error, Color.white);
        AddTriangleLabel(button.transform, "FUEL", new Vector2(-155f, -110f), accentSecondary, Color.white);
        AddTriangleLabel(button.transform, "OXYGEN", new Vector2(155f, -110f), info, Color.white);

        return button;
    }

    private void AddTriangleLabel(Transform parent, string text, Vector2 position, Color bgColor, Color textColor)
    {
        Image labelBackground = CreateImage(text, parent, bgColor);
        labelBackground.raycastTarget = false;
        labelBackground.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        labelBackground.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        labelBackground.rectTransform.anchoredPosition = position;
        labelBackground.rectTransform.sizeDelta = new Vector2(176f, 56f);
        AddBorder(labelBackground, bgColor, BorderWidth, BorderBottom); // tactile bottom edge

        Text label = CreateText(text + " text", labelBackground.transform, 22, TextAnchor.MiddleCenter, textColor);
        label.fontStyle = FontStyle.Bold;
        label.raycastTarget = false;
        Stretch(label.rectTransform);
        label.text = text;
    }

    private void CreatePrimaryButton(Transform parent)
    {
        var container = new GameObject("PrimaryContainer", typeof(RectTransform));
        container.transform.SetParent(parent, false);

        // Shadow (4px bottom offset, darker accent)
        primaryButtonShadow = CreateImage("PrimaryShadow", container.transform, accentPressed);
        SetRect(primaryButtonShadow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -BorderBottom), new Vector2(520f, ControlHeightLg + BorderBottom));
        primaryButtonShadow.raycastTarget = false;

        // Button background
        primaryButtonBg = CreateImage("PrimaryBg", container.transform, accent);
        SetRect(primaryButtonBg.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, BorderBottom), new Vector2(520f, ControlHeightLg));
        AddBorder(primaryButtonBg, accentPressed, BorderWidth, BorderBottom);

        primaryButton = primaryButtonBg.gameObject.AddComponent<Button>();
        primaryButton.targetGraphic = primaryButtonBg;

        // Button states
        ColorBlock cb = primaryButton.colors;
        cb.highlightedColor = accentHover;
        cb.pressedColor = accent;
        cb.disabledColor = new Color(accent.r, accent.g, accent.b, 0.5f);
        primaryButton.colors = cb;

        primaryButtonText = CreateText("PrimaryText", primaryButtonBg.transform, 32, TextAnchor.MiddleCenter, Color.white);
        primaryButtonText.fontStyle = FontStyle.Bold;
        Stretch(primaryButtonText.rectTransform);
        primaryButton.onClick.AddListener(PrimaryAction);
    }

    private void CreateDuolingoAnswerButton(Transform parent, FireBasicsAnswer answer, int index, out Button button, out Text buttonText, out Image buttonBg, out Image buttonShadow)
    {
        // Container holds both shadow and button
        var container = new GameObject("Answer " + (index + 1), typeof(RectTransform));
        container.transform.SetParent(parent, false);

        // Shadow (4px bottom offset, light gray)
        buttonShadow = CreateImage("Shadow", container.transform, border);
        buttonShadow.raycastTarget = false;
        Stretch(buttonShadow.rectTransform);
        buttonShadow.rectTransform.offsetMin = new Vector2(0f, -BorderBottom);
        buttonShadow.rectTransform.offsetMax = new Vector2(0f, -BorderBottom);

        // Button background (white with 2px border, 4px bottom border)
        buttonBg = CreateImage("Bg", container.transform, bgContainer);
        buttonBg.rectTransform.anchorMin = Vector2.zero;
        buttonBg.rectTransform.anchorMax = Vector2.one;
        buttonBg.rectTransform.offsetMin = Vector2.zero;
        buttonBg.rectTransform.offsetMax = new Vector2(0f, 0f);
        AddBorder(buttonBg, border, BorderWidth, BorderBottom);

        button = buttonBg.gameObject.AddComponent<Button>();
        button.targetGraphic = buttonBg;

        // Button states (subtle)
        ColorBlock cb = button.colors;
        cb.highlightedColor = Hex("#f8f8f8");
        cb.pressedColor = Hex("#f0f0f0");
        cb.disabledColor = new Color(0.9f, 0.9f, 0.9f, 0.5f);
        button.colors = cb;

        buttonText = CreateText("Text", buttonBg.transform, 28, TextAnchor.MiddleCenter, fg);
        buttonText.fontStyle = FontStyle.Bold;
        Stretch(buttonText.rectTransform);
        buttonText.rectTransform.offsetMin = new Vector2(Space6, 0f);
        buttonText.rectTransform.offsetMax = new Vector2(-Space6, 0f);

        // Position container (caller sets rect on container)
        SetRect(container.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -(Space8 * 2 + Space6) - index * (Space8 + Space2)), new Vector2(820f, ControlHeightLg + BorderBottom));

        button.onClick.AddListener(() => SelectAnswer(answer));
    }

    private void PrimaryAction()
    {
        switch (session.State.Stage)
        {
            case FireBasicsLearningStage.Picture:
                session.ShowAnimation();
                animationStartedAt = Time.unscaledTime;
                break;
            case FireBasicsLearningStage.Feedback:
                session.Continue();
                break;
            case FireBasicsLearningStage.Completed:
                session.Restart();
                break;
        }
        Render();
    }

    private void TapFireTriangle()
    {
        session.TapFireTriangle();
        Render();
    }

    private void SelectAnswer(FireBasicsAnswer answer)
    {
        session.Answer(answer);
        Render();
    }

    private void Render()
    {
        FireBasicsLearningState state = session.State;
        title.text = state.Title;
        progressLabel.text = state.Stage == FireBasicsLearningStage.Completed ? "LESSON COMPLETE  •  1 / 1" : "LEVEL 1  •  FIRE BASICS  •  LESSON 1 OF 5";
        body.text = state.Stage == FireBasicsLearningStage.Completed
            ? "You completed the What is fire? lesson. Follow your facility emergency action plan and only attempt firefighting when authorized, trained, and safe to do so."
            : state.Stage == FireBasicsLearningStage.Question ? state.Question : state.LessonText;

        // Progress bar (top bar)
        float progress = 0f;
        switch (state.Stage)
        {
            case FireBasicsLearningStage.Picture: progress = 0.20f; break;
            case FireBasicsLearningStage.Animation: progress = 0.40f; break;
            case FireBasicsLearningStage.Interaction: progress = 0.60f; break;
            case FireBasicsLearningStage.Question: progress = 0.80f; break;
            case FireBasicsLearningStage.Feedback: progress = 0.90f; break;
            case FireBasicsLearningStage.Completed: progress = 1f; break;
        }
        if (progressFill != null)
        {
            progressFill.rectTransform.anchorMax = new Vector2(progress, 1f);
        }

        // Fire triangle visibility
        bool showVisual = state.Stage == FireBasicsLearningStage.Picture
            || state.Stage == FireBasicsLearningStage.Animation
            || state.Stage == FireBasicsLearningStage.Interaction;
        fireTriangleButton.gameObject.SetActive(showVisual);
        fireTriangleButton.interactable = state.Stage == FireBasicsLearningStage.Interaction;

        // Answer buttons
        bool showAnswers = state.Stage == FireBasicsLearningStage.Question;
        for (int index = 0; index < answerButtons.Length; index++)
        {
            answerButtons[index].gameObject.SetActive(showAnswers);
            answerButtonTexts[index].text = state.AnswerOptions[index];

            // Reset to default state (white bg, dark text)
            answerButtonBgs[index].color = bgContainer;
            answerButtonTexts[index].color = fg;
            // Restore border
            // Note: borders are on the Image component via AddBorder; color change handled by button states
        }

        // Feedback
        bool showFeedback = state.Stage == FireBasicsLearningStage.Feedback;
        feedback.gameObject.SetActive(showFeedback);
        if (showFeedback)
        {
            feedback.text = state.FeedbackText;
            feedback.color = state.Feedback == FireBasicsFeedback.Correct ? Hex("#1d5900") : error;

            // Highlight the selected answer (we don't track which was selected, so skip for now)
        }

        prompt.text = PromptFor(state.Stage);
        prompt.color = showFeedback && state.Feedback == FireBasicsFeedback.Correct ? Hex("#1d5900") : muted;

        // Primary button
        bool showPrimary = state.Stage == FireBasicsLearningStage.Picture
            || state.Stage == FireBasicsLearningStage.Feedback
            || state.Stage == FireBasicsLearningStage.Completed;
        primaryButton.transform.parent.gameObject.SetActive(showPrimary);
        if (showPrimary)
        {
            if (state.Stage == FireBasicsLearningStage.Picture)
            {
                primaryButtonText.text = "PLAY ANIMATION";
                primaryButtonBg.color = accent;
                primaryButtonShadow.color = accentPressed;
                primaryButtonText.color = Color.white;
            }
            else if (state.Stage == FireBasicsLearningStage.Feedback)
            {
                bool correct = state.Feedback == FireBasicsFeedback.Correct;
                primaryButtonText.text = correct ? "CONTINUE" : "GOT IT";
                if (correct)
                {
                    primaryButtonBg.color = accent;
                    primaryButtonShadow.color = accentPressed;
                    primaryButtonText.color = Color.white;
                }
                else
                {
                    primaryButtonBg.color = info;
                    primaryButtonShadow.color = new Color(0.07f, 0.55f, 0.82f); // darker info
                    primaryButtonText.color = Color.white;
                }
            }
            else // Completed
            {
                primaryButtonText.text = "RESTART LESSON";
                primaryButtonBg.color = bgContainer;
                primaryButtonShadow.color = border;
                primaryButtonText.color = accent;
            }
        }
    }

    private static string PromptFor(FireBasicsLearningStage stage)
    {
        switch (stage)
        {
            case FireBasicsLearningStage.Animation:
                return "Watch how the three elements work together.";
            case FireBasicsLearningStage.Interaction:
                return "Tap the fire triangle to continue.";
            case FireBasicsLearningStage.Question:
                return "Choose the correct answer";
            case FireBasicsLearningStage.Feedback:
                return "Tap below to continue";
            case FireBasicsLearningStage.Completed:
                return "Great job! You can restart to practice again.";
            default:
                return "Start with the fire-triangle visual.";
        }
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        var imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = true;
        return image;
    }

    private static void AddBorder(Image image, Color borderColor, float width, float bottomWidth = -1f)
    {
        // Unity UI doesn't have native border; we simulate with child images
        // For simplicity, we'll add a bottom border as a separate image
        // The design system uses 2px sides + 4px bottom on tactile controls
        if (bottomWidth > 0)
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

    private static void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
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

public sealed class FireTriangleGraphic : Graphic
{
    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        Rect rect = rectTransform.rect;
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        // Triangle pointing up (center top, bottom left, bottom right)
        vertex.position = new Vector3(0f, rect.yMax, 0f);
        vertexHelper.AddVert(vertex);
        vertex.position = new Vector3(rect.xMin, rect.yMin, 0f);
        vertexHelper.AddVert(vertex);
        vertex.position = new Vector3(rect.xMax, rect.yMin, 0f);
        vertexHelper.AddVert(vertex);
        vertexHelper.AddTriangle(0, 1, 2);
    }
}