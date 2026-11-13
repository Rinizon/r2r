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

public class PcStationTests
{
    private PcStationInteraction station;
    private Rigidbody2D player;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        station = Object.FindFirstObjectByType<PcStationInteraction>();
        Assert.That(station, Is.Not.Null);
        player = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        player.GetComponent<PlayerKeyboardInput>().enabled = false;
        yield return null;
    }

    private IEnumerator WalkToStation(bool near)
    {
        player.position = (Vector2)GameObject.Find("PCStation").transform.position + new Vector2(near ? -1.5f : -3f, 0);
        player.linearVelocity = Vector2.zero;
        for (int i = 0; i < 2; i++) yield return new WaitForFixedUpdate();
        yield return null;
    }

    [UnityTest]
    public IEnumerator ReachGatesOpeningAndLeavingClosesUntilExplicitReentryInteraction()
    {
        var root = GameObject.Find("GameRoot").GetComponent<InfrastructureTycoon.GameRoot>();
        var session = root.Session;
        var clock = root.Clock;
        Assert.That(station.IsOpen, Is.False);
        Assert.That(station.TryOpen(), Is.False);
        station.HandleInput(true, false);
        Assert.That(station.IsOpen, Is.False);
        yield return WalkToStation(true);
        Assert.That(station.IsWithinReach, Is.True);
        Assert.That(GameObject.Find("InteractionPrompt").GetComponent<Text>().text, Does.Contain("E"));
        station.HandleInput(true, false);
        Assert.That(station.IsOpen, Is.True);
        Assert.That(GameObject.Find("InteractionPrompt"), Is.Null);
        Assert.That(station.TryOpen(), Is.False, "An open panel must consume further station actions.");
        station.HandleInput(true, false);
        Assert.That(station.IsOpen, Is.True);
        yield return WalkToStation(false);
        Assert.That(station.IsWithinReach, Is.False);
        Assert.That(station.IsOpen, Is.False);
        Assert.That(GameObject.Find("InteractionPrompt"), Is.Null);
        yield return WalkToStation(true);
        Assert.That(station.IsOpen, Is.False);
        Assert.That(station.TryOpen(), Is.True);
        Assert.That(root.Session, Is.SameAs(session));
        Assert.That(root.Clock, Is.SameAs(clock));
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator CloseButtonAndEscapeCloseWithoutReopeningBehindUi()
    {
        yield return WalkToStation(true);
        Assert.That(station.TryOpen(), Is.True);
        yield return null;
        Canvas.ForceUpdateCanvases();
        var button = GameObject.Find("ClosePcPanel").GetComponent<Button>();
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
        Assert.That(station.IsOpen, Is.False);
        Assert.That(station.TryOpen(), Is.True);
        station.HandleInput(true, true);
        Assert.That(station.IsOpen, Is.False, "Escape must take priority over interaction.");
        yield return null;
        Assert.That(GameObject.Find("InteractionPrompt"), Is.Not.Null);
    }

    [UnityTest]
    public IEnumerator FocusedTextInputConsumesTheInteractionKey()
    {
        yield return WalkToStation(true);
        var fieldObject = new GameObject("TestInput", typeof(RectTransform), typeof(InputField));
        fieldObject.transform.SetParent(GameObject.Find("HUD").transform, false);
        var textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(fieldObject.transform, false);
        var text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        fieldObject.GetComponent<InputField>().textComponent = text;
        EventSystem.current.SetSelectedGameObject(fieldObject);
        station.HandleInput(true, false);
        Assert.That(station.IsOpen, Is.False);
        EventSystem.current.SetSelectedGameObject(null);
        Assert.That(station.TryOpen(), Is.True);
        Object.Destroy(fieldObject);
    }

    [UnityTest]
    public IEnumerator PanelFitsBelowHudAtRequiredWindowSizesAndTimeControlsRemainReachable()
    {
        yield return WalkToStation(true);
        Assert.That(station.TryOpen(), Is.True);
        foreach (var size in new[] { new Vector2Int(800, 600), new Vector2Int(1280, 720) })
        {
            Screen.SetResolution(size.x, size.y, FullScreenMode.Windowed);
            yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();
            var panel = (RectTransform)GameObject.Find("PcPanel").transform;
            var top = new Vector3[4];
            var bottom = new Vector3[4];
            ((RectTransform)GameObject.Find("TopBar").transform).GetWorldCorners(top);
            ((RectTransform)GameObject.Find("StatusBar").transform).GetWorldCorners(bottom);
            foreach (var text in panel.GetComponentsInChildren<Text>())
            {
                var corners = new Vector3[4];
                text.rectTransform.GetWorldCorners(corners);
                foreach (var corner in corners)
                {
                    Assert.That(corner.x, Is.InRange(0, Screen.width));
                    Assert.That(corner.y, Is.InRange(bottom[1].y, top[0].y));
                }
            }
            var pause = GameObject.Find("Pause").GetComponent<Button>();
#if !UNITY_EDITOR
            var pauseRect = (RectTransform)pause.transform;
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = RectTransformUtility.WorldToScreenPoint(null, pauseRect.TransformPoint(pauseRect.rect.center))
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.That(hits.Count, Is.GreaterThan(0));
            Assert.That(hits[0].gameObject, Is.EqualTo(pause.gameObject), "Panel must not block the time controls.");
#endif
            pause.onClick.Invoke();
            Assert.That(GameObject.Find("SpeedLabel").GetComponent<Text>().text, Is.EqualTo("SPEED  PAUSED"));
            Assert.That(station.IsOpen, Is.True);
            ScreenCapture.CaptureScreenshot(Path.Combine(Application.temporaryCachePath, $"PcPanel-{size.x}x{size.y}.png"));
            for (int i = 0; i < 10; i++) yield return null;
        }
        LogAssert.NoUnexpectedReceived();
    }
}
