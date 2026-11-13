using System;
using System.Collections;
using System.Collections.Generic;
using InfrastructureTycoon.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class ClockHudTests
{
    [UnityTest]
    public IEnumerator MainComposesAnIndependentStarterSessionFromTheCatalog()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        var root = GameObject.Find("GameRoot").GetComponent<InfrastructureTycoon.GameRoot>();
        var session = root.Session;
        Assert.That(session, Is.Not.Null);
        Assert.That(root.Clock, Is.SameAs(session.Clock));
        Assert.That(session.Cash, Is.EqualTo(150m));
        Assert.That(session.OperatingProfit, Is.Zero);
        Assert.That(session.AcceptedContracts, Is.Empty);
        Assert.That(session.Hardware.Count, Is.EqualTo(1));
        Assert.That(session.Hardware[0].Definition.Id, Is.EqualTo("hardware.starter-pc"));
        Assert.That(session.Hardware[0].Resources.Available.CpuCores, Is.EqualTo(2));
        session.Clock.Speed = SimulationSpeed.Paused;
        string instanceId = session.Hardware[0].Id;
        yield return SceneManager.LoadSceneAsync("Main");
        root = GameObject.Find("GameRoot").GetComponent<InfrastructureTycoon.GameRoot>();
        Assert.That(root.Session, Is.Not.SameAs(session));
        Assert.That(root.Clock.Speed, Is.EqualTo(SimulationSpeed.Normal));
        Assert.That(root.Session.Hardware[0].Id, Is.Not.EqualTo(instanceId));
        Assert.That(root.Session.Hardware[0].Definition.Capacity.CpuCores, Is.EqualTo(2));
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator MainShowsStartingDateAndNormalSpeed()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        Assert.That(GameObject.Find("DateLabel").GetComponent<Text>().text, Is.EqualTo("DATE  01 JAN 1995"));
        Assert.That(GameObject.Find("SpeedLabel").GetComponent<Text>().text, Is.EqualTo("SPEED  1x"));
        Assert.That(GameObject.Find("Speed1x").GetComponent<Button>().interactable, Is.False);
        Assert.That(EventSystem.current, Is.Not.Null);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator ControlsChangeRateAndPausePreservesPartialDayUntilResume()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        // Let the first Canvas render establish graphic depths before hit testing.
        yield return null;
        yield return null;
        var root = GameObject.Find("GameRoot").GetComponent<InfrastructureTycoon.GameRoot>();
        Assert.That(root, Is.Not.Null);
        foreach (int multiplier in new[] { 2, 4, 8, 1 })
        {
            Click("Speed" + multiplier + "x");
            Assert.That(root.Clock.Speed, Is.EqualTo((SimulationSpeed)multiplier));
            Assert.That(GameObject.Find("SpeedLabel").GetComponent<Text>().text, Is.EqualTo("SPEED  " + multiplier + "x"));
            double before = GameDays(root.Clock);
            double elapsed = 0;
            for (int frame = 0; frame < 10; frame++)
            {
                yield return null;
                elapsed += Time.unscaledDeltaTime;
            }
            Assert.That(GameDays(root.Clock) - before, Is.EqualTo(elapsed * multiplier / 2).Within(0.001));
            Assert.That(GameObject.Find("DateLabel").GetComponent<Text>().text,
                Is.EqualTo("DATE  " + root.Clock.Date.ToString("dd MMM yyyy", System.Globalization.CultureInfo.InvariantCulture).ToUpperInvariant()));
        }

        Click("Speed8x");
        var dateBeforeRollover = root.Clock.Date;
        yield return new WaitForSecondsRealtime(0.3f);
        Assert.That(root.Clock.Date, Is.GreaterThan(dateBeforeRollover));
        Assert.That(GameObject.Find("DateLabel").GetComponent<Text>().text,
            Is.EqualTo("DATE  " + root.Clock.Date.ToString("dd MMM yyyy", System.Globalization.CultureInfo.InvariantCulture).ToUpperInvariant()));
        Click("Pause");
        var pausedDate = root.Clock.Date;
        double pausedProgress = root.Clock.DayProgress;
        Assert.That(pausedProgress, Is.GreaterThan(0));
        yield return new WaitForSecondsRealtime(0.2f);
        Assert.That(root.Clock.Date, Is.EqualTo(pausedDate));
        Assert.That(root.Clock.DayProgress, Is.EqualTo(pausedProgress));
        Assert.That(GameObject.Find("DateLabel").GetComponent<Text>().text,
            Is.EqualTo("DATE  " + pausedDate.ToString("dd MMM yyyy", System.Globalization.CultureInfo.InvariantCulture).ToUpperInvariant()));
        Assert.That(GameObject.Find("SpeedLabel").GetComponent<Text>().text, Is.EqualTo("SPEED  PAUSED"));
        Click("Speed1x");
        Assert.That(root.Clock.Date, Is.EqualTo(pausedDate));
        Assert.That(root.Clock.DayProgress, Is.EqualTo(pausedProgress));
        yield return null;
        Assert.That(GameDays(root.Clock), Is.GreaterThan((pausedDate - new DateTime(1995, 1, 1)).TotalDays + pausedProgress));
        LogAssert.NoUnexpectedReceived();
    }

    private static double GameDays(SimulationClock clock) =>
        (clock.Date - new DateTime(1995, 1, 1)).TotalDays + clock.DayProgress;

    private static void Click(string name)
    {
        var button = GameObject.Find(name).GetComponent<Button>();
        Assert.That(button.IsInteractable(), Is.True);
        Canvas.ForceUpdateCanvases();
        var rect = (RectTransform)button.transform;
        var pointer = new PointerEventData(EventSystem.current)
        {
            button = PointerEventData.InputButton.Left,
            position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center))
        };
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, hits);
#if !UNITY_EDITOR
        // Batch-mode Editor has no rendered Game view; the player checks hit testing.
        Assert.That(hits.Count, Is.GreaterThan(0));
        Assert.That(hits[0].gameObject, Is.EqualTo(button.gameObject), "Button must be reachable by the UI raycaster.");
#endif
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        Assert.That(button.interactable, Is.False, "Selected speed must be visually distinct.");
    }
}
