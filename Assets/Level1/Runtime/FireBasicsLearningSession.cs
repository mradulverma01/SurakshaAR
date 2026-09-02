using System.Collections.Generic;

namespace SurakshaAR.Level1
{
    public enum FireBasicsLearningStage
    {
        Triangle,
        Placing,
        Ignited,
        Question,
        Feedback,
        Completed,
    }

    public enum FireElement
    {
        Heat,
        Fuel,
        Oxygen,
    }

    public enum FireBasicsAnswer
    {
        HeatFuelAndOxygen,
        SmokeAndFlames,
        WaterAndFoam,
    }

    public enum FireBasicsFeedback
    {
        None,
        Correct,
        Incorrect,
    }

    public sealed class FireBasicsLearningState
    {
        internal FireBasicsLearningState(
            FireBasicsLearningStage stage,
            FireBasicsFeedback feedback,
            string feedbackText,
            IReadOnlyList<FireElement>? placedElements)
        {
            Stage = stage;
            Feedback = feedback;
            FeedbackText = feedbackText;
            PlacedElements = placedElements ?? new List<FireElement>();
        }

        public FireBasicsLearningStage Stage { get; }

        public FireBasicsFeedback Feedback { get; }

        public string FeedbackText { get; }

        public IReadOnlyList<FireElement> PlacedElements { get; }

        public bool AllElementsPlaced => PlacedElements.Count == 3;

        public string Title => "What is fire?";

        public string LessonText => "Fire needs heat, fuel, and oxygen. Smoke, toxic gases, and heat can be more dangerous than visible flames.";

        public string Question => "What three elements sustain a fire?";

        public IReadOnlyList<string> AnswerOptions => AnswerOptionsText;

        private static readonly string[] AnswerOptionsText =
        {
            "Heat, fuel, and oxygen",
            "Smoke and flames",
            "Water and foam",
        };
    }

    public sealed class FireBasicsLearningSession
    {
        public const float IgnitionDurationSeconds = 1.6f;

        private FireBasicsLearningState state;
        private readonly List<FireElement> placedElements = new List<FireElement>();

        public FireBasicsLearningSession()
        {
            state = NewState(FireBasicsLearningStage.Triangle);
        }

        public FireBasicsLearningState State => state;

        public FireBasicsLearningState BeginPlacing()
        {
            return Advance(FireBasicsLearningStage.Triangle, FireBasicsLearningStage.Placing);
        }

        public FireBasicsLearningState PlaceElement(FireElement element)
        {
            if (state.Stage != FireBasicsLearningStage.Placing)
            {
                return state;
            }

            if (placedElements.Contains(element))
            {
                return state;
            }

            placedElements.Add(element);
            FireBasicsLearningStage next = placedElements.Count == 3
                ? FireBasicsLearningStage.Ignited
                : FireBasicsLearningStage.Placing;
            state = new FireBasicsLearningState(next, FireBasicsFeedback.None, string.Empty, placedElements);
            return state;
        }

        public FireBasicsLearningState FinishIgnition(float elapsedSeconds)
        {
            if (elapsedSeconds < IgnitionDurationSeconds)
            {
                return state;
            }

            return Advance(FireBasicsLearningStage.Ignited, FireBasicsLearningStage.Question);
        }

        public FireBasicsLearningState Answer(FireBasicsAnswer answer)
        {
            if (state.Stage != FireBasicsLearningStage.Question)
            {
                return state;
            }

            bool correct = answer == FireBasicsAnswer.HeatFuelAndOxygen;
            state = new FireBasicsLearningState(
                FireBasicsLearningStage.Feedback,
                correct ? FireBasicsFeedback.Correct : FireBasicsFeedback.Incorrect,
                correct
                    ? "Correct. Heat, fuel, and oxygen must be present for a fire to continue."
                    : "Not quite. A fire needs heat, fuel, and oxygen. Leave immediately when smoke, toxic gases, or heat make an area unsafe.",
                placedElements);
            return state;
        }

        public FireBasicsLearningState Continue()
        {
            return Advance(FireBasicsLearningStage.Feedback, FireBasicsLearningStage.Completed);
        }

        public FireBasicsLearningState Restart()
        {
            if (state.Stage == FireBasicsLearningStage.Completed)
            {
                placedElements.Clear();
                state = NewState(FireBasicsLearningStage.Triangle);
            }

            return state;
        }

        private FireBasicsLearningState Advance(FireBasicsLearningStage expected, FireBasicsLearningStage next)
        {
            if (state.Stage == expected)
            {
                state = NewState(next);
            }

            return state;
        }

        private FireBasicsLearningState NewState(FireBasicsLearningStage stage)
        {
            return new FireBasicsLearningState(stage, FireBasicsFeedback.None, string.Empty, placedElements);
        }
    }
}
