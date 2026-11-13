using System.Collections;
using System.Collections.Generic;
using System.IO;
using InfrastructureTycoon;
using InfrastructureTycoon.Simulation;
using InfrastructureTycoon.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class UpgradeUiTests
{
    private GameRoot root;

    private IEnumerator Fresh()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        root = Object.FindFirstObjectByType<GameRoot>();
        root.Clock.Speed = SimulationSpeed.Paused;
        GameObject.Find("Player").GetComponent<PlayerKeyboardInput>().enabled = false;
    }

    private IEnumerator Open(string stationName)
    {
        PcStationInteraction.CloseAll();
        var player = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        player.position = (Vector2)GameObject.Find(stationName).transform.position + Vector2.left * 1.5f;
        player.linearVelocity = Vector2.zero;
        for (int i = 0; i < 2; i++) yield return new WaitForFixedUpdate();
        yield return null;
        string interaction = stationName == "PCStation" ? "PcStationInteraction" : "HardwareStationInteraction";
        Assert.That(GameObject.Find(interaction).GetComponent<PcStationInteraction>().TryOpen(), Is.True);
        yield return null;
        Canvas.ForceUpdateCanvases();
    }

    private static Button Buy(string id)
    {
        var row = GameObject.Find("Upgrade-" + id);
        Assert.That(row, Is.Not.Null, "Hardware panel must list catalog upgrades.");
        return row.transform.Find("BuyUpgrade").GetComponent<Button>();
    }

    private static void Click(Button button)
    {
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
        Assert.That(hits[0].gameObject, Is.EqualTo(button.gameObject));
#endif
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
    }

    [UnityTest]
    public IEnumerator EachUpgradePurchasesOnceAndUnaffordableQueuedActionIsRejected()
    {
        foreach (string id in new[] {"upgrade.starter-cpu", "upgrade.starter-ram", "upgrade.starter-storage", "upgrade.starter-internet"})
        {
            yield return Fresh();
            yield return Open("HardwareStation");
            var upgrade = System.Linq.Enumerable.Single(root.Session.Catalog.Upgrades, x => x.Id == id);
            Click(Buy(id));
            Assert.That(root.Session.Cash, Is.EqualTo(150m - upgrade.Price));
            Assert.That(root.Session.Hardware[0].PurchaseRecords.Count, Is.EqualTo(1));
            var capacity = root.Session.Hardware[0].EffectiveCapacity;
            Assert.That(capacity.CpuCores, Is.EqualTo(2 + upgrade.CapacityDelta.CpuCores));
            Assert.That(capacity.RamGb, Is.EqualTo(4 + upgrade.CapacityDelta.RamGb));
            Assert.That(capacity.StorageGb, Is.EqualTo(250 + upgrade.CapacityDelta.StorageGb));
            Assert.That(capacity.BandwidthMbps, Is.EqualTo(10 + upgrade.CapacityDelta.BandwidthMbps));
            Assert.That(Buy(id).interactable, Is.False);
            Buy(id).onClick.Invoke();
            Assert.That(root.Session.Hardware[0].PurchaseRecords.Count, Is.EqualTo(1));
            if (id == "upgrade.starter-cpu")
            {
                Assert.That(root.Session.Cash, Is.EqualTo(30m));
                Assert.That(GameObject.Find("HardwareCash").GetComponent<Text>().text, Does.Contain("$30"));
                Assert.That(Buy("upgrade.starter-ram").interactable, Is.False);
                Buy("upgrade.starter-ram").onClick.Invoke();
                Assert.That(GameObject.Find("HardwareFeedback").GetComponent<Text>().text, Does.Contain("Not enough cash"));
                Assert.That(root.Session.Cash, Is.EqualTo(30m));
                Assert.That(root.Session.Hardware[0].PurchaseRecords.Count, Is.EqualTo(1));
                Assert.That(root.Session.Hardware[0].EffectiveCapacity.RamGb, Is.EqualTo(4));
                Assert.That(root.Session.OperatingProfit, Is.Zero);
            }
        }
    }

    [UnityTest]
    public IEnumerator RepeatedLoadRebindsPurchaseActionsAndEffectiveHostingViews()
    {
        yield return Fresh();
        foreach (var offer in root.Session.Catalog.Offers)
        {
            root.Session.TryAcceptOffer(offer.Id, out var contract, out _);
            root.Session.TryAssignContract(contract.Id,root.Session.Hardware[0].Id,out _);
        }
        root.Clock.Speed = SimulationSpeed.Normal;
        root.Clock.Advance(118);
        root.Clock.Speed = SimulationSpeed.Paused;
        yield return Open("HardwareStation");
        Click(Buy("upgrade.starter-cpu"));
        var path = Path.Combine(Application.temporaryCachePath,"upgrade-ui-test.json");
        root.ConfigureSaveSlot(new SessionSaveSlot(path));
        root.RequestSave();
        yield return null;
        var old = root.Session;
        Click(Buy("upgrade.starter-ram"));
        for (int i = 0; i < 3; i++)
        {
            root.RequestLoad();
            yield return null;
            yield return Open("HardwareStation");
            Assert.That(Buy("upgrade.starter-cpu").interactable, Is.False);
            Assert.That(Buy("upgrade.starter-ram").interactable, Is.True);
            Assert.That(GameObject.Find("HardwareRows").transform.childCount, Is.EqualTo(4));
        }
        decimal cash = root.Session.Cash;
        Click(Buy("upgrade.starter-ram"));
        Assert.That(root.Session.Cash, Is.EqualTo(cash - 80));
        Assert.That(root.Session.Hardware[0].PurchaseRecords.Count, Is.EqualTo(2));
        Assert.That(old.Cash, Is.EqualTo(root.Session.Cash));
        yield return Open("PCStation");
        Assert.That(GameObject.Find("ResourceUsage").GetComponent<Text>().text, Does.Contain("CPU 3/4 cores").And.Contain("RAM 3/8 GB"));
        var row = GameObject.Find("Offer-offer.photosnap");
        Assert.That(row.transform.Find("Requirements").GetComponent<Text>().text, Does.Contain("CPU 2 cores"));
        root.RequestLoad();
        yield return null;
        yield return Open("PCStation");
        Assert.That(GameObject.Find("ResourceUsage").GetComponent<Text>().text, Does.Contain("RAM 3/4 GB"));
        Assert.That(GameObject.Find("Offer-offer.photosnap").transform.Find("Requirements").GetComponent<Text>().text, Does.Contain("CPU 2 cores"));
        File.Delete(path);
    }

    [UnityTest]
    public IEnumerator PurchaseRowsFitAndButtonsHaveRealHitTargetsAtRequiredSizes()
    {
        yield return Fresh();
        yield return Open("HardwareStation");
        foreach (var size in new[] {new Vector2Int(800,600),new Vector2Int(1280,720)})
        {
            Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);
            yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();
            var panel = (RectTransform)GameObject.Find("HardwarePanel").transform;
            var bounds = new Vector3[4];
            panel.GetWorldCorners(bounds);
            foreach (var text in panel.GetComponentsInChildren<Text>())
            {
                Assert.That(text.preferredHeight, Is.LessThanOrEqualTo(text.rectTransform.rect.height + 1),text.name);
                var corners = new Vector3[4];
                text.rectTransform.GetWorldCorners(corners);
                foreach (var corner in corners)
                {
                    Assert.That(corner.x,Is.InRange(bounds[0].x-1,bounds[2].x+1),text.name);
                    Assert.That(corner.y,Is.InRange(bounds[0].y-1,bounds[2].y+1),text.name);
                }
            }
            foreach(var upgrade in root.Session.Catalog.Upgrades)
            {
                var button=Buy(upgrade.Id);
#if !UNITY_EDITOR
                var rect=(RectTransform)button.transform;
                var pointer=new PointerEventData(EventSystem.current) {position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center))};
                var hits=new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer,hits);
                Assert.That(hits[0].gameObject,Is.EqualTo(button.gameObject));
#endif
            }
            ScreenCapture.CaptureScreenshot(Path.Combine(Application.temporaryCachePath,$"Upgrades-{size.x}x{size.y}.png"));
            for(int i=0;i<10;i++) yield return null;
        }
    }
}
