using System.Collections;
using System.Collections.Generic;
using System.IO;
using InfrastructureTycoon.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class HostingUiTests
{
    private InfrastructureTycoon.GameRoot root;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        root = GameObject.Find("GameRoot").GetComponent<InfrastructureTycoon.GameRoot>();
        root.Clock.Speed = InfrastructureTycoon.Simulation.SimulationSpeed.Paused;
    }

    private IEnumerator OpenStation()
    {
        var station = Object.FindFirstObjectByType<PcStationInteraction>();
        Assert.That(station.TryOpen(), Is.False);
        var player = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        player.GetComponent<PlayerKeyboardInput>().enabled = false;
        player.position = (Vector2)GameObject.Find("PCStation").transform.position + new Vector2(-1.5f, 0);
        for (int i = 0; i < 2; i++) yield return new WaitForFixedUpdate();
        Assert.That(station.TryOpen(), Is.True);
        yield return null;
        yield return null;
        Canvas.ForceUpdateCanvases();
    }

    [UnityTest]
    public IEnumerator UiHostsBothWebsitesAndDisplaysUsageAndMonthlyFinances()
    {
        Assert.That(GameObject.Find("CashLabel").GetComponent<Text>().text, Is.EqualTo("CASH  $150"));
        yield return OpenStation();
        foreach (var offer in root.Session.Catalog.Offers)
        {
            var row = GameObject.Find("Offer-" + offer.Id).transform;
            Assert.That(row.Find("OfferName").GetComponent<Text>().text, Does.Contain(offer.DisplayName));
            Assert.That(row.Find("OfferName").GetComponent<Text>().text, Does.Contain("$" + offer.MonthlyRevenue));
            Click(row.Find("AcceptOffer").GetComponent<Button>());
            Assert.That(row.Find("AssignWorkload").GetComponent<Button>().interactable, Is.True);
            Assert.That(row.Find("OfferState").GetComponent<Text>().text, Does.Contain("UNASSIGNED"));
            Click(row.Find("AssignWorkload").GetComponent<Button>());
            Assert.That(row.Find("OfferState").GetComponent<Text>().text, Does.Contain("HOSTED"));
        }
        Assert.That(root.Session.AcceptedContracts.Count, Is.EqualTo(2));
        Assert.That(GameObject.Find("ResourceUsage").GetComponent<Text>().text, Is.EqualTo("STARTER PC   CPU 2/2 cores   |   RAM 3/4 GB\nSTORAGE 30/250 GB   |   BANDWIDTH 8/10 Mbps"));
        Assert.That(root.Session.Cash, Is.EqualTo(150m));
        root.Clock.Speed = InfrastructureTycoon.Simulation.SimulationSpeed.Normal;
        root.Clock.Advance(62);
        root.Clock.Speed = InfrastructureTycoon.Simulation.SimulationSpeed.Paused;
        yield return null;
        Assert.That(GameObject.Find("CashLabel").GetComponent<Text>().text, Is.EqualTo("CASH  $310"));
        var billing = GameObject.Find("MonthlyResult").GetComponent<Text>().text;
        Assert.That(billing, Does.Contain("JAN 1995"));
        Assert.That(billing, Does.Contain("Revenue $180").And.Contain("Expenses $20").And.Contain("Net +$160"));
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator DisabledActionsAndQueuedDuplicateAcceptancePreserveState()
    {
        yield return OpenStation();
        var row = GameObject.Find("Offer-offer.photosnap").transform;
        var accept = row.Find("AcceptOffer").GetComponent<Button>();
        var assign = row.Find("AssignWorkload").GetComponent<Button>();
        Assert.That(assign.interactable, Is.False);
        Click(accept);
        Assert.That(accept.interactable, Is.False);
        // Simulate a queued second callback; session validation remains authoritative.
        accept.onClick.Invoke();
        Assert.That(root.Session.AcceptedContracts.Count, Is.EqualTo(1));
        Assert.That(GameObject.Find("HostingFeedback").GetComponent<Text>().text, Does.Contain("already"));
        Assert.That(root.Session.Hardware[0].Resources.Used.CpuCores, Is.Zero);
        Click(assign);
        Assert.That(assign.interactable, Is.False);
        Assert.That(root.Session.Cash, Is.EqualTo(150m));
    }

    [UnityTest]
    public IEnumerator OffersAndHostedStateRemainReadableAtRequiredSizes()
    {
        yield return OpenStation();
        foreach (var size in new[] { new Vector2Int(800, 600), new Vector2Int(1280, 720) })
        {
            Screen.SetResolution(size.x, size.y, FullScreenMode.Windowed);
            yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();
            var panel = (RectTransform)GameObject.Find("PcPanel").transform;
            var bounds = new Vector3[4];
            panel.GetWorldCorners(bounds);
            foreach (var text in panel.GetComponentsInChildren<Text>())
            {
                var corners = new Vector3[4];
                text.rectTransform.GetWorldCorners(corners);
                foreach (var corner in corners)
                {
                    Assert.That(corner.x, Is.InRange(bounds[0].x - 1, bounds[2].x + 1), text.name);
                    Assert.That(corner.y, Is.InRange(bounds[0].y - 1, bounds[2].y + 1), text.name);
                }
                Assert.That(text.preferredHeight, Is.LessThanOrEqualTo(text.rectTransform.rect.height + 1), text.name);
            }
            ScreenCapture.CaptureScreenshot(Path.Combine(Application.temporaryCachePath, $"Hosting-{size.x}x{size.y}.png"));
            for (int i = 0; i < 10; i++) yield return null;
        }
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
        Assert.That(hits.Count, Is.GreaterThan(0));
        Assert.That(hits[0].gameObject, Is.EqualTo(button.gameObject));
#endif
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
    }
}
