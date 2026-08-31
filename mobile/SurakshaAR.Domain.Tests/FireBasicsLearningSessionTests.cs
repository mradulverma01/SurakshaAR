using NUnit.Framework;
using SurakshaAR.Level1;

namespace SurakshaAR.Domain.Tests;

public sealed class FireBasicsLearningSessionTests
{
    [Test]
    public void Completing_the_fire_basics_learning_loop_shows_correct_feedback()
    {
        var session = new FireBasicsLearningSession();

        session.ShowAnimation();
        session.FinishAnimation(FireBasicsLearningSession.AnimationDurationSeconds);
        session.TapFireTriangle();
        var state = session.Answer(FireBasicsAnswer.HeatFuelAndOxygen);
        var completed = session.Continue();

        Assert.Multiple(() =>
        {
            Assert.That(state.Stage, Is.EqualTo(FireBasicsLearningStage.Feedback));
            Assert.That(state.Feedback, Is.EqualTo(FireBasicsFeedback.Correct));
            Assert.That(state.FeedbackText, Does.Contain("heat, fuel, and oxygen"));
            Assert.That(completed.Stage, Is.EqualTo(FireBasicsLearningStage.Completed));
        });
    }

    [Test]
    public void Incorrect_answer_shows_corrective_fire_basics_feedback()
    {
        var session = new FireBasicsLearningSession();

        session.ShowAnimation();
        session.FinishAnimation(FireBasicsLearningSession.AnimationDurationSeconds);
        session.TapFireTriangle();
        var state = session.Answer(FireBasicsAnswer.SmokeAndFlames);

        Assert.Multiple(() =>
        {
            Assert.That(state.Stage, Is.EqualTo(FireBasicsLearningStage.Feedback));
            Assert.That(state.Feedback, Is.EqualTo(FireBasicsFeedback.Incorrect));
            Assert.That(state.FeedbackText, Does.Contain("heat, fuel, and oxygen"));
        });
    }

    [Test]
    public void Out_of_sequence_actions_do_not_bypass_the_learning_loop()
    {
        var session = new FireBasicsLearningSession();

        session.FinishAnimation(FireBasicsLearningSession.AnimationDurationSeconds);
        session.TapFireTriangle();
        session.Answer(FireBasicsAnswer.HeatFuelAndOxygen);

        Assert.That(session.State.Stage, Is.EqualTo(FireBasicsLearningStage.Picture));

        session.ShowAnimation();
        session.TapFireTriangle();
        Assert.That(session.State.Stage, Is.EqualTo(FireBasicsLearningStage.Animation));

        session.FinishAnimation(FireBasicsLearningSession.AnimationDurationSeconds);
        session.Answer(FireBasicsAnswer.HeatFuelAndOxygen);
        Assert.That(session.State.Stage, Is.EqualTo(FireBasicsLearningStage.Interaction));
    }

    [Test]
    public void Short_animation_does_not_enable_the_fire_triangle_interaction()
    {
        var session = new FireBasicsLearningSession();

        session.ShowAnimation();
        var state = session.FinishAnimation(FireBasicsLearningSession.AnimationDurationSeconds - 0.1f);

        Assert.That(state.Stage, Is.EqualTo(FireBasicsLearningStage.Animation));
    }
}
