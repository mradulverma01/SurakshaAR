using NUnit.Framework;
using SurakshaAR.Level1;

namespace SurakshaAR.Domain.Tests;

public sealed class FireBasicsLearningSessionTests
{
    [Test]
    public void Fire_only_ignites_after_every_element_is_placed_on_the_logs()
    {
        var session = new FireBasicsLearningSession();
        session.BeginPlacing();

        var afterHeat = session.PlaceElement(FireElement.Heat);
        var afterFuel = session.PlaceElement(FireElement.Fuel);
        var afterOxygen = session.PlaceElement(FireElement.Oxygen);

        Assert.Multiple(() =>
        {
            Assert.That(afterHeat.Stage, Is.EqualTo(FireBasicsLearningStage.Placing));
            Assert.That(afterFuel.Stage, Is.EqualTo(FireBasicsLearningStage.Placing));
            Assert.That(afterOxygen.Stage, Is.EqualTo(FireBasicsLearningStage.Ignited));
            Assert.That(afterOxygen.AllElementsPlaced, Is.True);
        });
    }

    [Test]
    public void Fire_does_not_ignite_until_all_three_elements_are_placed()
    {
        var session = new FireBasicsLearningSession();
        session.BeginPlacing();

        session.PlaceElement(FireElement.Heat);
        session.PlaceElement(FireElement.Fuel);
        var state = session.FinishIgnition(FireBasicsLearningSession.IgnitionDurationSeconds);

        Assert.That(state.Stage, Is.EqualTo(FireBasicsLearningStage.Placing));
        Assert.That(state.AllElementsPlaced, Is.False);
    }

    [Test]
    public void Completing_the_fire_basics_learning_loop_shows_correct_feedback()
    {
        var session = new FireBasicsLearningSession();
        session.BeginPlacing();
        session.PlaceElement(FireElement.Heat);
        session.PlaceElement(FireElement.Fuel);
        session.PlaceElement(FireElement.Oxygen);
        session.FinishIgnition(FireBasicsLearningSession.IgnitionDurationSeconds);

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
        session.BeginPlacing();
        session.PlaceElement(FireElement.Heat);
        session.PlaceElement(FireElement.Fuel);
        session.PlaceElement(FireElement.Oxygen);
        session.FinishIgnition(FireBasicsLearningSession.IgnitionDurationSeconds);

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

        session.PlaceElement(FireElement.Heat);
        Assert.That(session.State.Stage, Is.EqualTo(FireBasicsLearningStage.Triangle));

        session.BeginPlacing();
        session.FinishIgnition(FireBasicsLearningSession.IgnitionDurationSeconds);
        Assert.That(session.State.Stage, Is.EqualTo(FireBasicsLearningStage.Placing));

        session.PlaceElement(FireElement.Heat);
        session.PlaceElement(FireElement.Heat);
        Assert.That(session.State.Stage, Is.EqualTo(FireBasicsLearningStage.Placing));
    }
}
