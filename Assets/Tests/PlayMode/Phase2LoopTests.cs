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

public class Phase2LoopTests
{
    private string directory;
    private int originalWidth, originalHeight;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        originalWidth = Screen.width;
        originalHeight = Screen.height;
        directory = Path.Combine(Application.temporaryCachePath, "r2r-phase2-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        yield return SceneManager.LoadSceneAsync("Main");
        yield return null;
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Screen.SetResolution(originalWidth, originalHeight, FullScreenMode.Windowed);
        if (directory != null && Directory.Exists(directory))
        {
            foreach (var file in Directory.GetFiles(directory)) File.Delete(file);
            Directory.Delete(directory);
        }
        yield return null;
    }

    [UnityTest]
    public IEnumerator FreshGameWalkHostSaveBeforeAndAfterBillingThenResumeNextMonth()
    {
        var root = GameObject.Find("GameRoot").GetComponent<InfrastructureTycoon.GameRoot>();
        string path = Path.Combine(directory, "company-v1.json");
        root.ConfigureSaveSlot(new SessionSaveSlot(path));
        var station = UnityEngine.Object.FindFirstObjectByType<PcStationInteraction>();
        Assert.That(root.Session.Cash, Is.EqualTo(150m));
        Assert.That(root.Session.AcceptedContracts, Is.Empty);
        station.HandleInput(true, false);
        Assert.That(station.IsOpen, Is.False, "Interaction must fail before walking into reach.");
        Click("Pause");

        var player = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        player.GetComponent<PlayerKeyboardInput>().enabled = false;
        var movement = player.GetComponent<PlayerMovement>();
        var start = player.position;
        var target = (Vector2)GameObject.Find("PCStation").transform.position + new Vector2(-1.5f, 0);
        // Drive the same movement motor as keyboard input, without teleporting the player.
        for (int step = 0; step < 200 && !station.IsWithinReach; step++)
        {
            movement.SetDirection((target - player.position).normalized);
            yield return new WaitForFixedUpdate();
        }
        movement.SetDirection(Vector2.zero);
        yield return new WaitForFixedUpdate();
        Assert.That(Vector2.Distance(start, player.position), Is.GreaterThan(1));
        Assert.That(station.IsWithinReach, Is.True);
        station.HandleInput(true, false);
        Assert.That(station.IsOpen, Is.True);
        yield return null;
        yield return null;
        var photoRow = GameObject.Find("Offer-offer.photosnap").transform;
        Click(photoRow.Find("AcceptOffer").gameObject);
        Assert.That(root.Session.Hardware[0].Resources.Used.CpuCores, Is.Zero);
        Click(photoRow.Find("AssignWorkload").gameObject);
        Assert.That(GameObject.Find("ResourceUsage").GetComponent<Text>().text,
            Is.EqualTo("STARTER PC   CPU 1/2 cores   |   RAM 2/4 GB\nSTORAGE 20/250 GB   |   BANDWIDTH 4/10 Mbps"));
        string contractId = root.Session.AcceptedContracts[0].Id;
        string pcId = root.Session.Hardware[0].Id;
        Click("Speed8x");
        AdvanceTo(root.Clock, new DateTime(1995, 1, 31), 0.75);
        Click("Pause");
        double savedProgress = root.Clock.DayProgress;
        Assert.That(savedProgress, Is.EqualTo(0.75).Within(1e-9));
        Click("SaveGame");
        yield return null;
        Assert.That(File.Exists(path), Is.True);
        Click("LoadGame");
        yield return null;
        Assert.That(root.Clock.Date, Is.EqualTo(new DateTime(1995, 1, 31)));
        Assert.That(root.Clock.DayProgress, Is.EqualTo(savedProgress));
        Assert.That(root.Session.Hardware[0].Id, Is.EqualTo(pcId));
        Assert.That(root.Session.AcceptedContracts[0].Id, Is.EqualTo(contractId));
        Assert.That(root.Session.AcceptedContracts[0].AssignedHardwareId, Is.EqualTo(pcId));
        Assert.That(root.Session.Cash, Is.EqualTo(150m));
        Assert.That(station.IsOpen, Is.False);

        Click("Speed1x");
        AdvanceTo(root.Clock, new DateTime(1995, 2, 1));
        Click("Pause");
        yield return null;
        AssertPaid(root, 230m, 1, new DateTime(1995, 1, 31));
        Click("SaveGame");
        yield return null;
        for (int i = 0; i < 3; i++)
        {
            Click("LoadGame");
            yield return null;
            AssertPaid(root, 230m, 1, new DateTime(1995, 1, 31));
            Assert.That(root.Session.TryBillMonth(new DateTime(1995, 1, 31), out _), Is.False);
            Assert.That(root.Clock.Speed, Is.EqualTo(SimulationSpeed.Paused));
        }

        Click("Speed4x");
        double before = root.Clock.DayProgress;
        yield return null;
        Assert.That(root.Clock.DayProgress, Is.GreaterThan(before));
        Click("Pause");
        var pausedDate = root.Clock.Date;
        double pausedProgress = root.Clock.DayProgress;
        yield return new WaitForSecondsRealtime(0.1f);
        Assert.That(root.Clock.Date, Is.EqualTo(pausedDate));
        Assert.That(root.Clock.DayProgress, Is.EqualTo(pausedProgress));
        Click("Speed2x");
        AdvanceTo(root.Clock, new DateTime(1995, 3, 1));
        Click("Pause");
        yield return null;
        AssertPaid(root, 310m, 2, new DateTime(1995, 2, 28));
        station.HandleInput(true, false);
        Assert.That(station.IsOpen, Is.True);
        yield return null;
        yield return null;
        Assert.That(GameObject.Find("Offer-offer.photosnap").transform.Find("OfferState").GetComponent<Text>().text, Does.Contain("HOSTED"));
        Assert.That(GameObject.Find("MonthlyResult").GetComponent<Text>().text, Does.Contain("Revenue $100").And.Contain("Expenses $20").And.Contain("Net +$80"));
        foreach (var size in new[] { new Vector2Int(800, 600), new Vector2Int(1280, 720) })
        {
            Screen.SetResolution(size.x, size.y, FullScreenMode.Windowed);
            yield return null;
            yield return null;
            ScreenCapture.CaptureScreenshot(Path.Combine(Application.temporaryCachePath, $"Phase2-Resumed-{size.x}x{size.y}.png"));
            for (int frame = 0; frame < 10; frame++) yield return null;
        }
        LogAssert.NoUnexpectedReceived();
    }

    private static void AdvanceTo(SimulationClock clock, DateTime date, double progress = 0)
    {
        double days = (date - clock.Date).TotalDays + progress - clock.DayProgress;
        clock.Advance(days * SimulationClock.SecondsPerDay / (int)clock.Speed);
    }

    private static void AssertPaid(InfrastructureTycoon.GameRoot root, decimal cash, int months, DateTime period)
    {
        Assert.That(root.Session.Cash, Is.EqualTo(cash));
        Assert.That(root.Session.BillingHistory.Count, Is.EqualTo(months));
        Assert.That(root.Session.AcceptedContracts[0].PaidMonths, Is.EqualTo(months));
        Assert.That(root.Session.LastMonthlyResult.PeriodEnd, Is.EqualTo(period));
        Assert.That(root.Session.LastMonthlyResult.Revenue, Is.EqualTo(100m));
        Assert.That(root.Session.LastMonthlyResult.Expenses, Is.EqualTo(20m));
        Assert.That(GameObject.Find("CashLabel").GetComponent<Text>().text, Is.EqualTo("CASH  $" + cash));
    }

    private static void Click(string name) => Click(GameObject.Find(name));
    private static void Click(GameObject obj)
    {
        Assert.That(obj, Is.Not.Null);
        Assert.That(obj.GetComponent<Button>().interactable, Is.True);
        Canvas.ForceUpdateCanvases();
        var rect = (RectTransform)obj.transform;
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
