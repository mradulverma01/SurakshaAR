using System.Collections.Generic;

namespace SurakshaAR.Level1
{
    public enum FireBasicsLearningStage
    {
        Picture,
        Animation,
        Interaction,
        Question,
        Feedback,
        Completed,
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
        internal FireBasicsLearningState(FireBasicsLearningStage stage, FireBasicsFeedback feedback, string feedbackText)
        {
            Stage = stage;
            Feedback = feedback;
            FeedbackText = feedbackText;
        }

        public FireBasicsLearningStage Stage { get; }

        public FireBasicsFeedback Feedback { get; }

        public string FeedbackText { get; }

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
        private FireBasicsLearningState state = NewState(FireBasicsLearningStage.Picture);

        public FireBasicsLearningState State => state;

        public FireBasicsLearningState ShowAnimation()
        {
            return Advance(FireBasicsLearningStage.Picture, FireBasicsLearningStage.Animation);
        }

        public FireBasicsLearningState FinishAnimation()
        {
            return Advance(FireBasicsLearningStage.Animation, FireBasicsLearningStage.Interaction);
        }

        public FireBasicsLearningState TapFireTriangle()
        {
            return Advance(FireBasicsLearningStage.Interaction, FireBasicsLearningStage.Question);
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
                    : "Not quite. A fire needs heat, fuel, and oxygen. Leave immediately when smoke, toxic gases, or heat make an area unsafe.");
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
                state = NewState(FireBasicsLearningStage.Picture);
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

        private static FireBasicsLearningState NewState(FireBasicsLearningStage stage)
        {
            return new FireBasicsLearningState(stage, FireBasicsFeedback.None, string.Empty);
        }
    }
}
