using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InfrastructureTycoon;
using InfrastructureTycoon.Simulation;
using InfrastructureTycoon.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class Phase3LoopTests
{
    private GameRoot root;
    private Rigidbody2D player;
    private string directory, path;
    private int width, height;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        width=Screen.width; height=Screen.height;
        yield return SceneManager.LoadSceneAsync("Main");
        yield return null;
        yield return null;
        root=GameObject.Find("GameRoot").GetComponent<GameRoot>();
        Click(GameObject.Find("Pause"));
        player=GameObject.Find("Player").GetComponent<Rigidbody2D>();
        player.GetComponent<PlayerKeyboardInput>().enabled=false;
        directory=Path.Combine(Application.temporaryCachePath,"r2r-phase3-loop-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        path=Path.Combine(directory,"company-v1.json");
        root.ConfigureSaveSlot(new SessionSaveSlot(path));
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Screen.SetResolution(width,height,FullScreenMode.Windowed);
        if(directory!=null && Directory.Exists(directory))
        {
            foreach(var file in Directory.GetFiles(directory)) File.Delete(file);
            Directory.Delete(directory);
        }
        yield return null;
    }

    private static PcStationInteraction Station(string name) => GameObject.Find(name=="PCStation" ? "PcStationInteraction" : "HardwareStationInteraction").GetComponent<PcStationInteraction>();
    private static string Alerts => GameObject.Find("AlertsLabel").GetComponent<Text>().text;

    private IEnumerator Walk(Vector2 target)
    {
        var movement=player.GetComponent<PlayerMovement>();
        for(int step=0;step<240 && Vector2.Distance(target,player.position)>.08f;step++)
        {
            movement.SetDirection((target-player.position).normalized);
            yield return new WaitForFixedUpdate();
        }
        movement.SetDirection(Vector2.zero);
        yield return new WaitForFixedUpdate();
        Assert.That(Vector2.Distance(target,player.position),Is.LessThan(.1f),"Walking route must remain clear.");
    }

    private IEnumerator WalkOpen(string name)
    {
        // Route through the open floor using the keyboard movement motor, without teleporting.
        var target=(Vector2)GameObject.Find(name).transform.position+Vector2.left*1.5f;
        yield return Walk(new Vector2(player.position.x,-1.6f));
        yield return Walk(new Vector2(target.x,-1.6f));
        yield return Walk(target);
        yield return null;
        Assert.That(Station(name).IsWithinReach,Is.True);
        Station(name).HandleInput(true,false);
        Assert.That(Station(name).IsOpen,Is.True);
        yield return null;
    }

    private void AdvanceTo(DateTime date,double progress)
    {
        // Only calendar time is accelerated; walking and UI continue through real frames.
        root.Clock.Speed=SimulationSpeed.Normal;
        double days=(date-root.Clock.Date).TotalDays+progress-root.Clock.DayProgress;
        root.Clock.Advance(days*SimulationClock.SecondsPerDay);
        root.Clock.Speed=SimulationSpeed.Paused;
    }

    private static string Snapshot(CompanySession session)
    {
        var data=SessionSaveJson.Capture(session,"phase3-test");
        data.Timestamp="fixed";
        return SessionSaveJson.ToJson(data);
    }

    private IEnumerator SaveReload()
    {
        string expected=Snapshot(root.Session);
        var position=player.position;
        Click(GameObject.Find("SaveGame"));
        yield return null;
        Click(GameObject.Find("LoadGame"));
        yield return null;
        Assert.That(Snapshot(root.Session),Is.EqualTo(expected),"Partial days, IDs, finances and progression must round-trip exactly.");
        Assert.That(player.position,Is.EqualTo(position));
        Assert.That(Station("PCStation").IsOpen,Is.False);
        Assert.That(Station("HardwareStation").IsOpen,Is.False);
    }

    private static GameObject Buy(string id) => GameObject.Find("Upgrade-"+id).transform.Find("BuyUpgrade").gameObject;

    [UnityTest]
    public IEnumerator WalkHostGrowUpgradeAndResumeExactlyOneMonthlyPayment()
    {
        foreach(string name in new[]{"PCStation","HardwareStation"})
        {
            Station(name).HandleInput(true,false);
            Assert.That(Station(name).IsOpen,Is.False);
        }
        yield return WalkOpen("PCStation");
        foreach(var offer in root.Session.Catalog.Offers)
        {
            var row=GameObject.Find("Offer-"+offer.Id).transform;
            Click(row.Find("AcceptOffer").gameObject);
            Click(row.Find("AssignWorkload").gameObject);
        }
        AdvanceTo(new DateTime(1995,2,28),.75);
        yield return null;
        Assert.That(root.Session.Cash,Is.EqualTo(310m));
        Assert.That(root.Session.Hardware[0].Resources.Used.CpuCores,Is.EqualTo(2));
        Assert.That(root.Session.AcceptedContracts.All(x=>x.AppliedMilestoneIds.Count==0),Is.True);
        yield return SaveReload();
        int growthEvents=0;
        root.Session.MilestoneApplied+=(_,__)=>growthEvents++;
        AdvanceTo(new DateTime(1995,3,1),.25);
        yield return null;
        Assert.That(growthEvents,Is.EqualTo(1));
        Assert.That(root.Session.Cash,Is.EqualTo(470m));
        Assert.That(root.Session.OperatingProfit,Is.EqualTo(320m));
        Assert.That(root.Session.Hardware[0].Resources.Used.CpuCores,Is.EqualTo(3));
        Assert.That(Alerts,Does.Contain("CPU"));
        Station("PCStation").HandleInput(true,false);
        yield return null;
        Assert.That(GameObject.Find("CapacityDetails").GetComponent<Text>().text,Does.Contain("CPU 3/2 cores"));
        yield return SaveReload();
        Assert.That(Alerts,Does.Contain("CPU"));
        yield return WalkOpen("HardwareStation");
        Click(Buy("upgrade.starter-cpu"));
        Assert.That(root.Session.Cash,Is.EqualTo(350m));
        Assert.That(root.Session.OperatingProfit,Is.EqualTo(320m));
        Assert.That(root.Session.Hardware[0].Resources.Capacity.CpuCores,Is.EqualTo(4));
        Assert.That(Alerts,Is.EqualTo("NO CAPACITY ALERTS"));
        AdvanceTo(new DateTime(1995,3,31),.75);
        yield return SaveReload();
        int repeatedGrowth=0;
        root.Session.MilestoneApplied+=(_,__)=>repeatedGrowth++;
        Assert.That(root.Session.Hardware[0].AppliedUpgradeIds.Single(),Is.EqualTo("upgrade.starter-cpu"));
        Assert.That(root.Session.AcceptedContracts.Single(x=>x.Definition.Id=="offer.photosnap").AppliedMilestoneIds.Single(),Is.EqualTo("growth.photosnap.cpu-2"));
        yield return WalkOpen("HardwareStation");
        Assert.That(Buy("upgrade.starter-cpu").GetComponent<Button>().interactable,Is.False);
        Click(Buy("upgrade.starter-ram"));
        Assert.That(root.Session.Cash,Is.EqualTo(270m));
        // Restore the CPU-only checkpoint before checking the next complete billing cycle.
        Click(GameObject.Find("LoadGame"));
        yield return null;
        root.Session.MilestoneApplied+=(_,__)=>repeatedGrowth++;
        AdvanceTo(new DateTime(1995,4,1),.25);
        yield return null;
        Assert.That(root.Session.Cash,Is.EqualTo(510m));
        Assert.That(root.Session.OperatingProfit,Is.EqualTo(480m));
        Assert.That(root.Session.BillingHistory.Count,Is.EqualTo(3));
        Assert.That(root.Session.AcceptedContracts.All(x=>x.PaidMonths==3),Is.True);
        Assert.That(root.Session.Hardware[0].PurchaseRecords.Count,Is.EqualTo(1));
        Assert.That(repeatedGrowth,Is.Zero);
        Assert.That(root.Session.TryBillMonth(new DateTime(1995,3,31),out _),Is.False);
        yield return WalkOpen("PCStation");
        Assert.That(GameObject.Find("ResourceUsage").GetComponent<Text>().text,Does.Contain("CPU 3/4 cores"));
        Assert.That(GameObject.Find("Offer-offer.photosnap").transform.Find("OfferState").GetComponent<Text>().text,Does.Contain("Growth"));
        foreach(var size in new[]{new Vector2Int(800,600),new Vector2Int(1280,720)})
        {
            Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);
            yield return null;yield return null;
            ScreenCapture.CaptureScreenshot(Path.Combine(Application.temporaryCachePath,$"Phase3-Resumed-{size.x}x{size.y}.png"));
            for(int i=0;i<10;i++) yield return null;
        }
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator FreshCompanyRejectsRamAfterCpuWithoutChangingState()
    {
        Assert.That(Station("PCStation").TryOpen(),Is.False);
        Assert.That(Station("HardwareStation").TryOpen(),Is.False);
        yield return WalkOpen("HardwareStation");
        Click(Buy("upgrade.starter-cpu"));
        Assert.That(root.Session.Cash,Is.EqualTo(30m));
        Assert.That(root.Session.OperatingProfit,Is.Zero);
        string before=Snapshot(root.Session);
        var ram=Buy("upgrade.starter-ram").GetComponent<Button>();
        Assert.That(ram.interactable,Is.False);
        ram.onClick.Invoke();
        Assert.That(Snapshot(root.Session),Is.EqualTo(before));
        Assert.That(GameObject.Find("HardwareFeedback").GetComponent<Text>().text,Does.Contain("Not enough cash"));
    }

    [UnityTest]
    public IEnumerator LegacyMigrationAllowsRestoredStationPurchasesAndNextPayment()
    {
        foreach(var offer in root.Session.Catalog.Offers)
        {
            root.Session.TryAcceptOffer(offer.Id,out var contract,out _);
            root.Session.TryAssignContract(contract.Id,root.Session.Hardware[0].Id,out _);
        }
        AdvanceTo(new DateTime(1995,3,1),.75);
        var legacy=SessionSaveJson.Capture(root.Session,"phase2-legacy");
        legacy.SaveVersion=1;
        foreach(var hardware in legacy.Hardware) hardware.Purchases=null;
        foreach(var contract in legacy.Contracts) contract.AppliedMilestoneIds=null;
        string json=SessionSaveJson.ToJson(legacy);
        File.WriteAllText(path,json);
        Click(GameObject.Find("LoadGame"));
        yield return null;
        Assert.That(File.ReadAllText(path),Is.EqualTo(json));
        Assert.That(root.Clock.DayProgress,Is.EqualTo(.75));
        Assert.That(root.Session.Hardware[0].Resources.Used.CpuCores,Is.EqualTo(3));
        Assert.That(Alerts,Does.Contain("CPU"));
        yield return WalkOpen("HardwareStation");
        Click(Buy("upgrade.starter-cpu"));
        yield return SaveReload();
        Assert.That(File.ReadAllText(path),Does.Contain("\"save_version\":2"));
        AdvanceTo(new DateTime(1995,4,1),.25);
        yield return null;
        Assert.That(root.Session.Cash,Is.EqualTo(510m));
        Assert.That(root.Session.BillingHistory.Count,Is.EqualTo(3));
        yield return WalkOpen("PCStation");
        Assert.That(GameObject.Find("ResourceUsage").GetComponent<Text>().text,Does.Contain("CPU 3/4 cores"));
        Assert.That(Alerts,Is.EqualTo("NO CAPACITY ALERTS"));
    }

    private static void Click(GameObject obj)
    {
        Assert.That(obj,Is.Not.Null);
        var button=obj.GetComponent<Button>();
        Assert.That(button.interactable,Is.True);
        Canvas.ForceUpdateCanvases();
        var rect=(RectTransform)obj.transform;
        var pointer=new PointerEventData(EventSystem.current)
        {
            button=PointerEventData.InputButton.Left,
            position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center))
        };
#if !UNITY_EDITOR
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
        Assert.That(hits.Count,Is.GreaterThan(0),obj.name+" must have a pointer hit target.");
        Assert.That(hits[0].gameObject,Is.EqualTo(obj));
#endif
        ExecuteEvents.Execute(obj,pointer,ExecuteEvents.pointerClickHandler);
    }
}
