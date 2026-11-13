using System.Collections;
using System.Collections.Generic;
using System.IO;
using InfrastructureTycoon;
using InfrastructureTycoon.Simulation;
using InfrastructureTycoon.Content;
using InfrastructureTycoon.UI;
using InfrastructureTycoon.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class CapacityFeedbackTests
{
    private GameRoot root;
    private string path;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        root = Object.FindFirstObjectByType<GameRoot>();
        root.Clock.Speed = SimulationSpeed.Paused;
        GameObject.Find("Player").GetComponent<PlayerKeyboardInput>().enabled = false;
        path = Path.Combine(Application.temporaryCachePath,"capacity-feedback-test.json");
        root.ConfigureSaveSlot(new SessionSaveSlot(path));
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (File.Exists(path)) File.Delete(path);
        yield return null;
    }

    private static string Alerts => GameObject.Find("AlertsLabel").GetComponent<Text>().text;

    private void HostBoth()
    {
        foreach (var offer in root.Session.Catalog.Offers)
        {
            root.Session.TryAcceptOffer(offer.Id,out var contract,out _);
            root.Session.TryAssignContract(contract.Id,root.Session.Hardware[0].Id,out _);
        }
    }

    private void Advance(double seconds)
    {
        root.Clock.Speed = SimulationSpeed.Normal;
        root.Clock.Advance(seconds);
        root.Clock.Speed = SimulationSpeed.Paused;
    }

    private IEnumerator Open(string name)
    {
        PcStationInteraction.CloseAll();
        var player = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        player.position = (Vector2)GameObject.Find(name).transform.position + Vector2.left * 1.5f;
        player.linearVelocity = Vector2.zero;
        for (int i=0;i<2;i++) yield return new WaitForFixedUpdate();
        yield return null;
        var interaction = GameObject.Find(name == "PCStation" ? "PcStationInteraction" : "HardwareStationInteraction").GetComponent<PcStationInteraction>();
        Assert.That(interaction.TryOpen(),Is.True);
        yield return null;
    }

    private static void Click(Button button)
    {
        Assert.That(button.interactable,Is.True);
        Canvas.ForceUpdateCanvases();
        var rect = (RectTransform)button.transform;
        var pointer = new PointerEventData(EventSystem.current)
        {
            button = PointerEventData.InputButton.Left,
            position = RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center))
        };
#if !UNITY_EDITOR
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer,hits);
        Assert.That(hits[0].gameObject,Is.EqualTo(button.gameObject));
#endif
        ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
    }

    private static Button Purchase(string id) => GameObject.Find("Upgrade-"+id).transform.Find("BuyUpgrade").GetComponent<Button>();

    private static System.Collections.ObjectModel.ReadOnlyCollection<ResourceBottleneck> AllConstraints()
    {
        var hardware=new HardwareInstance(new HardwareData("pc","Starter PC",new ResourceAmounts(2,2,2,2),20));
        var offer=new OfferData("web","Customer",new ResourceAmounts(1,1,1,1),100);
        var growth=new GrowthMilestoneData("growth","Growth",offer.Id,2,new ResourceAmounts(3.5,4,5,6));
        var contract=new ContractInstance(offer,"contract",2,new[]{growth.Id},new[]{growth},hardware.Id);
        return ResourceBottlenecks.Diagnose(hardware,new[]{contract});
    }

    [Test]
    public void AllResourceNamesUnitsAndShortfallsAreFormattedWithoutLosingConstraints()
    {
        var reports=AllConstraints();
        Assert.That(CapacityFeedback.Summary(reports),Is.EqualTo("CAPACITY ALERTS: CPU, RAM, Storage, Bandwidth"));
        var details=CapacityFeedback.Details(reports);
        Assert.That(details,Does.Contain("CPU 3.5/2 cores (short 1.5 cores)"));
        Assert.That(details,Does.Contain("RAM 4/2 GB (short 2 GB)"));
        Assert.That(details,Does.Contain("Storage 5/2 GB (short 3 GB)"));
        Assert.That(details,Does.Contain("Bandwidth 6/2 Mbps (short 4 Mbps)"));
        Assert.That(details,Does.Contain("Affected: Customer"));
        Assert.That(details.Split('\n').Length,Is.EqualTo(5));
    }

    [UnityTest]
    public IEnumerator GrowthUpgradeAndReloadReconstructCurrentAlertsWithoutReplayingGrowth()
    {
        Assert.That(Alerts,Is.EqualTo("NO CAPACITY ALERTS"));
        HostBoth();
        int growthEvents=0;
        root.Session.MilestoneApplied += (_,__) => growthEvents++;
        Advance(62);
        yield return null;
        Assert.That(Alerts,Is.EqualTo("NO CAPACITY ALERTS"));
        Advance(56);
        yield return null;
        Assert.That(growthEvents,Is.EqualTo(1));
        Assert.That(Alerts,Does.Contain("CPU"));
        yield return Open("PCStation");
        var details=GameObject.Find("CapacityDetails").GetComponent<Text>().text;
        Assert.That(details,Does.Contain("Starter PC").And.Contain("CPU 3/2 cores").And.Contain("short 1 cores"));
        Assert.That(details,Does.Contain("PhotoSnap").And.Contain("Local Business Website"));
        var state=GameObject.Find("Offer-offer.photosnap").transform.Find("OfferState").GetComponent<Text>().text;
        Assert.That(state,Does.Contain("Growth").And.Contain("CPU 1 -> 2 cores").And.Contain("2 paid months"));
        root.RequestSave();
        yield return null;
        yield return Open("HardwareStation");
        Click(Purchase("upgrade.starter-ram"));
        Assert.That(Alerts,Does.Contain("CPU"));
        Click(Purchase("upgrade.starter-cpu"));
        Assert.That(Alerts,Is.EqualTo("NO CAPACITY ALERTS"));
        root.RequestLoad();
        yield return null;
        Assert.That(Alerts,Does.Contain("CPU"));
        Assert.That(root.Session.Cash,Is.EqualTo(470m));
        Assert.That(root.Session.BillingHistory.Count,Is.EqualTo(2));
        Assert.That(growthEvents,Is.EqualTo(1));
        yield return Open("PCStation");
        Assert.That(GameObject.Find("CapacityDetails").GetComponent<Text>().text,Does.Contain("CPU 3/2 cores"));
        Assert.That(GameObject.Find("Offer-offer.photosnap").transform.Find("OfferState").GetComponent<Text>().text,Does.Contain("CPU 1 -> 2 cores"));
        yield return Open("HardwareStation");
        Click(Purchase("upgrade.starter-cpu"));
        root.RequestSave();
        yield return null;
        root.RequestLoad();
        yield return null;
        Assert.That(Alerts,Is.EqualTo("NO CAPACITY ALERTS"));
        Assert.That(root.Session.Cash,Is.EqualTo(350m));
        Assert.That(root.Session.BillingHistory.Count,Is.EqualTo(2));
        yield return Open("PCStation");
        Assert.That(GameObject.Find("CapacityDetails").GetComponent<Text>().text,Is.EqualTo("No capacity alerts."));
        Assert.That(GameObject.Find("ResourceUsage").GetComponent<Text>().text,Does.Contain("CPU 3/4 cores"));
        Assert.That(GameObject.Find("Offer-offer.photosnap").transform.Find("OfferState").GetComponent<Text>().text,Does.Contain("CPU 1 -> 2 cores"));
    }

    [UnityTest]
    public IEnumerator OverloadedHostingFeedbackFitsAndHudControlsRemainAccessible()
    {
        HostBoth();
        Advance(118);
        yield return Open("PCStation");
        Assert.That(GameObject.Find("CapacityDetails"),Is.Not.Null);
        foreach (var size in new[] {new Vector2Int(800,600),new Vector2Int(1280,720)})
        {
            Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);
            yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();
            var panel=(RectTransform)GameObject.Find("PcPanel").transform;
            var bounds=new Vector3[4];
            panel.GetWorldCorners(bounds);
            foreach(var text in panel.GetComponentsInChildren<Text>())
            {
                Assert.That(text.preferredHeight,Is.LessThanOrEqualTo(text.rectTransform.rect.height+1),text.name);
                var corners=new Vector3[4];text.rectTransform.GetWorldCorners(corners);
                foreach(var point in corners)
                {
                    Assert.That(point.x,Is.InRange(bounds[0].x-1,bounds[2].x+1),text.name);
                    Assert.That(point.y,Is.InRange(bounds[0].y-1,bounds[2].y+1),text.name);
                }
            }
            var alerts=GameObject.Find("AlertsLabel").GetComponent<Text>();
            Assert.That(alerts.preferredWidth,Is.LessThanOrEqualTo(alerts.rectTransform.rect.width));
            // Ensure simultaneous constraints also fit the same presentation region.
            var detailText=GameObject.Find("CapacityDetails").GetComponent<Text>();
            var detailBounds=new Vector3[4];detailText.rectTransform.GetWorldCorners(detailBounds);
            foreach(Transform row in GameObject.Find("HostingRows").transform)
            {
                var rowBounds=new Vector3[4];((RectTransform)row).GetWorldCorners(rowBounds);
                Assert.That(rowBounds[0].y,Is.GreaterThanOrEqualTo(detailBounds[1].y+2),"Contract rows must remain above diagnostic text.");
            }
            var monthlyBounds=new Vector3[4];
            ((RectTransform)GameObject.Find("MonthlyResult").transform).GetWorldCorners(monthlyBounds);
            Assert.That(detailBounds[0].y,Is.GreaterThanOrEqualTo(monthlyBounds[1].y),"Diagnostics must remain above monthly finances.");
            string originalDetails=detailText.text;
            string originalSummary=alerts.text;
            detailText.text=CapacityFeedback.Details(AllConstraints());
            alerts.text=CapacityFeedback.Summary(AllConstraints());
            Canvas.ForceUpdateCanvases();
            Assert.That(detailText.preferredHeight,Is.LessThanOrEqualTo(detailText.rectTransform.rect.height+1));
            Assert.That(alerts.preferredWidth,Is.LessThanOrEqualTo(alerts.rectTransform.rect.width));
            detailText.text=originalDetails;
            alerts.text=originalSummary;
#if !UNITY_EDITOR
            foreach(string name in new[] {"Pause","Speed1x","Speed2x","Speed4x","Speed8x","SaveGame","LoadGame","ClosePcPanel"})
            {
                var button=GameObject.Find(name).GetComponent<Button>();
                var rect=(RectTransform)button.transform;
                var pointer=new PointerEventData(EventSystem.current) {position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center))};
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
                Assert.That(hits[0].gameObject,Is.EqualTo(button.gameObject));
            }
#endif
            ScreenCapture.CaptureScreenshot(Path.Combine(Application.temporaryCachePath,$"Capacity-{size.x}x{size.y}.png"));
            for(int i=0;i<10;i++) yield return null;
        }
    }
}
