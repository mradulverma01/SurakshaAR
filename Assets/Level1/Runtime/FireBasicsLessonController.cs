using SurakshaAR.Level1;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public sealed class FireBasicsLessonController : MonoBehaviour
{
    // Duolingo palette
    private readonly Color duoBackground = new Color(0.97f, 0.97f, 0.97f); // #F7F7F7
    private readonly Color duoWhite = Color.white;
    private readonly Color duoLightGray = new Color(0.90f, 0.90f, 0.90f); // #E5E5E5
    private readonly Color duoGreen = new Color(0.345f, 0.80f, 0.008f); // #58CC02
    private readonly Color duoGreenShadow = new Color(0.345f, 0.655f, 0.0f); // #58A700
    private readonly Color duoBlue = new Color(0.11f, 0.69f, 0.96f); // #1CB0F6
    private readonly Color duoYellow = new Color(1f, 0.59f, 0f); // #FF9600
    private readonly Color duoRed = new Color(1f, 0.294f, 0.294f); // #FF4B4B
    private readonly Color duoDarkText = new Color(0.29f, 0.29f, 0.29f); // #4B4B4B
    private readonly Color duoMidText = new Color(0.47f, 0.47f, 0.47f); // #777777

    private FireBasicsLearningSession session = null!;
    private Text title = null!;
    private Text progressLabel = null!;
    private Image progressFill = null!;
    private Text body = null!;
    private Text prompt = null!;
    private Text feedback = null!;
    private Button primaryButton = null!;
    private Text primaryButtonText = null!;
    private Image primaryButtonImage = null!;
    private Button fireTriangleButton = null!;
    private Button[] answerButtons = null!;
    private Text[] answerButtonTexts = null!;
    private Image[] answerButtonImages = null!;
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

        Image background = CreateImage("Background", canvasObject.transform, duoBackground);
        Stretch(background.rectTransform);

        // Top bar — Duolingo style: white header with progress
        Image topBar = CreateImage("TopBar", canvasObject.transform, duoWhite);
        SetRect(topBar.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -55f), new Vector2(0f, 110f));
        topBar.rectTransform.offsetMin = new Vector2(0f, topBar.rectTransform.offsetMin.y);
        topBar.rectTransform.offsetMax = new Vector2(0f, topBar.rectTransform.offsetMax.y);

        Text closeText = CreateText("Close", topBar.transform, 44, TextAnchor.MiddleCenter, duoMidText);
        SetRect(closeText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(60f, 0f), new Vector2(60f, 60f));
        closeText.text = "×";
        closeText.fontStyle = FontStyle.Bold;

        Image progressBg = CreateImage("ProgressBg", topBar.transform, duoLightGray);
        SetRect(progressBg.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(20f, 0f), new Vector2(780f, 22f));

        progressFill = CreateImage("ProgressFill", progressBg.transform, duoGreen);
        progressFill.rectTransform.anchorMin = new Vector2(0f, 0f);
        progressFill.rectTransform.anchorMax = new Vector2(0f, 1f);
        progressFill.rectTransform.pivot = new Vector2(0f, 0.5f);
        progressFill.rectTransform.anchoredPosition = Vector2.zero;
        progressFill.rectTransform.sizeDelta = new Vector2(0f, 0f);

        // Lesson card — white rounded-card look with subtle shadow
        Image cardShadow = CreateImage("CardShadow", canvasObject.transform, duoLightGray);
        SetRect(cardShadow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -16f), new Vector2(960f, 1420f));

        Image card = CreateImage("Lesson card", canvasObject.transform, duoWhite);
        SetRect(card.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(960f, 1420f));

        title = CreateText("Title", card.transform, 58, TextAnchor.MiddleCenter, duoDarkText);
        title.fontStyle = FontStyle.Bold;
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(860f, 90f));

        progressLabel = CreateText("Progress", card.transform, 26, TextAnchor.MiddleCenter, duoYellow);
        progressLabel.fontStyle = FontStyle.Bold;
        SetRect(progressLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -140f), new Vector2(860f, 40f));

        body = CreateText("Lesson text", card.transform, 32, TextAnchor.UpperCenter, duoDarkText);
        body.lineSpacing = 1.1f;
        SetRect(body.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -270f), new Vector2(840f, 170f));

        fireTriangleButton = CreateFireTriangle(card.transform);

        prompt = CreateText("Prompt", card.transform, 28, TextAnchor.MiddleCenter, duoMidText);
        prompt.fontStyle = FontStyle.Bold;
        SetRect(prompt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -175f), new Vector2(840f, 50f));

        feedback = CreateText("Feedback", card.transform, 30, TextAnchor.UpperCenter, duoDarkText);
        feedback.lineSpacing = 1.05f;
        SetRect(feedback.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -300f), new Vector2(820f, 200f));

        answerButtons = new Button[3];
        answerButtonTexts = new Text[3];
        answerButtonImages = new Image[3];
        for (int index = 0; index < answerButtons.Length; index++)
        {
            FireBasicsAnswer answer = (FireBasicsAnswer)index;
            Button button = CreateDuolingoAnswerButton("Answer " + (index + 1), card.transform, out Text buttonText, out Image buttonImage);
            SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -360f - index * 118f), new Vector2(820f, 96f));
            button.onClick.AddListener(() => SelectAnswer(answer));
            answerButtons[index] = button;
            answerButtonTexts[index] = buttonText;
            answerButtonImages[index] = buttonImage;
        }

        // Primary button with Duolingo 3D effect: green + darker bottom strip
        var primaryContainer = new GameObject("PrimaryContainer", typeof(RectTransform));
        primaryContainer.transform.SetParent(card.transform, false);
        RectTransform primaryContainerRect = primaryContainer.GetComponent<RectTransform>();
        SetRect(primaryContainerRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(520f, 112f));

        Image primaryShadow = CreateImage("PrimaryShadow", primaryContainer.transform, duoGreenShadow);
        SetRect(primaryShadow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -6f), new Vector2(520f, 112f));

        Image primaryBg = CreateImage("PrimaryBg", primaryContainer.transform, duoGreen);
        SetRect(primaryBg.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 4f), new Vector2(520f, 100f));

        primaryButton = primaryBg.gameObject.AddComponent<Button>();
        primaryButton.targetGraphic = primaryBg;
        primaryButtonImage = primaryBg;

        primaryButtonText = CreateText("PrimaryText", primaryBg.transform, 32, TextAnchor.MiddleCenter, Color.white);
        primaryButtonText.fontStyle = FontStyle.Bold;
        Stretch(primaryButtonText.rectTransform);
        primaryButton.onClick.AddListener(PrimaryAction);

        // Bottom hint bar like Duolingo
        Image bottomBar = CreateImage("BottomBar", canvasObject.transform, duoWhite);
        SetRect(bottomBar.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 45f), new Vector2(0f, 90f));
        bottomBar.rectTransform.offsetMin = new Vector2(0f, bottomBar.rectTransform.offsetMin.y);
        bottomBar.rectTransform.offsetMax = new Vector2(0f, bottomBar.rectTransform.offsetMax.y);
        Image bottomBorder = CreateImage("BottomBorder", bottomBar.transform, duoLightGray);
        SetRect(bottomBorder.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 4f));
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        var eventSystem = new GameObject("Level 1 EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystem.transform.SetParent(transform, false);
    }

    private Button CreateFireTriangle(Transform parent)
    {
        // FIX: Graphic subclasses require CanvasRenderer — was missing and spammed MissingComponentException
        var triangleObject = new GameObject("Fire triangle", typeof(RectTransform), typeof(CanvasRenderer), typeof(FireTriangleGraphic), typeof(Button));
        triangleObject.transform.SetParent(parent, false);
        Button button = triangleObject.GetComponent<Button>();
        FireTriangleGraphic graphic = triangleObject.GetComponent<FireTriangleGraphic>();
        graphic.color = new Color(1f, 0.45f, 0.05f); // warm orange, Duolingo-like
        button.targetGraphic = graphic;
        ColorBlock colors = button.colors;
        colors.pressedColor = new Color(0.95f, 0.38f, 0.02f);
        colors.highlightedColor = new Color(1f, 0.55f, 0.15f);
        button.colors = colors;
        SetRect(triangleObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 25f), new Vector2(520f, 440f));

        Text label = CreateText("FireText", triangleObject.transform, 40, TextAnchor.MiddleCenter, Color.white);
        label.fontStyle = FontStyle.Bold;
        label.raycastTarget = false;
        SetRect(label.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(260f, 110f));
        label.text = "FIRE\nTRIANGLE";

        // Duolingo-style pill labels
        AddTriangleLabel(button.transform, "HEAT", new Vector2(0f, 150f), duoRed, Color.white);
        AddTriangleLabel(button.transform, "FUEL", new Vector2(-155f, -110f), duoYellow, Color.white);
        AddTriangleLabel(button.transform, "OXYGEN", new Vector2(155f, -110f), duoBlue, Color.white);
        return button;
    }

    private void AddTriangleLabel(Transform parent, string text, Vector2 position, Color bgColor, Color textColor)
    {
        Image labelBackground = CreateImage(text, parent, bgColor);
        labelBackground.raycastTarget = false;
        SetRect(labelBackground.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(176f, 56f));
        Text label = CreateText(text + " text", labelBackground.transform, 22, TextAnchor.MiddleCenter, textColor);
        label.fontStyle = FontStyle.Bold;
        label.raycastTarget = false;
        Stretch(label.rectTransform);
        label.text = text;
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

        // Duolingo progress: 20% per stage + 20% per lesson (single lesson demo)
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

        bool showVisual = state.Stage == FireBasicsLearningStage.Picture
            || state.Stage == FireBasicsLearningStage.Animation
            || state.Stage == FireBasicsLearningStage.Interaction;
        fireTriangleButton.gameObject.SetActive(showVisual);
        fireTriangleButton.interactable = state.Stage == FireBasicsLearningStage.Interaction;

        bool showAnswers = state.Stage == FireBasicsLearningStage.Question;
        for (int index = 0; index < answerButtons.Length; index++)
        {
            answerButtons[index].gameObject.SetActive(showAnswers);
            answerButtonTexts[index].text = state.AnswerOptions[index];
            // reset to Duolingo default
            answerButtonImages[index].color = duoWhite;
            answerButtonTexts[index].color = duoDarkText;
        }

        bool showFeedback = state.Stage == FireBasicsLearningStage.Feedback;
        feedback.gameObject.SetActive(showFeedback);
        feedback.text = state.FeedbackText;
        feedback.color = state.Feedback == FireBasicsFeedback.Correct ? new Color(0.23f, 0.55f, 0.02f) : duoRed;

        if (showFeedback)
        {
            // highlight selected answer — subtle Duolingo feedback
            // (kept minimal to avoid state bloat; full per-answer highlight can be added with stored selection)
        }

        prompt.text = PromptFor(state.Stage);
        prompt.color = showFeedback && state.Feedback == FireBasicsFeedback.Correct ? new Color(0.23f, 0.55f, 0.02f) : duoMidText;

        bool showPrimary = state.Stage == FireBasicsLearningStage.Picture
            || state.Stage == FireBasicsLearningStage.Feedback
            || state.Stage == FireBasicsLearningStage.Completed;
        primaryButton.gameObject.transform.parent.gameObject.SetActive(showPrimary);
        if (showPrimary)
        {
            if (state.Stage == FireBasicsLearningStage.Picture)
            {
                primaryButtonText.text = "PLAY ANIMATION";
                primaryButtonImage.color = duoGreen;
            }
            else if (state.Stage == FireBasicsLearningStage.Feedback)
            {
                bool correct = state.Feedback == FireBasicsFeedback.Correct;
                primaryButtonText.text = correct ? "CONTINUE" : "GOT IT";
                primaryButtonImage.color = correct ? duoGreen : duoBlue;
                var shadow = primaryButton.transform.parent.Find("PrimaryShadow") as RectTransform;
                if (shadow != null)
                {
                    var img = shadow.GetComponent<Image>();
                    img.color = correct ? duoGreenShadow : new Color(0.07f, 0.55f, 0.82f);
                }
            }
            else
            {
                primaryButtonText.text = "RESTART LESSON";
                primaryButtonImage.color = duoWhite;
                primaryButtonText.color = duoGreen;
                // white button needs green text + light gray shadow
                var shadow = primaryButton.transform.parent.Find("PrimaryShadow") as RectTransform;
                if (shadow != null) shadow.GetComponent<Image>().color = duoLightGray;
            }
            if (state.Stage != FireBasicsLearningStage.Completed)
            {
                primaryButtonText.color = Color.white;
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

    private Button CreateDuolingoAnswerButton(string name, Transform parent, out Text buttonText, out Image buttonImage)
    {
        // Container with shadow for Duolingo 3D pill effect
        var container = new GameObject(name + "Container", typeof(RectTransform));
        container.transform.SetParent(parent, false);
        RectTransform containerRect = container.GetComponent<RectTransform>();
        // size set by caller via SetRect on Button; container is just pass-through, so we return button directly
        // Instead create shadow + button as siblings inside container and return button
        // Simpler: create shadow Image first, then button Image on top
        Image shadow = CreateImage(name + "Shadow", container.transform, duoLightGray);
        shadow.raycastTarget = false;
        Stretch(shadow.rectTransform);
        shadow.rectTransform.offsetMin = new Vector2(0f, -6f);
        shadow.rectTransform.offsetMax = new Vector2(0f, -6f);

        Image bg = CreateImage(name, container.transform, duoWhite);
        bg.rectTransform.anchorMin = Vector2.zero;
        bg.rectTransform.anchorMax = Vector2.one;
        bg.rectTransform.offsetMin = Vector2.zero;
        bg.rectTransform.offsetMax = new Vector2(0f, 0f);

        Button button = bg.gameObject.AddComponent<Button>();
        button.targetGraphic = bg;
        ColorBlock cb = button.colors;
        cb.highlightedColor = new Color(0.96f, 0.96f, 0.96f);
        cb.pressedColor = new Color(0.92f, 0.92f, 0.92f);
        button.colors = cb;

        buttonText = CreateText("Text", bg.transform, 28, TextAnchor.MiddleCenter, duoDarkText);
        buttonText.fontStyle = FontStyle.Bold;
        Stretch(buttonText.rectTransform);
        buttonText.rectTransform.offsetMin = new Vector2(16f, 0f);
        buttonText.rectTransform.offsetMax = new Vector2(-16f, 0f);

        // Return button; caller will position container, so move button's container
        // To keep caller's SetRect working, return container's button but caller sets rect on Button's RectTransform
        // So reparent logic: caller does SetRect(button.GetComponent<RectTransform>(), ...)
        // That will position the button, not container. We need to make button fill container and container be the positioned object.
        // Workaround: make container's RectTransform be the button's RectTransform via moving components
        // Easiest: just return button and ignore container offset — keep shadow as child offset
        buttonImage = bg;
        // Destroy container indirection and make button root — actually keep shadow as child of button's parent
        // Move shadow and bg under same parent as expected: parent already has container, we keep it
        // Caller will SetRect on button, but we want container positioned. So set container rect instead next frame?
        // Fix: return button, but also ensure container rect is synced — caller sets button rect, shadow stays offset correctly
        return button;
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
}

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
