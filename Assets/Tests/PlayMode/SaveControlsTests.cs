using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using InfrastructureTycoon.Simulation;
using InfrastructureTycoon.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class SaveControlsTests
{
    private InfrastructureTycoon.GameRoot root;
    private string directory;
    private string path;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        directory = Path.Combine(Application.temporaryCachePath, "r2r-save-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        path = Path.Combine(directory, "company-v1.json");
        yield return SceneManager.LoadSceneAsync("Main");
        root = GameObject.Find("GameRoot").GetComponent<InfrastructureTycoon.GameRoot>();
        root.Clock.Speed = SimulationSpeed.Paused;
        root.ConfigureSaveSlot(new SessionSaveSlot(path));
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (directory != null && Directory.Exists(directory))
        {
            foreach (var file in Directory.GetFiles(directory)) File.Delete(file);
            Directory.Delete(directory);
        }
        yield return null;
    }

    [UnityTest]
    public IEnumerator SaveOutOfReachThenRepeatedLoadRebindsHudPanelButtonsAndBilling()
    {
        var original = root.Session;
        original.TryAcceptOffer("offer.photosnap", out var photo, out _);
        original.TryAssignContract(photo.Id, original.Hardware[0].Id, out _);
        root.Clock.RestoreState(new DateTime(1995, 1, 31), 0.75, SimulationSpeed.Paused);
        var station = UnityEngine.Object.FindFirstObjectByType<PcStationInteraction>();
        Assert.That(station.IsWithinReach, Is.False);
        Click("SaveGame");
        yield return null;
        Assert.That(File.Exists(path), Is.True);
        Assert.That(GameObject.Find("SaveFeedback").GetComponent<Text>().text, Does.Contain("Saved"));
        root.Clock.Speed = SimulationSpeed.Normal;
        root.Clock.Advance(0.5);
        root.Clock.Speed = SimulationSpeed.Paused;
        Assert.That(original.Cash, Is.EqualTo(230m));
        var player = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        player.GetComponent<PlayerKeyboardInput>().enabled = false;
        player.position = (Vector2)GameObject.Find("PCStation").transform.position + new Vector2(-1.5f, 0);
        for (int i = 0; i < 2; i++) yield return new WaitForFixedUpdate();
        Assert.That(station.TryOpen(), Is.True);
        var position = player.position;
        for (int i = 0; i < 3; i++)
        {
            Click("LoadGame");
            yield return null;
            Assert.That(root.Session, Is.Not.SameAs(original));
            Assert.That(root.Clock.DayProgress, Is.EqualTo(0.75));
            Assert.That(root.Clock.Speed, Is.EqualTo(SimulationSpeed.Paused));
            Assert.That(root.Session.Cash, Is.EqualTo(150m));
            Assert.That(GameObject.Find("CashLabel").GetComponent<Text>().text, Is.EqualTo("CASH  $150"));
            Assert.That(GameObject.Find("DateLabel").GetComponent<Text>().text, Is.EqualTo("DATE  31 JAN 1995"));
            Assert.That(station.IsOpen, Is.False);
            Assert.That(player.position, Is.EqualTo(position));
        }
        Assert.That(station.TryOpen(), Is.True);
        yield return null;
        yield return null;
        var row = GameObject.Find("Offer-offer.local-website").transform;
        Click(row.Find("AcceptOffer").gameObject);
        Click(row.Find("AssignWorkload").gameObject);
        Assert.That(root.Session.AcceptedContracts.Count, Is.EqualTo(2));
        Assert.That(original.AcceptedContracts.Count, Is.EqualTo(1));
        Click("Speed8x");
        Assert.That(root.Clock.Speed, Is.EqualTo(SimulationSpeed.Octuple));
        Assert.That(original.Clock.Speed, Is.EqualTo(SimulationSpeed.Paused));
        root.Clock.Advance(0.0625);
        root.Clock.Speed = SimulationSpeed.Paused;
        yield return null;
        Assert.That(root.Session.Cash, Is.EqualTo(310m));
        Assert.That(root.Session.BillingHistory.Count, Is.EqualTo(1));
        Assert.That(root.Session.AcceptedContracts[0].PaidMonths, Is.EqualTo(1));
        Assert.That(GameObject.Find("MonthlyResult").GetComponent<Text>().text, Does.Contain("Revenue $180"));
        Assert.That(original.Cash, Is.EqualTo(230m));
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator MissingCorruptUnsupportedAndUnreadableSavePreserveLiveState()
    {
        var original = root.Session;
        Click("LoadGame");
        yield return null;
        Assert.That(GameObject.Find("SaveFeedback").GetComponent<Text>().text, Does.Contain("No save"));
        var data = SessionSaveJson.Capture(original, "test");
        data.SaveVersion = 99;
        foreach (var json in new[] { "{", SessionSaveJson.ToJson(data) })
        {
            File.WriteAllText(path, json);
            Click("LoadGame");
            yield return null;
            Assert.That(root.Session, Is.SameAs(original));
            Assert.That(GameObject.Find("SaveFeedback").GetComponent<Text>().text, Does.Contain("invalid").Or.Contain("unsupported"));
        }
        File.WriteAllText(path, SessionSaveJson.ToJson(SessionSaveJson.Capture(original, "test")));
        using (var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            Click("LoadGame");
            yield return null;
            Assert.That(root.Session, Is.SameAs(original));
            Assert.That(GameObject.Find("SaveFeedback").GetComponent<Text>().text, Does.Contain("Could not read"));
        }
        Assert.That(root.Session.Cash, Is.EqualTo(150m));
    }

    [UnityTest]
    public IEnumerator AtomicReplacementWorksAndFailedReplacementPreservesPreviousSave()
    {
        Click("SaveGame");
        yield return null;
        AdvancePausedClock(62);
        Click("SaveGame");
        yield return null;
        string previous = File.ReadAllText(path);
        Assert.That(SessionSaveJson.TryLoad(previous, root.Session.Catalog, out var saved, out var reason), Is.True, reason);
        Assert.That(saved.Cash, Is.EqualTo(130m));
        AdvancePausedClock(56);
        using (var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            Click("SaveGame");
            yield return null;
            Assert.That(File.ReadAllText(path), Is.EqualTo(previous));
            Assert.That(GameObject.Find("SaveFeedback").GetComponent<Text>().text, Does.Contain("Could not write"));
        }
        Assert.That(Directory.GetFiles(directory).Length, Is.EqualTo(1), "Temporary files must be cleaned up.");
        Assert.That(root.Session.Cash, Is.EqualTo(110m));
        Click("LoadGame");
        yield return null;
        Assert.That(root.Session.Cash, Is.EqualTo(130m));
    }

    [UnityTest]
    public IEnumerator SaveRequestsDuringMonthEventsWaitForTheCompletedUpdate()
    {
        root.Clock.MonthEnded += _ => root.RequestSave();
        AdvancePausedClock(118);
        Assert.That(File.Exists(path), Is.False);
        yield return null;
        Assert.That(SessionSaveJson.TryLoad(File.ReadAllText(path), root.Session.Catalog, out var saved, out var reason), Is.True, reason);
        Assert.That(saved.BillingHistory.Count, Is.EqualTo(2));
        Assert.That(saved.Clock.Date, Is.EqualTo(new DateTime(1995, 3, 1)));
        Assert.That(saved.Cash, Is.EqualTo(110m));
    }

    private void AdvancePausedClock(double seconds)
    {
        root.Clock.Speed = SimulationSpeed.Normal;
        root.Clock.Advance(seconds);
        root.Clock.Speed = SimulationSpeed.Paused;
    }

    private static void Click(string name) => Click(GameObject.Find(name));
    private static void Click(GameObject obj)
    {
        Assert.That(obj, Is.Not.Null);
        var button = obj.GetComponent<Button>();
        Assert.That(button.interactable, Is.True);
        Canvas.ForceUpdateCanvases();
        var rect = (RectTransform)button.transform;
        var pointer = new PointerEventData(EventSystem.current)
        {
            button = PointerEventData.InputButton.Left,
            position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center))
        };
#if !UNITY_EDITOR
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, hits);
        Assert.That(hits.Count, Is.GreaterThan(0));
        Assert.That(hits[0].gameObject, Is.EqualTo(obj));
#endif
        ExecuteEvents.Execute(obj, pointer, ExecuteEvents.pointerClickHandler);
    }
}
