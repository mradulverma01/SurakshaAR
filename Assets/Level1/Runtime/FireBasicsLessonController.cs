using SurakshaAR.Level1;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public sealed class FireBasicsLessonController : MonoBehaviour
{
    private readonly Color backgroundColor = new Color(0.04f, 0.08f, 0.12f);
    private readonly Color panelColor = new Color(0.08f, 0.16f, 0.22f);
    private readonly Color accentColor = new Color(1f, 0.42f, 0.08f);

    private FireBasicsLearningSession session = null!;
    private Text title = null!;
    private Text progress = null!;
    private Text body = null!;
    private Text prompt = null!;
    private Text feedback = null!;
    private Button primaryButton = null!;
    private Text primaryButtonText = null!;
    private Button fireTriangleButton = null!;
    private Button[] answerButtons = null!;
    private Text[] answerButtonTexts = null!;
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

        EnsureEventSystem();

        Image background = CreateImage("Background", canvasObject.transform, backgroundColor);
        Stretch(background.rectTransform);

        Image panel = CreateImage("Lesson panel", canvasObject.transform, panelColor);
        SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(920f, 1620f));

        title = CreateText("Title", panel.transform, 60, TextAnchor.MiddleCenter, Color.white);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(820f, 100f));

        progress = CreateText("Progress", panel.transform, 28, TextAnchor.MiddleCenter, new Color(0.7f, 0.8f, 0.85f));
        SetRect(progress.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -165f), new Vector2(820f, 50f));

        body = CreateText("Lesson text", panel.transform, 34, TextAnchor.UpperCenter, Color.white);
        SetRect(body.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -295f), new Vector2(800f, 190f));

        fireTriangleButton = CreateFireTriangle(panel.transform);

        prompt = CreateText("Prompt", panel.transform, 34, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.7f));
        SetRect(prompt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -185f), new Vector2(800f, 110f));

        feedback = CreateText("Feedback", panel.transform, 31, TextAnchor.UpperCenter, Color.white);
        SetRect(feedback.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -315f), new Vector2(780f, 190f));

        answerButtons = new Button[3];
        answerButtonTexts = new Text[3];
        for (int index = 0; index < answerButtons.Length; index++)
        {
            FireBasicsAnswer answer = (FireBasicsAnswer)index;
            Button button = CreateButton("Answer " + (index + 1), panel.transform, out Text buttonText);
            SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -375f - index * 115f), new Vector2(760f, 90f));
            button.onClick.AddListener(() => SelectAnswer(answer));
            answerButtons[index] = button;
            answerButtonTexts[index] = buttonText;
        }

        primaryButton = CreateButton("Primary action", panel.transform, out primaryButtonText);
        SetRect(primaryButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 105f), new Vector2(500f, 105f));
        primaryButton.onClick.AddListener(PrimaryAction);
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
        Button button = CreateButton("Fire triangle", parent, out Text label);
        SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 25f), new Vector2(500f, 420f));
        button.GetComponent<Image>().color = new Color(0.28f, 0.1f, 0.03f);
        label.text = "FIRE\nTRIANGLE";
        label.fontSize = 38;
        button.onClick.AddListener(TapFireTriangle);

        AddTriangleLabel(button.transform, "HEAT", new Vector2(0f, 130f), new Color(1f, 0.35f, 0.08f));
        AddTriangleLabel(button.transform, "FUEL", new Vector2(-145f, -100f), new Color(0.93f, 0.62f, 0.1f));
        AddTriangleLabel(button.transform, "OXYGEN", new Vector2(145f, -100f), new Color(0.25f, 0.68f, 0.95f));
        return button;
    }

    private void AddTriangleLabel(Transform parent, string text, Vector2 position, Color color)
    {
        Image labelBackground = CreateImage(text, parent, color);
        labelBackground.raycastTarget = false;
        SetRect(labelBackground.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(170f, 72f));
        Text label = CreateText(text + " text", labelBackground.transform, 23, TextAnchor.MiddleCenter, Color.black);
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
        progress.text = state.Stage == FireBasicsLearningStage.Completed ? "What is fire? complete" : "Level 1 | Fire basics | Lesson 1";
        body.text = state.Stage == FireBasicsLearningStage.Completed
            ? "You completed the What is fire? lesson. Follow your facility emergency action plan and only attempt firefighting when authorized, trained, and safe to do so."
            : state.LessonText;

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
        }

        feedback.gameObject.SetActive(state.Stage == FireBasicsLearningStage.Feedback);
        feedback.text = state.FeedbackText;
        prompt.text = PromptFor(state.Stage);

        primaryButton.gameObject.SetActive(state.Stage == FireBasicsLearningStage.Picture
            || state.Stage == FireBasicsLearningStage.Feedback
            || state.Stage == FireBasicsLearningStage.Completed);
        primaryButtonText.text = state.Stage == FireBasicsLearningStage.Picture ? "Play animation"
            : state.Stage == FireBasicsLearningStage.Feedback ? "Finish lesson"
            : "Restart lesson";
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
                return "Knowledge check";
            case FireBasicsLearningStage.Feedback:
                return "Immediate feedback";
            case FireBasicsLearningStage.Completed:
                return "Standalone Level 1 lesson";
            default:
                return "Start with the fire-triangle visual.";
        }
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        var imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static Text CreateText(string name, Transform parent, int fontSize, TextAnchor alignment, Color color)
    {
        var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private Button CreateButton(string name, Transform parent, out Text buttonText)
    {
        Image image = CreateImage(name, parent, accentColor);
        Button button = image.gameObject.AddComponent<Button>();
        buttonText = CreateText("Text", image.transform, 30, TextAnchor.MiddleCenter, Color.black);
        Stretch(buttonText.rectTransform);
        return button;
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
